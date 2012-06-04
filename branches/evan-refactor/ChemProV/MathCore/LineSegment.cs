/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

using System;
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
    /// <summary>
    /// Represents a line segment in 2-dimensional space. The pixel coordinate system is assumed, 
    /// although the code pretty much works with standard cartesians coordinates too.
    /// </summary>
    public struct LineSegment
    {
        private Vector m_a, m_b;

        public LineSegment(Point a, Point b)
        {
            m_a = new Vector(a.X, a.Y);
            m_b = new Vector(b.X, b.Y);
        }

        public LineSegment(Vector a, Vector b)
        {
            m_a = a;
            m_b = b;
        }

        public Vector A
        {
            get
            {
                return m_a;
            }
        }

        public Vector B
        {
            get
            {
                return m_b;
            }
        }

        /// <summary>
        /// Returns point B - point A, which is a vector pointing in the direction of 
        /// the segment, from point A to point B.
        /// </summary>
        public Vector Direction
        {
            get
            {
                return m_b - m_a;
            }
        }

        /// <summary>
        /// Gets the point on this line segment that is closest to the specified point.
        /// </summary>
        public Vector GetClosestPoint(Vector point)
        {
            Vector ptMinusA = point - m_a;
            
            // Handle degeneracy case
            if (m_a.Equals(m_b))
            {
                return m_a;
            }

            double sp = (point - m_a).ScalarProjectOnto(Direction);
            if (sp <= 0.0)
            {
                return m_a;
            }
            else if (sp >= 1.0)
            {
                return m_b;
            }

            return Vector.Project(ptMinusA, Direction);
        }

        /// <summary>
        /// Gets the unsigned distance between the specified point and this line
        /// </summary>
        public double GetDistance(Point point)
        {
            Vector pt = new Vector(point);
            Vector v = pt - m_a;
            double adj = v.ScalarProjectOnto(Direction);
            if (adj < 0.0 || adj > Length)
            {
                // This means the closest point is an endpoint
                return Math.Min(v.Length, (pt - m_b).Length);
            }

            double hyp = v.Length;
            // opp^2 + adj^2 = hyp^2
            // opp^2 = hyp^2 - adj^2
            double oppSqr = hyp * hyp - adj * adj;
            if (oppSqr <= 0.0)
            {
                return 0.0;
            }
            return Math.Sqrt(oppSqr);
        }

        public Point GetPointA()
        {
            return new Point(m_a.X, m_a.Y);
        }

        public Point GetPointB()
        {
            return new Point(m_b.X, m_b.Y);
        }

        public bool Intersects(LineSegment other)
        {
            Vector v = new Vector();
            return Intersects(other, ref v);
        }

        public bool Intersects(LineSegment other, ref Vector pointOfIntersection)
        {
            // Build directional vectors for both lines. Negate the ones for the
            // second segment by flipping the subtraction order.
            double dx1 = m_b.X - m_a.X;
            double dy1 = m_b.Y - m_a.Y;
            double dx2 = other.m_a.X - other.m_b.X;
            double dy2 = other.m_a.Y - other.m_b.Y;

            // Calculate determinate
            double det = (dx1 * dy2) - (dx2 * dy1);

            // If the determinate is zero then there is no intersection
            if (0.0 == det)
            {
                return false;
            }

            // Calculate origin differences
            double ox = other.m_a.X - m_a.X;
            double oy = other.m_a.Y - m_a.Y;

            // Calculate t1 and test bounds
            double t1 = (ox * dy2 - dx2 * oy) / det;
            if (t1 < 0.0 || t1 > 1.0)
            {
                return false;
            }

            // Calculate t2 and test bounds
            double t2 = (dx1 * oy - ox * dy1) / det;
            if (t2 < 0.0f || t2 > 1.0f)
            {
                return false;
            }

            // Set the intersection point
            dx1 = m_b.X - m_a.X;
            dy1 = m_b.Y - m_a.Y;
            pointOfIntersection = new Vector(m_a.X + t1 * dx1, m_a.Y + t1 * dy1);

            return true;
        }

        /// <summary>
        /// Gets the length of the line segment
        /// </summary>
        public double Length
        {
            get
            {
                return (m_a - m_b).Length;
            }
        }
    }
}
