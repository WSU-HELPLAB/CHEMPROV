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

namespace ChemProV.PFD.EquationEditor.Models
{
    public class EquationType : IComparable
    {
        public EquationTypeClassification Classification { get; set; }
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public EquationType()
        {
            Classification = EquationTypeClassification.Total;
            Name = "Overall";
        }

        public EquationType(EquationTypeClassification classification, string name)
        {
            Classification = classification;
            Name = name;
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
    }
}
