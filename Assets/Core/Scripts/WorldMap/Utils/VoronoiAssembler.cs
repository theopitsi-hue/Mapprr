
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class VoronoiAssembler
{
    List<Edge> voronoiEdges = new();
    MapDomain domain;
    VoronoiCellData[] cellData;

    public VoronoiAssembler(MapDomain domain)
    {
        this.domain = domain;
    }

    public void Generate(List<Point> centroids, List<Triangle> tris, Dictionary<Point, List<Triangle>> pointTriangles)
    {
        cellData = new VoronoiCellData[centroids.Count];

        //temp
        List<int> land = new();
        land.AddRange(new int[] { 1, 2, 3, 4, 5, 6 });

        for (int i = 0; i < centroids.Count; i++)
        {//todo: change from references to int indexes from the map assembler, maybe?
            cellData[i] = new VoronoiCellData(i, centroids[i], land.Contains(i), pointTriangles[centroids[i]], domain);
        }

        // DrawVoronoiEdgesOld(tris);
    }

    //this is a more optimized way to draw voronoi cell edges, so im keeping it for potential use in the future
    public void DrawVoronoiEdgesOld(List<Triangle> tris)
    {

        Dictionary<Edge, List<Triangle>> edgeMap = new();
        //generate an edgemap- any triangles that share an edge basically,
        //with the edge as determinant
        foreach (var tri in tris)
        {
            foreach (var edge in tri.edges)
            {
                if (!edgeMap.ContainsKey(edge))
                {
                    edgeMap[edge] = new List<Triangle>();
                }

                edgeMap[edge].Add(tri);
            }
        }

        foreach (var sharedEdge in edgeMap)
        {
            var triangles = sharedEdge.Value;

            //if an edge is shared between 2 triangles exactly
            if (triangles.Count == 2)
            {
                Vector2 c1 = triangles[0].CircumCenter.pos;
                Vector2 c2 = triangles[1].CircumCenter.pos;

                //IF i had access to the cell index right here, i could give each cell its correct points. Allas.
                if (domain.ClipLineToRect(ref c1, ref c2))
                {
                    //create a voronoi "edge" between them
                    voronoiEdges.Add(new Edge(new(c1), new(c2)));
                }
            }
        }


    }

    public void DrawGizmos()
    {

        foreach (var item in cellData)
        {

            Handles.Label(item.centroid, " Node:" + item.index + " land:" + item.isLand);
            foreach (var tr in item.debugMesh)
            {
                tr.DrawGizmos();
            }
            for (int i = 0; i < item.edgePoints.Count; i++)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(item.edgePoints[i].pos, 0.1f);
            }
        }

        foreach (var item in voronoiEdges)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(item.a.pos, item.b.pos);
        }

    }
}