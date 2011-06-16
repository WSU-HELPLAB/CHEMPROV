using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ChemProV.Validation.Rules
{
    public class NullRule : IRule
    {
        private ObservableCollection<ValidationResult> results = new ObservableCollection<ValidationResult>();

        public void CheckRule()
        {
            
        }

        public System.Collections.ObjectModel.ObservableCollection<ValidationResult> ValidationResults
        {
            get { return results; }
        }

        public object Target
        {
            get
            {
                return null;
            }
            set
            {
                
            }
        }
    }
}
