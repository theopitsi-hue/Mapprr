using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WorldMapDatagen : MonoBehaviour
{
    [SerializeField]
    MapDomain domain = new MapDomain(100, 100);
    [SerializeField]
    PoissonDiscSampler sampler;
    [Range(0.1f, 5)]
    public float sampleResolution = 2;
    [SerializeField]
    VoronoiAssembler voronoiAssembler;

    public bool drawVoronoi = false;
    public bool drawTris = false;

    //COLLECT DATA HERE WOOOOOOOOOOOOOOOOOOOOOO
    List<Point> centroids;
    //List<Point> triangleCentroids; //centroids produced by the delu triangulation
    List<Triangle> tris;
    List<Triangle> voronoiTris;

    //associates points/centroids with all triangles containing them
    Dictionary<Point, List<Triangle>> pointDeluTriDictionary;

    //TEMPORARY
    MeshRenderer renderer;
    MeshFilter filter;

    void Start()
    {
        sampler = new PoissonDiscSampler(domain);
        sampler.Generate(sampleResolution);
        //post sampler step: curb points based on height map ?

        centroids = sampler.GetData().points.ToList();

        tris = DelaunayRedo.BowyerWatsonTriangulation(centroids);
        pointDeluTriDictionary = DelaunayRedo.AssemblePointToTriangleConnections(centroids, tris);
        //triangleCentroids = DelaunayRedo.GetTriangleCentroids(tris);

        voronoiAssembler = new(domain);
        voronoiAssembler.Generate(centroids, tris, pointDeluTriDictionary);
        voronoiTris = voronoiAssembler.GetAllMesh();

        filter = GetComponent<MeshFilter>();
        renderer = GetComponent<MeshRenderer>();
        fillMesh();
    }

    private void fillMesh()
    {
        // Mesh mesh = new Mesh();

        // List<Vector3> vertices = new();
        // List<int> indices = new();
        // List<Vector2> uv = new();

        // //approach 1 - triangles from delaunay
        // for (int i = 0; i < tris.Count; i++)
        // {
        //     var tri = tris[i];
        //     int baseIndex = vertices.Count;
        //     vertices.Add(tri.a.pos);
        //     vertices.Add(tri.b.pos);
        //     vertices.Add(tri.c.pos);

        //     indices.Add(baseIndex + 0);
        //     indices.Add(baseIndex + 1);
        //     indices.Add(baseIndex + 2);

        //     uv.Add(domain.ToDomainUV(tri.a.pos));
        //     uv.Add(domain.ToDomainUV(tri.b.pos));
        //     uv.Add(domain.ToDomainUV(tri.c.pos));
        // }

        // mesh.SetVertices(vertices);
        // mesh.SetUVs(0, uv);
        // mesh.SetTriangles(indices, 0);
        // mesh.RecalculateNormals();
        // mesh.RecalculateBounds();

        filter.mesh = BuildMeshIndexed(voronoiTris);

    }

    public Mesh BuildMeshIndexed(List<Triangle> tris)
    {
        var vertices = new List<Vector3>();
        var indices = new List<int>();
        var uvs = new List<Vector2>();
        var map = new Dictionary<Vector3, int>();

        int GetIndex(Vector3 v)
        {
            if (map.TryGetValue(v, out int index))
                return index;

            index = vertices.Count;
            vertices.Add(v);

            uvs.Add(domain.ToDomainUV(v));

            map[v] = index;
            return index;
        }

        foreach (var t in tris)
        {
            // Flip winding if needed
            indices.Add(GetIndex(t.a.pos));
            indices.Add(GetIndex(t.c.pos));
            indices.Add(GetIndex(t.b.pos));
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(indices, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private void OnDrawGizmos()
    {
        domain.DrawGizmos();
        if (Application.isPlaying && sampler != null && sampler.GetData() != null)
        {
            for (int i = 0; i < centroids.Count; i++)
            {
                var item = centroids[i];
                Gizmos.DrawSphere(item.pos, 0.1f);
            }
            sampler.DrawGizmos();

            foreach (var tri in tris)
            {

                if (drawTris)
                    tri.DrawGizmos();


                Gizmos.color = Color.red;
                Gizmos.DrawSphere(tri.CircumCenter.pos, 0.1f);

                // Gizmos.color = Color.cyan - new Color(0, 0, 0, 0.9f);
                // Gizmos.DrawWireSphere(tri.CircumCenter.pos, tri.CircumRadius);


                // Gizmos.color = Color.magenta;
                // Gizmos.DrawSphere(tri.a.pos, 0.1f);
                // Gizmos.DrawSphere(tri.b.pos, 0.1f);
                // Gizmos.DrawSphere(tri.c.pos, 0.1f);

            }

            if (drawVoronoi)
                voronoiAssembler.DrawGizmos();
        }
    }
}
