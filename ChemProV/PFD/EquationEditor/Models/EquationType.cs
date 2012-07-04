using System;

namespace ChemProV.PFD.EquationEditor.Models
{
    public enum EquationTypeClassification
    {
        Total,
        Compound,
        Atom,
        VariableDefinition,
        Energy,
        Specification,
    }
    
    public class EquationType : IEquationModifier, IComparable
    {
        public EquationTypeClassification Classification { get; set; }
        public string Name { get; set; }
        public int ClassificationId
        {
            get
            {
                return (int)Classification;
            }
        }

        public EquationType()
        {
            Classification = EquationTypeClassification.Total;
            Name = "Total";
        }

        public EquationType(EquationTypeClassification classification, string name)
        {
            Classification = classification;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(object obj)
        {
            EquationType other = obj as EquationType;
            if(this.Name.CompareTo(other.Name) == 0)
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
    }
}
