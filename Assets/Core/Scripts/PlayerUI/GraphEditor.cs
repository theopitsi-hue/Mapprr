using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum EditModeState
{
    SELECT,
    MOVE,
}

[RequireComponent(typeof(GraphRenderer))]
public class GraphEditor : MonoBehaviour
{
    [SerializeField]
    public Camera cam;

    private GraphRenderer render;

    [SerializeField]
    private GraphData data;
    private Vector2 prevMpos;
    private Vector2 mDelta;
    private Vector2 mousePosition;
    [SerializeField]
    private EditModeState state;

    [SerializeField]
    private float selectionRange = 1;

    public List<GraphData.Point> selectedPoints = new();


    private void Awake()
    {
        data = new GraphData();
        render = GetComponent<GraphRenderer>();
        render.Initialize(data);
    }

    private void Update()
    {
        mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
        mDelta = mousePosition - prevMpos;
        prevMpos = mousePosition;

        aaa();
        CheckChangeState();
        UpdateStateLogic();

    }

    private void aaa()
    {
        if (Input.GetMouseButtonDown(1))
        {
            data.AddPoint(mousePosition);
        }
    }

    private void CheckChangeState()
    {
        switch (state)
        {
            case EditModeState.MOVE:
                if (!HasSeletedPoints() || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))
                {
                    state = EditModeState.SELECT;
                }
                return;
            case EditModeState.SELECT:
                if (Input.GetKeyDown(KeyCode.G) && HasSeletedPoints())
                {
                    state = EditModeState.MOVE;
                }
                return;
        }
    }

    private void UpdateStateLogic()
    {
        switch (state)
        {
            case EditModeState.MOVE:
                foreach (var item in selectedPoints)
                {
                    item.Position += mDelta;
                }

                return;

            case EditModeState.SELECT:
                if (Input.GetMouseButtonDown(0))
                {
                    if (!Input.GetKey(KeyCode.LeftShift))
                    {
                        ClearSelection();
                    }
                    AddSelectPoint(data.GetFirstClosest(mousePosition, selectionRange));
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ClearSelection();
                }

                if (selectedPoints.Count == 2 && Input.GetKeyDown(KeyCode.F))
                {
                    for (int i = 0; i < selectedPoints.Count; i++)
                    {
                        for (int y = selectedPoints.Count - 1; y >= 0; y--)
                        {
                            data.ConnectPoints(selectedPoints[i], selectedPoints[y]);
                        }
                    }
                }
                return;
        }
    }

    public bool HasSeletedPoints()
    {
        return selectedPoints.Count > 0;
    }

    public void ClearSelection()
    {
        selectedPoints.Clear();
    }

    public void AddSelectPoint(GraphData.Point a)
    {
        if (!selectedPoints.Contains(a))
            selectedPoints.Add(a);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var item in data.Connections)
        {
            Gizmos.DrawLine(item.a.Position, item.b.Position);
            Gizmos.DrawLine(item.b.Position, item.a.Position);
        }

        Gizmos.color = Color.white;
        foreach (var item in data.Points)
        {
            Gizmos.color = Color.white;
            if (selectedPoints.Contains(item))
            {
                Gizmos.color = Color.blue;
            }
            Gizmos.DrawSphere(item.Position, 0.1f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(mousePosition, selectionRange);
    }
}