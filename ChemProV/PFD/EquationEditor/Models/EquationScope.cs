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
    public class EquationScope
    {
        public EquationScopeClassification Classification { get; set; }
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public EquationScope()
        {
            Classification = EquationScopeClassification.Overall;
            Name = "Overall";
        }

        public EquationScope(EquationScopeClassification classification, string name)
        {
            Classification = classification;
            Name = name;
        }
    }
}
