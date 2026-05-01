using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Vertex
{
    [SerializeField]
    public Vector2 pos;
    public float x => pos.x;
    public float y => pos.y;
    public float VectorMagnitude => pos.magnitude;
    public float VectorMagnitudeSqr => pos.sqrMagnitude;

    public Vertex(Vector2 pos)
    {
        this.pos = pos;
    }

    public Vertex(float a, float b)
    {
        this.pos = new(a, b);
    }
}

[Serializable]
public class Edge
{
    [SerializeField]
    public Vertex a, b;

    public float Length => (a.pos - b.pos).magnitude;

    public Edge(Vertex a, Vertex b)
    {
        this.a = a;
        this.b = b;
    }
    public override bool Equals(object obj)
    {
        if (obj is not Edge o) return false;

        return (a == o.a && b == o.b) ||
               (a == o.b && b == o.a);
    }

    public override int GetHashCode()
    {
        int ha = a.GetHashCode();
        int hb = b.GetHashCode();

        // order-independent hash
        return ha ^ hb;
    }
}


[Serializable]
public class Triangle
{
    public Vertex a, b, c;

    public Edge e1, e2, e3;

    public Edge[] edges;
    public Vertex[] vertices;

    public Vertex CircumCenter;
    public float CircumRadius;

    private bool degenerate;

    public Triangle(Vertex a, Vertex b, Vertex c)
    {
        degenerate = false;

        float cross = (b.x - a.x) * (c.y - a.y) -
                 (b.y - a.y) * (c.x - a.x);

        if (cross < 0)
        {
            (b, c) = (c, b);
        }

        this.a = a;
        this.b = b;
        this.c = c;

        edges = new Edge[3];
        vertices = new Vertex[3];

        e1 = new Edge(a, b);
        e2 = new Edge(b, c);
        e3 = new Edge(c, a);

        edges[0] = e1;
        edges[1] = e2;
        edges[2] = e3;

        vertices[0] = a;
        vertices[1] = b;
        vertices[2] = c;

        // float ab = a.VectorMagnitudeSqr;
        // float cd = b.VectorMagnitudeSqr;
        // float ef = c.VectorMagnitudeSqr;

        // float ax = a.x;
        // float ay = a.y;
        // float bx = b.x;
        // float by = b.y;
        // float cx = c.x;
        // float cy = c.y;

        // float denomX = (ax * (cy - by) + bx * (ay - cy) + cx * (by - ay));
        // float denomY = (ay * (cx - bx) + by * (ax - cx) + cy * (bx - ax));

        // float circumX = (ab * (cy - by) + cd * (ay - cy) + ef * (by - ay)) / denomX;
        // float circumY = (ab * (cx - bx) + cd * (ax - cx) + ef * (bx - ax)) / denomY;

        // var circum = new Vertex(circumX / 2f, circumY / 2f);
        // float circumRadius = Vector2.Distance(a.pos, circum.pos);

        Vector2 center = CalcCircumCenter();
        //all points should b exactly on the circumradius so this is fine, right?
        float radius = Vector2.Distance(a.pos, center);

        CircumCenter = new Vertex(center);
        CircumRadius = radius;
    }

    public bool SharesAnyEdges(Triangle other)
    {
        return e1.Equals(other.e1) || e1.Equals(other.e2) || e1.Equals(other.e3) ||
               e2.Equals(other.e1) || e2.Equals(other.e2) || e2.Equals(other.e3) ||
               e3.Equals(other.e1) || e3.Equals(other.e2) || e3.Equals(other.e3);
    }

    public bool IsDegenerate()
    {
        return degenerate || float.IsInfinity(CircumRadius) || float.IsNaN(a.x) || float.IsNaN(b.x) || float.IsNaN(c.x) || Mathf.Approximately((float)Area(), 0);
    }

    public Vector2 CalcCircumCenter()
    {
        float x1 = a.x, y1 = a.y;
        float x2 = b.x, y2 = b.y;
        float x3 = c.x, y3 = c.y;

        float D = 2 * (x1 * (y2 - y3) +
                       x2 * (y3 - y1) +
                       x3 * (y1 - y2));

        if (Mathf.Approximately(D, 0))
        {
            //Debug.LogWarning("Points are collinear; no circumcenter exists.");
            degenerate = true;
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

    public float CalcCircumRadius()
    {
        float sideA = Vector2.Distance(b.pos, c.pos); //BC
        float sideB = Vector2.Distance(c.pos, a.pos); //CA
        float sideC = Vector2.Distance(a.pos, b.pos); //AB

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

    public bool IsPointInCircumCircle(Vertex p)
    {
        float dist = Vector2.Distance(p.pos, CircumCenter.pos);
        return dist < CircumRadius;
    }

    public bool HasEdge(Edge edge)
    {
        foreach (var item in edges)
        {
            if (edge == item)
            {
                return true;
            }
        }

        return false;
    }

    public bool HasVertex(Vertex v)
    {
        return a == v || b == v || c == v;
    }


    //Compares two triangles to see if they share points. Reference based.
    public bool IsSameTriangle(Triangle obj)
    {
        if (obj is not Triangle o) return false;

        return HasVertex(o.a) &&
               HasVertex(o.b) &&
               HasVertex(o.c);
    }

}
