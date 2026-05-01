using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class LayerManager : MonoBehaviour
{
    public Vector2 CanvasSize;

    private Transform layersChild;

    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i);
            if (c.name == "Layers")
            {
                layersChild = c;
            }
        }

        if (layersChild == null)
        {
            var n = new GameObject();
            n.name = "Layers";
            n.transform.parent = this.transform;
        }

        CreateLayer();
        CreateLayer();
        CreateLayer();
    }

    void Update()
    {

    }

    Layer CreateLayer()
    {
        var n = new GameObject();
        var l = n.AddComponent<Layer>();
        n.transform.parent = layersChild.transform;
        return l;
    }

    void PlaceLimitWalls()
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        // Gizmos.DrawSphere(new Vector3(CanvasSize.x, CanvasSize.y, 0), 0.5f);
        // Gizmos.DrawSphere(new Vector3(0, 0, 0), 0.5f);
        // Gizmos.DrawSphere(new Vector3(CanvasSize.x, 0, 0), 0.5f);
        // Gizmos.DrawSphere(new Vector3(0, CanvasSize.y, 0), 0.5f);
        Gizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(CanvasSize.x, 0, 0));
        Gizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(0, CanvasSize.y, 0));
        Gizmos.DrawLine(new Vector3(CanvasSize.x, 0, 0), new Vector3(CanvasSize.x, CanvasSize.y, 0));
        Gizmos.DrawLine(new Vector3(0, CanvasSize.y, 0), new Vector3(CanvasSize.x, CanvasSize.y, 0));
    }
}
