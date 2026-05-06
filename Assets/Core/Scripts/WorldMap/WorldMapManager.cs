using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

public class WorldMapManager : MonoBehaviour
{
    [SerializeField]
    MapDomain domain = new MapDomain(100, 100);
    [SerializeField]
    PoissonDiscSampler sampler;
    [Range(0.1f, 5)]
    public float sampleResolution = 2;
    [SerializeField]
    VoronoiAssembler voronoiAssembler;

    [SerializeField] List<Triangle> tris;
    [SerializeField] List<Point> points;

    public bool drawVoronoi = false;
    public bool drawTris = false;

    Dictionary<Point, List<Triangle>> pointTriangles;

    // Start is called before the first frame update
    void Start()
    {
        sampler = new PoissonDiscSampler(domain);
        sampler.Generate(sampleResolution);
        //post sampler step: curb points based on height map ?

        points = sampler.GetData().points.ToList();

        tris = DelaunayRedo.BowyerWatsonTriangulation(points);
        pointTriangles = DelaunayRedo.AssemblePointToTriangleConnections(points, tris);

        voronoiAssembler = new(domain);
        voronoiAssembler.Generate(points, tris, pointTriangles);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        domain.DrawGizmos();
        if (Application.isPlaying && sampler != null && sampler.GetData() != null)
        {
            for (int i = 0; i < points.Count; i++)
            {
                var item = points[i];
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
