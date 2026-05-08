using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SimplePolygonMesh : MonoBehaviour
{
    [Header("Polygon points (Counter-Clockwise)")]
    public List<Vector2> points = new List<Vector2>();

    void Start()
    {
        Generate();
    }

    public void Generate()
    {
        if (points == null || points.Count < 3)
        {
            Debug.LogError("Need at least 3 points");
            return;
        }

        Mesh mesh = new Mesh();

        // Convert to Vector3
        Vector3[] vertices = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++)
            vertices[i] = new Vector3(points[i].x, 0, points[i].y);

        List<int> triangles = Triangulate(points);

        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    // ----------------------------------------
    // EAR CLIPPING
    // ----------------------------------------
    List<int> Triangulate(List<Vector2> verts)
    {
        List<int> indices = new List<int>();
        List<int> V = new List<int>();

        for (int i = 0; i < verts.Count; i++)
            V.Add(i);

        int guard = 0; // prevents infinite loop

        while (V.Count > 3 && guard < 10000)
        {
            guard++;
            bool earFound = false;

            for (int i = 0; i < V.Count; i++)
            {
                int prev = V[(i - 1 + V.Count) % V.Count];
                int curr = V[i];
                int next = V[(i + 1) % V.Count];

                if (IsEar(prev, curr, next, verts, V))
                {
                    indices.Add(prev);
                    indices.Add(curr);
                    indices.Add(next);

                    V.RemoveAt(i);
                    earFound = true;
                    break;
                }
            }

            if (!earFound)
            {
                Debug.LogWarning("Triangulation failed. Check polygon shape/winding.");
                break;
            }
        }

        if (V.Count == 3)
        {
            indices.Add(V[0]);
            indices.Add(V[1]);
            indices.Add(V[2]);
        }

        return indices;
    }

    bool IsEar(int a, int b, int c, List<Vector2> verts, List<int> V)
    {
        if (!IsConvex(verts[a], verts[b], verts[c]))
            return false;

        for (int i = 0; i < V.Count; i++)
        {
            int p = V[i];
            if (p == a || p == b || p == c) continue;

            if (PointInTriangle(verts[p], verts[a], verts[b], verts[c]))
                return false;
        }

        return true;
    }

    bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
    {
        return Cross(b - a, c - b) > 0;
    }

    float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float area = Cross(b - a, c - a);

        float s = Cross(c - a, p - a) / area;
        float t = Cross(a - b, p - b) / area;
        float u = Cross(b - c, p - c) / area;

        return s >= 0 && t >= 0 && u >= 0;
    }
}