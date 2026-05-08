using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
class WorldMapMeshManager : MonoBehaviour
{
    //TEMPORARY
    MeshRenderer renderer;
    MeshFilter filter;

    public void Generate()
    {
        throw new NotImplementedException();
    }

    public void Initialize(WorldMapGenData data)
    {
        filter = GetComponent<MeshFilter>();
        renderer = GetComponent<MeshRenderer>();


        // filter.mesh = BuildMeshIndexed(voronoiTris);

    }

    public void Tick()
    {
    }


    public Mesh BuildMeshIndexed(MapDomain domain, List<Triangle> tris)
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
}