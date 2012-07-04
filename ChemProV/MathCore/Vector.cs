/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

// Keep this class UI-independent. Don't use things like System.Windows.Point or anything 
// else that requires Silverlight, WPF, etc.
using System;

namespace ChemProV.MathCore
{
    /// <summary>
    /// Represents a mutable vector in 2-dimensional space.
    /// </summary>
    public struct Vector
    {
        public double X;

        public double Y;

        public Vector(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double DotProduct(Vector vector)
        {
            return (vector.X * X + vector.Y * Y);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector)
            {
                Vector other = (Vector)obj;
                return (other.X == this.X && other.Y == this.Y);
            }
            return false;
        }

        public bool Equals(Vector other)
        {
            return (other.X == this.X && other.Y == this.Y);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public static Vector GetPerpendicular1(Vector vector)
        {
            return new Vector(vector.Y, -vector.X);
        }

        public static Vector GetPerpendicular2(Vector vector)
        {
            return new Vector(-vector.Y, vector.X);
        }

        public double Length
        {
            get
            {
                return Math.Sqrt(X * X + Y * Y); 
            }
        }

        public static Vector Normalize(Vector normalizeMe)
        {
            double len = normalizeMe.Length;
            if (0.0 == len)
            {
                return new Vector();
            }
            return normalizeMe / len;
        }

        public static Vector operator -(Vector lhs, Vector rhs)
        {
            return new Vector(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public static Vector operator *(Vector lhs, double rhs)
        {
            return new Vector(lhs.X * rhs, lhs.Y * rhs);
        }

        public static Vector operator /(Vector lhs, double rhs)
        {
            return new Vector(lhs.X / rhs, lhs.Y / rhs);
        }

        public static Vector operator +(Vector lhs, Vector rhs)
        {
            return new Vector(lhs.X + rhs.X, lhs.Y + rhs.Y);
        }

        public static Vector Project(Vector me, Vector ontome)
        {
            ontome = Vector.Normalize(ontome);
            ontome *= (me.DotProduct(ontome));
            return ontome;
        }

        public double ScalarProjectOnto(Vector ontome)
        {
            return (X * ontome.X + Y * ontome.Y) / ontome.Length;
        }

        public override string ToString()
        {
            return X.ToString() + "," + Y.ToString();
        }
    }
}
