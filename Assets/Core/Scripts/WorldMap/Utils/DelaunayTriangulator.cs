using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

[Serializable]
public class DelaunayTriangulator
{
    public List<Triangle> Process(Vector2[] points)
    {
        var triangles = new List<Triangle>();
        var soopa = CreateSuperTri(points);
        triangles.Add(soopa);

        foreach (var point in points)
        {
            foreach (var tr in AddVertex(point, triangles))
            {
                triangles.Add(tr);
            }
        }

        triangles.RemoveAll(t =>
        {
            return !soopa.SharesAnyEdges(t);
        });

        return triangles;
    }

    public List<Triangle> AddVertex(Vector2 point, List<Triangle> triangles)
    {
        var nTriangles = new List<Triangle>();
        nTriangles.AddRange(triangles);
        var edges = new List<Edge>();

        nTriangles.RemoveAll(t =>
        {
            if (t.IsInCircumcircle(point))
            {
                edges.Add(new Edge(t.a, t.b));
                edges.Add(new Edge(t.b, t.c));
                edges.Add(new Edge(t.c, t.a));

                return false;
            }
            return true;
        });

        edges = GetUniqueEdges(edges);

        foreach (var edge in edges)
        {
            nTriangles.Add(new(edge.a, edge.b, point));
        }

        return nTriangles;
    }

    public static List<Edge> GetUniqueEdges(List<Edge> edges)
    {
        List<Edge> unique = new List<Edge>();

        for (int i = 0; i < edges.Count; i++)
        {
            bool isUnique = true;

            for (var j = 0; j < edges.Count; j++)
            {
                if (i != j && edges[i].Equals(edges[j]))
                {
                    isUnique = false;
                    break;
                }
            }

            if (isUnique)
            {
                unique.Add(edges[i]);
            }
        }

        return unique;
    }


    public Triangle CreateSuperTri(Vector2[] points)
    {
        float minx = float.MaxValue, miny = float.MaxValue;
        float maxx = float.MinValue, maxy = float.MinValue;

        foreach (var p in points)
        {
            minx = Mathf.Min(p.x, minx);
            miny = Mathf.Min(p.y, miny);

            maxx = Mathf.Max(p.x, maxx);
            maxy = Mathf.Max(p.y, maxy);
        }

        float dx = (maxx - minx) * 10f;
        float dy = (maxy - miny) * 10f;

        Vector2 p1 = new Vector2(minx - dx, miny - dy);
        Vector2 p2 = new Vector2(minx - dx, maxy + dy);
        Vector2 p3 = new Vector2(maxx + dx, maxy + dy);

        return new Triangle(p1, p2, p3);
    }


    [Serializable]
    public class Triangle
    {
        public int index;
        public Vector2 a, b, c;

        public Edge e1, e2, e3;

        public Vector2 CircumCenter => CalcCircumcenter();
        public float Circumradius => CalcCircumradius();

        public Triangle(Vector2 a, Vector2 b, Vector2 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;

            e1 = new Edge(a, b);
            e2 = new Edge(b, c);
            e3 = new Edge(c, a);
        }

        public bool SharesAnyEdges(Triangle other)
        {
            return e1 == other.e1 || e1 == other.e2 || e1 == other.e3 ||
                   e2 == other.e1 || e2 == other.e2 || e2 == other.e3 ||
                   e3 == other.e1 || e3 == other.e2 || e3 == other.e3;
        }

        public Vector2 CalcCircumcenter()
        {
            float x1 = a.x, y1 = a.y;
            float x2 = b.x, y2 = b.y;
            float x3 = c.x, y3 = c.y;

            float D = 2 * (x1 * (y2 - y3) +
                           x2 * (y3 - y1) +
                           x3 * (y1 - y2));

            if (Mathf.Approximately(D, 0))
            {
                Debug.LogWarning("Points are collinear; no circumcenter exists.");
                return Vector2.zero;
            }

            float centerX = ((x1 * x1 + y1 * y1) * (y2 - y3) +
                             (x2 * x2 + y2 * y2) * (y3 - y1) +
                             (x3 * x3 + y3 * y3) * (y1 - y2)) / D;

            float centerY = ((x1 * x1 + y1 * y1) * (x3 - x2) +
                             (x2 * x2 + y2 * y2) * (x1 - x3) +
                             (x3 * x3 + y3 * y3) * (x2 - x1)) / D;

            return new Vector2(centerX, centerY);
        }

        public float CalcCircumradius()
        {
            float sideA = Vector2.Distance(b, c); //BC
            float sideB = Vector2.Distance(c, a); //CA
            float sideC = Vector2.Distance(a, b); //AB

            float area = (float)Area();

            if (Mathf.Approximately(area, 0))
            {
                Debug.LogWarning("Degenerate triangle; no circumradius.");
                return 0;
            }

            return (sideA * sideB * sideC) / (4f * area);
        }

        private double Area()
        {
            return Math.Abs(
                (a.x * (b.y - c.y) +
                 b.x * (c.y - a.y) +
                 c.x * (a.y - b.y)) / 2.0
            );
        }

        public bool IsInCircumcircle(Vector2 p)
        {
            Vector2 center = CircumCenter;

            float dx = center.x - p.x;
            float dy = center.y - p.y;

            return (dx * dx + dy * dy) <= (Circumradius * Circumradius);
        }
    }

    [Serializable]
    public class Edge
    {
        public Vector2 a, b;

        public float length => (a - b).magnitude;

        public Edge(Vector2 a, Vector2 b)
        {
            this.a = a;
            this.b = b;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            Edge other = obj as Edge;
            return a.Equals(other.a) && b.Equals(other.b) || (a.Equals(other.b) && b.Equals(other.a));
        }

        public override int GetHashCode()
        {
            return a.GetHashCode() + b.GetHashCode();
        }
    }
}
