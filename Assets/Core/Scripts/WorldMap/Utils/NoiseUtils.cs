using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class NoiseUtils
{
    public static Vector2 Quantize(Vector2 v, float grid = 0.0001f)
    {
        return new Vector2(
            Mathf.Round(v.x / grid) * grid,
            Mathf.Round(v.y / grid) * grid
        );
    }
}
