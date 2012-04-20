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
using ChemProV.PFD.EquationEditor.Models;
using System.Collections.ObjectModel;

namespace ChemProV.Validation.Rules.EquationRules
{
    public class EquationRule : IRule
    {
        private EquationModel _target;

        public void CheckRule()
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<ValidationResult> ValidationResults
        {
            get { throw new NotImplementedException(); }
        }

        public object Target
        {
            get
            {
                return _target;
            }
            set
            {
                _target = value as EquationModel;
            }
        }
    }
}
