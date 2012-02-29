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
    public class EquationType
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
    }
}
