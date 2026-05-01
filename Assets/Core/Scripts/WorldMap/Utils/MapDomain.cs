using System;
using UnityEngine;

//Displays data about the domain of the map and has functions to check if something is inside/out.
//Used by samplers or generation algorythms.
[Serializable]
public class MapDomain
{
    public int ScaleX = 100;
    public int ScaleY = 100;

    public Vector2 topLeft => new Vector2(0, ScaleY);
    public Vector2 topRight => new Vector2(ScaleX, ScaleY);
    public Vector2 bottomLeft => new Vector2(0, 0);
    public Vector2 bottomRight => new Vector2(ScaleX, 0);

    public MapDomain(int ScaleX, int ScaleY)
    {
        this.ScaleX = ScaleX;
        this.ScaleY = ScaleY;
    }

    public bool IsPointInDomain(Vector2 point)
    {
        return point.x >= 0 && point.x <= ScaleX && point.y >= 0 && point.y <= ScaleY;
    }

    public Vector2 ClampPointToBounds(Vector2 p)
    {
        float minX = bottomLeft.x;   // 0
        float maxX = bottomRight.x;  // ScaleX

        float minY = bottomLeft.y;   // 0
        float maxY = topLeft.y;      // ScaleY

        return new Vector2(
            Mathf.Clamp(p.x, minX, maxX),
            Mathf.Clamp(p.y, minY, maxY)
        );
    }

    public bool ClipLineToRect(ref Vector2 a, ref Vector2 b)
    {
        float t0 = 0f;
        float t1 = 1f;

        Vector2 d = b - a;

        bool Clip(float p, float q)
        {
            if (Mathf.Approximately(p, 0))
                return q >= 0;

            float r = q / p;

            if (p < 0)
            {
                if (r > t1) return false;
                if (r > t0) t0 = r;
            }
            else
            {
                if (r < t0) return false;
                if (r < t1) t1 = r;
            }
            return true;
        }

        if (Clip(-d.x, a.x - 0) &&
            Clip(d.x, ScaleX - a.x) &&
            Clip(-d.y, a.y - 0) &&
            Clip(d.y, ScaleY - a.y))
        {
            Vector2 newA = a + d * t0;
            Vector2 newB = a + d * t1;

            a = newA;
            b = newB;
            return true;
        }

        return false;
    }

    public void DrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topLeft, bottomLeft);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomLeft, bottomRight);

        Gizmos.DrawLine(topLeft + Vector2.up, topRight + Vector2.up);
        Gizmos.DrawLine(topLeft + Vector2.left, bottomLeft + Vector2.left);
        Gizmos.DrawLine(topRight + Vector2.right, bottomRight + Vector2.right);
        Gizmos.DrawLine(bottomLeft + Vector2.down, bottomRight + Vector2.down);
    }
}