using System;
using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[Serializable]
public class PoissonDiscSampler
{
    //todo: replace with my own random number generator for the sake of 
    //making save files smaller by just including the seed number.
    //that is, if it can be deterministic thru later on
    [HideInInspector]
    public Unity.Mathematics.Random random = new Unity.Mathematics.Random();

    public bool drawGizmos = false;

    private int[,] grid;
    [SerializeField]
    private Vector2[] points;
    private float cellSize;
    private float r;
    private int N;
    private int gridWidth;
    private int gridHeight;

    private int currentIndex = 0;
    private int domainSizeX => domain.ScaleX;
    private int domainSizeY => domain.ScaleY;

    private bool hasData = false;
    private Data cachedData;
    private MapDomain domain;

    public PoissonDiscSampler(MapDomain domain)
    {
        this.domain = domain;
    }

    /// <summary>
    /// Generates point data via poisson disc sampling.
    /// </summary>
    /// <param name="r">Minimum radius between samples.</param>
    /// <param name="N">Total samples to generate. Leave as -1 to fill available space.</param>
    /// <param name="k">Retries around a single sample.</param>
    public void Generate(float r = 1, int N = -1, int k = 30)
    {
        if (hasData) return;

        random.InitState();

        this.r = r;
        this.N = N;

        if (N <= 0)
        {
            var newN = (domainSizeX / r) * (domainSizeY / r);

            this.N = Mathf.RoundToInt(newN);
        }

        //cell size bounded by r/sqrt(n=2)
        cellSize = r / math.sqrt(2f);

        gridWidth = Mathf.CeilToInt(domainSizeX / cellSize);
        gridHeight = Mathf.CeilToInt(domainSizeY / cellSize);

        //initialized as this because each cell can hold only one sample,
        //due to the above cell size calculation.
        //hold indexes
        grid = new int[gridWidth, gridHeight];

        points = new Vector2[this.N];

        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                grid[i, j] = -1;
            }
        }

        Sample(k);
        AddPointsToDomainCorners();
        hasData = true;
    }

    public void AddPointsToDomainCorners()
    {
        AddPoint(domain.topLeft);
        AddPoint(domain.topRight);
        AddPoint(domain.bottomLeft);
        AddPoint(domain.bottomRight);
    }

    public void ClearData()
    {
        hasData = false;
        points = null;
        grid = null;
        cachedData = null;
    }

    private int AddPoint(Vector2 point)
    {
        int i = Mathf.FloorToInt(point.x / cellSize);
        int j = Mathf.FloorToInt(point.y / cellSize);

        var index = currentIndex;
        points[index] = point;
        grid[i, j] = index;
        currentIndex++;
        return index;
    }

    //might be able to get away with just a 0,0?;
    private int AddInitialPoint()
    {
        // return AddPoint(new(random.NextFloat(), random.NextFloat()));
        return AddPoint(new Vector2(domainSizeX / 2f, domainSizeY / 2f));
    }

    //N- total samples to generate,n - dimensions, fixed at 2, r- radius between the samples
    //k- constant. Should take O(N)
    private void Sample(int k = 30)
    {
        var activeList = new List<int>();

        AddInitialPoint();
        activeList.Add(0);

        while (activeList.Count > 0 && currentIndex + 1 < N)
        {
            //pick a random index
            var ri = random.NextInt(activeList.Count);
            var sampleIndex = activeList[ri];
            var selectedPoint = points[sampleIndex];

            //generate k random points in the radius of r and r^2
            var genpoints = SampleAnnulus(selectedPoint, k);

            var tossFlag = true;
            foreach (var point in genpoints)
            {
                if (CheckPointRadius(point))
                {
                    var insertIndex = AddPoint(point);
                    activeList.Add(insertIndex);
                    tossFlag = false;
                    break;
                }
            }

            if (tossFlag) //no points found in k attempts
            {
                activeList.Remove(sampleIndex);
            }
        }
    }

    private bool CheckPointRadius(Vector2 point)
    {
        int gridWidth = grid.GetLength(0);
        int gridHeight = grid.GetLength(1);

        int i = (int)(point.x / cellSize);
        int j = (int)(point.y / cellSize);

        // Reject if outside domain
        if (i < 0 || j < 0 || i >= gridWidth || j >= gridHeight)
            return false;

        int searchRadius = 2; // check neighboring cells (5x5 area)

        for (int x = i - searchRadius; x <= i + searchRadius; x++)
        {
            for (int y = j - searchRadius; y <= j + searchRadius; y++)
            {
                if (x < 0 || y < 0 || x >= gridWidth || y >= gridHeight)
                    continue;

                int pointIndex = grid[x, y];
                if (pointIndex != -1)
                {
                    if ((point - points[pointIndex]).sqrMagnitude < (r * r))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private List<Vector2> SampleAnnulus(Vector2 center, int k)
    {
        var points = new List<Vector2>(k);

        for (int i = 0; i < k; i++)
        {
            // 1. Uniform direction
            float angle = random.NextFloat(0f, 2f * math.PI);
            Vector2 dir = new Vector2(math.cos(angle), math.sin(angle));

            float t = random.NextFloat();

            float inner = Mathf.Pow(r, 2);
            float outer = Mathf.Pow(2f * r, 2);

            float radius = Mathf.Pow(inner + t * (outer - inner), 1f / 2f);

            Vector2 point = center + dir * radius;
            //clamp point in domain
            if (point.x < 0 || point.y < 0 || point.x > domainSizeX || point.y > domainSizeY)
                continue;

            points.Add(point);
        }

        return points;
    }

    public Data GetData()
    {
        if (!hasData)
        {
            Debug.LogError("Sampler has not generated any data. Please run Sampler.Generate");
            return null;
        }
        if (cachedData == null)
        {
            cachedData = new Data((Vector2[])points.Clone(), (int[,])grid.Clone());
        }
        return cachedData;
    }

    public void DrawGizmos()
    {
        if (!drawGizmos) return;
        if (grid == null) return;

        //Draw the Grid Boundaries
        // Gizmos.color = new Color(1, 1, 1, 0.1f); // Transparent white

        //Draw vertical lines
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = new Vector3(x * cellSize, 0, 0);
            Vector3 end = new Vector3(x * cellSize, gridHeight * cellSize, 0);
            Gizmos.DrawLine(start, end);
        }

        //Draw horizontal lines
        for (int y = 0; y <= gridHeight; y++)
        {
            Vector3 start = new Vector3(0, y * cellSize, 0);
            Vector3 end = new Vector3(gridWidth * cellSize, y * cellSize, 0);
            Gizmos.DrawLine(start, end);
        }

        //Draw the Domain Boundary (The actual area sampling is allowed)
        Gizmos.color = Color.cyan;
        Vector3 bottomNode = Vector3.zero;
        Vector3 topNode = new Vector3(domainSizeX, domainSizeY, 0);
        // Draw a simple square representing the domain
        Gizmos.DrawWireCube(new Vector3(domainSizeX / 2, domainSizeY / 2, 0), new Vector3(domainSizeX, domainSizeY, 0));

        //Draw the sampled points
        Gizmos.color = Color.yellow;
        for (int i = 0; i < currentIndex; i++)
        {
            // Draw a small sphere for each point
            // Using a small fraction of 'r' to ensure they don't overlap visually
            Gizmos.DrawSphere(new Vector3(points[i].x, points[i].y, 0), r * 0.1f);
        }
    }

    public class Data
    {
        public Point[] points;
        public int[,] grid;

        public Data(Vector2[] pointsIn, int[,] grid)
        {
            this.points = new Point[pointsIn.Length];
            for (int i = 0; i < pointsIn.Length; i++)
            {
                this.points[i] = new(pointsIn[i]);
            }
            this.grid = grid;
        }
    }
}