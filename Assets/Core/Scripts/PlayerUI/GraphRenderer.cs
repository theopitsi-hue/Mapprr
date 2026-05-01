using System;
using UnityEngine;

public class GraphRenderer : MonoBehaviour
{
    GraphData data;
    bool isInitialized = false;
    public Camera cam;

    public void Initialize(GraphData data)
    {
        isInitialized = true;
        this.data = data;
    }

    private void Update()
    {
        if (!isInitialized) return;
    }


}