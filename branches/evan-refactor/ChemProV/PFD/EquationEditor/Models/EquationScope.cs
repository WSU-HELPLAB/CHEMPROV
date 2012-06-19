/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;

namespace ChemProV.PFD.EquationEditor.Models
{
    public enum EquationScopeClassification
    {
        Overall,
        SubProcess,
        SingleUnit,
        Unknown
    }

    public class EquationScope : IEquationModifier, IComparable, IEquatable<EquationScope>
    {
        public EquationScopeClassification Classification { get; set; }
        public string Name { get; set; }
        public int ClassificationId
        {
            get
            {
                return (int)Classification;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public EquationScope()
        {
            Classification = EquationScopeClassification.Overall;
            Name = "Overall";
        }

        public EquationScope(EquationScope scope)
        {
            Classification = scope.Classification;
            Name = scope.Name;
        }

        public EquationScope(EquationScopeClassification classification, string name)
        {
            Classification = classification;
            Name = name;
        }

        public int CompareTo(object obj)
        {
            EquationScope other = obj as EquationScope;
            if (this.Name.CompareTo(other.Name) == 0)
            {
                return this.Classification.CompareTo(other.Classification);
            }
            else
            {
                return this.Name.CompareTo(other.Name);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (this.CompareTo(obj) == 0)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(EquationScope other)
        {
            if (this.CompareTo(other) == 0)
            {
                return true;
            }
            return false;
        }
    }
}
