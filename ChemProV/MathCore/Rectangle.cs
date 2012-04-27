/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ChemProV.MathCore
{
    public class Rectangle
    {
        /// <summary>
        /// Center point
        /// </summary>
        private Vector m_c;

        /// <summary>
        /// Rectangle coordinate system vectors
        /// </summary>
        private Vector m_u, m_v;

        public static Rectangle CreateFromCanvasRect(Point location, double width, double height)
        {
            Rectangle r = new Rectangle();
            r.m_c = new Vector(location.X + width / 2.0, location.Y + height / 2.0);
            r.m_u = new Vector(width / 2.0, 0.0);
            r.m_v = new Vector(0.0, height / -2.0);
            return r;
        }

        public bool Contains(Point point)
        {
            double c = point.X - m_c.X;
            double d = point.Y - m_c.Y;

            double det = m_u.X * m_v.Y - m_v.X * m_u.Y;
            if (0.0 == det)
            {
                return false;
            }

            double a = (c * m_v.Y - m_v.X * d) / det;
            double b = (m_u.X * d - c * m_u.Y) / det;

            return (a >= -1.0f && a <= 1.0f && b >= -1.0f && b <= 1.0f);
        }

        public bool Contains(Vector point)
        {
            return Contains(new Point(point.X, point.Y));
        }

        public bool ContainsAny(params Vector[] points)
        {
            foreach (Vector pt in points)
            {
                if (Contains(pt))
                {
                    return true;
                }
            }

            return false;
        }

        public Vector[] GetCornerPoints()
        {
            return new Vector[]{
                m_c - m_u + m_v,
                m_c + m_u + m_v,
                m_c + m_u - m_v,
                m_c - m_u - m_v};
        }

        public LineSegment[] GetEdges()
        {
            Vector[] pts = GetCornerPoints();
            return new LineSegment[]{
                new LineSegment(pts[0], pts[1]),
                new LineSegment(pts[1], pts[2]),
                new LineSegment(pts[2], pts[3]),
                new LineSegment(pts[3], pts[0])};
        }

        public Vector[] GetIntersections(LineSegment segment)
        {
            List<Vector> pts = new List<Vector>();
            foreach (LineSegment ls in GetEdges())
            {
                Vector pt = new Vector();
                if (ls.Intersects(segment, ref pt))
                {
                    pts.Add(pt);
                }
            }

            return pts.ToArray();
        }

        public LineSegment GetShortestConnectingLine(Rectangle other)
        {
            // Start with the special case of overlapping rectangles
            if (Overlaps(other))
            {
                // In this case we'll make a line connecting the two closest corner points
                LineSegment returnMe = new LineSegment();
                double dist = double.MaxValue;
                foreach (Vector v1 in GetCornerPoints())
                {
                    foreach (Vector v2 in other.GetCornerPoints())
                    {
                        if ((v2 - v1).Length < dist)
                        {
                            returnMe = new LineSegment(v1, v2);
                            dist = returnMe.Length;
                        }
                    }
                }

                return returnMe;
            }

            // Find the shortest line between all pairs of edge segments
            bool didFirst = false;
            LineSegment segment = new LineSegment();
            foreach (LineSegment edgeInThis in GetEdges())
            {
                foreach (LineSegment edgeInOther in other.GetEdges())
                {
                    LineSegment temp = GetShortestLine(edgeInThis, edgeInOther);
                    if (!didFirst)
                    {
                        didFirst = true;
                        segment = temp;
                        continue;
                    }

                    // See if this segment is shorter
                    if (temp.Length < segment.Length)
                    {
                        segment = temp;
                    }
                }
            }

            return segment;
        }

        /// <summary>
        /// Gets the shortest connecting line between two line segments that are known NOT 
        /// TO INTERSECT.
        /// </summary>
        private LineSegment GetShortestLine(LineSegment a, LineSegment b)
        {
            // With two non-intersecting segments, the shortest line will have at least 
            // one of its points being an endpoint of one of the lines.
            Vector onBctAA = b.GetClosestPoint(a.A);
            Vector onBctAB = b.GetClosestPoint(a.B);
            Vector onActBA = a.GetClosestPoint(b.A);
            Vector onActBB = a.GetClosestPoint(b.B);

            double l1 = (onBctAA - a.A).Length;
            double l2 = (onBctAB - a.B).Length;
            double l3 = (onActBA - b.A).Length;
            double l4 = (onActBB - b.B).Length;

            double minLength = Math.Min(l1, Math.Min(l2, Math.Min(l3, l4)));
            if (minLength == l1)
            {
                return new LineSegment(onBctAA, a.A);
            }
            else if (minLength == l2)
            {
                return new LineSegment(onBctAB, a.B);
            }
            else if (minLength == l3)
            {
                return new LineSegment(onActBA, b.A);
            }
            return new LineSegment(onActBB, b.B);
        }

        /// <summary>
        /// Returns true if there is any point in 2D space that exists within both rectangles.
        /// </summary>
        public bool Overlaps(Rectangle other)
        {
            // First check if any corner points of one are inside the other
            if (ContainsAny(other.GetCornerPoints()) || other.ContainsAny(GetCornerPoints()))
            {
                return true;
            }

            // The other possibility is that any edges intersect
            LineSegment[] b = other.GetEdges();
            foreach (LineSegment ls1 in GetEdges())
            {
                foreach (LineSegment ls2 in b)
                {
                    if (ls1.Intersects(ls2))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
