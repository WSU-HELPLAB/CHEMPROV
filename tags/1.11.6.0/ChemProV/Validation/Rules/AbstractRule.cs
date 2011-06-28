/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/
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
using System.Collections.Generic;

namespace ChemProV.Validation.Rules
{
    /// <summary>
    /// This contains all of the ruleNames.
    /// </summary>
    public enum RuleName
    {
        EquationNameValidationRule,
        EquationUnitsSameRule,
        EquationCompoundSameRule,
        EquationPercentUsageRule,
        TableUnitsSameRule,
        TableQuantityRule,
        ProcessUnitConservationRule,
        SolvabilityStatus
    };

    /// <summary>
    /// This is an abstract class that inherients from IRule so addError could be defined once.
    /// </summary>
    public abstract class AbstractRule : IRule
    {

        public abstract void CheckRule();
        private Dictionary<object, List<Tuple<RuleName, string>>> dictOfErrors;

        public Dictionary<object, List<Tuple<RuleName, string>>> DictOfErrors
        {
            get
            {
                return dictOfErrors;
            }
        }

        public AbstractRule()
        {
            dictOfErrors = new Dictionary<object, List<Tuple<RuleName, string>>>();
        }

        /// <summary>
        /// This created a new entry for our DictOfErrors and then adds it to it.
        /// </summary>
        /// <param name="target">Target that broke the rule</param>
        /// <param name="errorName">Name of the rule broken</param>
        /// <param name="errorMessage">the errorMessage associated with that error</param>
        public void addError(object target, RuleName errorName, string errorMessage)
        {
            List<Tuple<RuleName, string>> temp = new List<Tuple<RuleName,string>>();
            if (DictOfErrors.TryGetValue(target, out temp))
            {
                temp.Add(new Tuple<RuleName, string>(errorName, errorMessage));
                DictOfErrors.Remove(target);
            }
            else
            {
                temp = new List<Tuple<RuleName, string>>();
                temp.Add(new Tuple<RuleName, string>(errorName, errorMessage));
            }
            DictOfErrors.Add(target, temp);
        }
    }
}
