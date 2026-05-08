using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VoronoiCellData
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

        foreach (var tri in triangles)
        {
            Vector2 c = tri.CircumCenter.pos;
            // c = domain.ClampPointToBounds(c);
            edgePoints.Add(new Point(c));
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