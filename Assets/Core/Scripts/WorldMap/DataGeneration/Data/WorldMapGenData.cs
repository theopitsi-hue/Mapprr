//Storage class for the world's gen data. Populated during generation
using System;
using System.Collections.Generic;

[Serializable]
public class WorldMapGenData
{
    public readonly MapDomain domain;
    public int randomSeed;

    public List<Point> Centroids => centroids;
    public List<Triangle> DelaunayTriangles => delaunayTriangles;
    public List<VoronoiCellData> VoronoiCells => voronoiCells;

    private List<Point> centroids;
    private List<Triangle> delaunayTriangles;
    private List<VoronoiCellData> voronoiCells;

    public WorldMapGenData(MapDomain domain)
    {
        this.domain = domain;
    }

    public void SetCentroids(List<Point> points)
    {
        centroids = points;
    }

    public void SetDelaunayTriangles(List<Triangle> triangles)
    {
        delaunayTriangles = triangles;
    }

    public void SetVoronoiData(List<VoronoiCellData> data)
    {
        voronoiCells = data;
    }
}