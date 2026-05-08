using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

public class WorldMapDatagen : MonoBehaviour
{
    [SerializeField]
    MapDomain domain = new MapDomain(100, 100);
    PoissonDiscSampler sampler;
    [Range(0.1f, 5)]
    public float sampleResolution = 2;
    VoronoiAssembler voronoiAssembler;

    public bool drawVoronoi = false;
    public bool drawTris = false;

    //COLLECT DATA HERE WOOOOOOOOOOOOOOOOOOOOOO
    WorldMapGenData data;

    void Start()
    {
        data = GenerateData();
    }

    private WorldMapGenData GenerateData()
    {
        var data = new WorldMapGenData(domain);
        sampler = new PoissonDiscSampler(domain);
        sampler.Generate(sampleResolution);
        //post sampler step: curb point resolution based on if its land or not? thru heightmap?

        data.SetCentroids(sampler.GetData().points.ToList());

        data.SetDelaunayTriangles(DelaunayRedo.BowyerWatsonTriangulation(data.Centroids));

        voronoiAssembler = new(domain);
        voronoiAssembler.Generate(data.Centroids, data.DelaunayTriangles);
        data.SetVoronoiData(voronoiAssembler.GetVoronoiData());

        return data;
    }


    private void OnDrawGizmos()
    {
        domain.DrawGizmos();
        if (Application.isPlaying && sampler != null && sampler.GetData() != null)
        {
            for (int i = 0; i < data.Centroids.Count; i++)
            {
                var item = data.Centroids[i];
                Gizmos.DrawSphere(item.pos, 0.1f);
            }
            sampler.DrawGizmos();

            foreach (var tri in data.DelaunayTriangles)
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
            {
                Gizmos.color = Color.cyan;
                foreach (var item in data.VoronoiCells)
                {
                    //edge points r already ordered
                    for (int i = 0; i < item.edgePoints.Count; i++)
                    {
                        int next = i + 1;
                        if (i == item.edgePoints.Count - 1)
                        {
                            next = 0;
                        }
                        //this generates double edges. Oh well.
                        Gizmos.DrawLine(item.edgePoints[i].pos, item.edgePoints[next].pos);
                    }
                }
            }
        }
    }
}
