using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DelaunayRedo
{
    private static bool debug = false;

    public static List<Triangle> BowyerWatsonTriangulation(List<Point> points)
    {
        if (debug) Debug.Log($"Triangulating {points.Count} points..");

        //structure to hold all produced triangles
        List<Triangle> triangulation = new();

        //generate and add a super triangle that contains all points in list
        Triangle super = GenerateSuperTriangle(points);
        triangulation.Add(super);

        foreach (var point in points)
        {
            List<Triangle> badTriangles = new();

            //find all triangles that are no longer valid due
            //to the insersion
            foreach (var triangle in triangulation)
            {
                if (triangle.IsPointInCircumCircle(point))
                {
                    badTriangles.Add(triangle);
                }
            }
            if (debug) Debug.Log($"Point {point} -> bad triangles: {badTriangles.Count}");

            Dictionary<Edge, int> edgeCount = new();

            foreach (var triangle in badTriangles)
            {
                foreach (var edge in triangle.edges)
                {
                    if (!edgeCount.ContainsKey(edge))
                        edgeCount[edge] = 0;

                    edgeCount[edge]++;
                }
            }

            List<Edge> polygon = new();

            foreach (var kv in edgeCount)
            {
                if (kv.Value == 1)
                {
                    polygon.Add(kv.Key);
                }
            }
            if (debug) Debug.Log($"Polygon edges: {polygon.Count}");

            if (debug) Debug.Log($"Triangulation before remove:{triangulation.Count}");

            //remove em from the data structure
            foreach (var item in badTriangles)
            {
                triangulation.Remove(item);
            }
            if (debug) Debug.Log($"Removed:{badTriangles.Count}");

            if (debug) Debug.Log($"Triangulation after remove:{triangulation.Count}");

            //re-triangulate the hole
            foreach (var edge in polygon)
            {
                //ordering might be weird, but internal triangle ordering fixes should solve it
                Triangle nt = new Triangle(edge.a, edge.b, point);

                //prevent degenerate triangles entirely from being added to the final triangulation
                if (!nt.IsDegenerate())
                {
                    triangulation.Add(nt);
                }
            }
        }

        List<Triangle> evenBadderTriangles = new();
        //clean up post-insertion (HA)
        foreach (var triangle in triangulation)
        {
            foreach (var vertex in super.vertices)
            {
                if (triangle.HasVertex(vertex))
                {
                    evenBadderTriangles.Add(triangle);
                }
            }
        }

        if (debug) Debug.Log($"POST Triangulation before remove:{triangulation.Count}");
        foreach (var triangle in evenBadderTriangles)
        {
            triangulation.Remove(triangle);
        }

        if (debug) Debug.Log($"Removing:{evenBadderTriangles.Count}");

        if (debug) Debug.Log($"POST Triangulation after remove:{triangulation.Count}");

        // int degenerate = 0;
        // for (int i = triangulation.Count - 1; i >= 0; i--)
        // {
        //     if (triangulation[i].IsDegenerate())
        //     {
        //         triangulation.RemoveAt(i);
        //         degenerate++;
        //     }
        // }
        // if (debug) Debug.Log($"Removed {degenerate} Degenerate Triangles");

        if (debug) Debug.Log($"Generated: {triangulation.Count} triangles!");
        return triangulation;
    }

    //todo: change to convex hull -> least binding triangle, because this doesnt cover all vertices.
    private static Triangle GenerateSuperTriangle(List<Point> points)
    {
        float minx = float.PositiveInfinity;
        float miny = float.PositiveInfinity;
        float maxx = float.NegativeInfinity;
        float maxy = float.NegativeInfinity;

        foreach (var v in points)
        {
            minx = Mathf.Min(minx, v.x);
            miny = Mathf.Min(miny, v.y);
            maxx = Mathf.Max(maxx, v.x);
            maxy = Mathf.Max(maxy, v.y);
        }

        float dx = maxx - minx;
        float dy = maxy - miny;
        float dmax = Mathf.Max(dx, dy);

        float midx = (minx + maxx) * 0.5f;
        float midy = (miny + maxy) * 0.5f;

        //Much tighter and symmetric
        float scale = 2f;

        Vector2 v0 = new Vector2(midx - scale * dmax, midy - dmax);
        Vector2 v1 = new Vector2(midx, midy + scale * dmax);
        Vector2 v2 = new Vector2(midx + scale * dmax, midy - dmax);
        if (debug) Debug.Log("Generated super triangle with edges: " + v0 + "," + v1 + "," + v2);
        return new Triangle(new(v0), new(v1), new(v2));
    }
}