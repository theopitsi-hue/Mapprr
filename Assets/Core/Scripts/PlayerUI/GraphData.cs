using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

[Serializable]
public class GraphData
{
    [SerializeField]
    private List<Point> points = new();

    [SerializeField]
    private HashSet<Connection> connections = new();

    public List<Point> Points => points;
    public HashSet<Connection> Connections => connections;

    public void RemovePoint(int i)
    {
        points.RemoveAt(i);
    }
    public Point AddPoint(Vector2 position)
    {
        var p = new Point(position);
        points.Add(p);
        return p;
    }

    public void ConnectPoints(Point a, Point b)
    {
        var c = new Connection(a, b);
        if (connections.Contains(c))
            return;
        connections.Add(c);
    }

    //todo: use chunking later
    public Point GetFirstClosest(Vector2 pos)
    {
        var dist = float.MaxValue;
        Point o = null;
        foreach (var item in points)
        {
            var dt = Vector2.Distance(item.Position, pos);
            if (dt < dist)
            {
                dist = dt;
                o = item;
            }
        }
        return o;
    }
    public Point GetFirstClosest(Vector2 pos, float range)
    {
        var dist = range;
        Point o = null;
        foreach (var item in points)
        {
            var dt = Vector2.Distance(item.Position, pos);
            if (dt < dist)
            {
                dist = dt;
                o = item;
            }
        }
        return o;
    }

    [Serializable]
    public class Point
    {
        [SerializeField]
        private Vector2 position;
        public Vector2 Position
        {
            get => position;
            set => position = value;
        }

        public Point(Vector2 position)
        {
            this.position = position;
        }

    }

    [Serializable]
    public class Connection : IEquatable<Connection>
    {
        [SerializeField]
        public Point a;
        [SerializeField]
        public Point b;

        public Connection(Point a, Point b)
        {
            this.a = a;
            this.b = b;
        }

        public bool Equals(Connection other)
        {
            if (other == null) return false;

            return
                (Equals(a, other.a) && Equals(b, other.b)) ||
                (Equals(a, other.b) && Equals(b, other.a));
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Connection);
        }

        public override int GetHashCode()
        {
            // Order-independent hash
            int hashA = a != null ? a.GetHashCode() : 0;
            int hashB = b != null ? b.GetHashCode() : 0;

            // XOR makes it order-independent
            return hashA ^ hashB;
        }

        public static bool operator ==(Connection c1, Connection c2)
        {
            if (ReferenceEquals(c1, c2)) return true;
            if (c1 is null || c2 is null) return false;
            return c1.Equals(c2);
        }

        public static bool operator !=(Connection c1, Connection c2)
        {
            return !(c1 == c2);
        }
    }
}
