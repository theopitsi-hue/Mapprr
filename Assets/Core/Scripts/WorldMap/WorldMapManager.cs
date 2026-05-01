using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    // Start is called before the first frame update
    void Start()
    {
        sampler = new PoissonDiscSampler(domain);
        sampler.Generate(sampleResolution);
        tris = DelaunayRedo.BowyerWatsonTriangulation(sampler.GetData().points.ToList());

        voronoiAssembler = new(domain);
        voronoiAssembler.Generate(sampler.GetData().points.ToList(), tris);
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
            foreach (var item in sampler.GetData().points)
            {
                Gizmos.DrawSphere(item.pos, 0.1f);
            }
            sampler.DrawGizmos();

            foreach (var tri in tris)
            {

                // Gizmos.color = Color.green;
                // Gizmos.DrawLine(tri.a.pos, tri.b.pos);
                // Gizmos.DrawLine(tri.b.pos, tri.c.pos);
                // Gizmos.DrawLine(tri.c.pos, tri.a.pos);


                Gizmos.color = Color.red;
                Gizmos.DrawSphere(tri.CircumCenter.pos, 0.1f);

                // Gizmos.color = Color.cyan - new Color(0, 0, 0, 0.9f);
                // Gizmos.DrawWireSphere(tri.CircumCenter.pos, tri.CircumRadius);


                // Gizmos.color = Color.magenta;
                // Gizmos.DrawSphere(tri.a.pos, 0.1f);
                // Gizmos.DrawSphere(tri.b.pos, 0.1f);
                // Gizmos.DrawSphere(tri.c.pos, 0.1f);

            }

            voronoiAssembler.DrawGizmos();
        }
    }
}
