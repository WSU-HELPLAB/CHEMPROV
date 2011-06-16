/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/
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
    /// <summary>
    /// This is the NullRule, the idea is it does nothing while not breaking anything
    /// </summary>
    public class NullRule : IRule
    {
        private ObservableCollection<ValidationResult> results = new ObservableCollection<ValidationResult>();

        /// <summary>
        /// This is empty because it is the null rule;
        /// </summary>
        public void CheckRule()
        {
            
        }

        /// <summary>
        /// This will always return an empty observableCollection of ValidationResult
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<ValidationResult> ValidationResults
        {
            get { return results; }
        }

        /// <summary>
        /// This always return null and set does nothing;
        /// </summary>
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
