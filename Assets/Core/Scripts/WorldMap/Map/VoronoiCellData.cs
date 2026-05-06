using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
class VoronoiCellData
{
    public readonly int index;
    private Point center;
    public Vector2 centroid => center.pos;
    public bool isLand = false;
    public List<Point> edgePoints;
    public List<Triangle> debugMesh;

    public VoronoiCellData(int index, Point center, bool isLand, List<Triangle> triangles, MapDomain domain)
    {
        this.index = index;
        this.center = center;
        this.isLand = isLand;
        edgePoints = new();
        debugMesh = new();

        for (int i = 0; i < triangles.Count; i++)
        {
            //where potentially edits could hapen to immitate the editing capabilities of shorelines
            //in that one fantasy map

            //borders look like ass

            //attempt 1 - messy
            // edgePoints.Add(new(domain.ClampPointToBounds(triangles[i].CircumCenter.pos)));

            //attempt 2 - dont include circumcenters outside of bounds
            //very bad edges and incomplete geometry.
            // if (domain.IsPointInDomain(triangles[i].CircumCenter.pos))
            //     edgePoints.Add(triangles[i].CircumCenter);
        }

        Dictionary<Edge, List<Triangle>> edgeMap = new();
        //generate an edgemap- any triangles that share an edge basically,
        //with the edge as determinant
        foreach (var tri in triangles)
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
            var tris = sharedEdge.Value;

            //if an edge is shared between 2 triangles exactly
            if (tris.Count == 2)
            {
                Vector2 c1 = tris[0].CircumCenter.pos;
                Vector2 c2 = tris[1].CircumCenter.pos;


                if (domain.ClipLineToRect(ref c1, ref c2))
                {
                    //create a voronoi "edge" between them
                    edgePoints.Add(new(c1));
                    edgePoints.Add(new(c2));
                }
            }
        }


        //sorting for triangle creation for mesh stage, might be able to gpu it
        edgePoints.Sort((a, b) =>
        {
            var angleA = MathF.Atan2(a.y - centroid.y, a.x - centroid.x);
            var angleB = MathF.Atan2(b.y - centroid.y, b.x - centroid.x);
            return angleA.CompareTo(angleB);
        });

        //assembling the triangle mesh, will move to other function soon
        //probably will turn it into some collect data from all cells -> render thing
        for (int i = 0; i < edgePoints.Count; i++)
        {
            var P0 = edgePoints[i];
            var P1 = edgePoints[(i + 1) % edgePoints.Count];

            Triangle triangle = new(center, P0, P1); //should b mesh ready
            debugMesh.Add(triangle);
        }
    }

}