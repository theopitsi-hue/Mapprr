
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class VoronoiAssembler
{
    List<Edge> voronoiEdges = new();
    MapDomain domain;

    public VoronoiAssembler(MapDomain domain)
    {
        this.domain = domain;
    }

    public void Generate(List<Point> vertices, List<Triangle> tris)
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
        foreach (var item in voronoiEdges)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(item.a.pos, item.b.pos);
        }
    }
}