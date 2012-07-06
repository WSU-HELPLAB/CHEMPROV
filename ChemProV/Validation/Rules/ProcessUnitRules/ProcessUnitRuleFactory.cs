/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using ChemProV.UI;

namespace ChemProV.Validation.Rules.ProcessUnitRules
{
    /// <summary>
    ///
    /// </summary>
    public class ProcessUnitRuleFactory
    {
        private ProcessUnitRuleFactory()
        {
        }

        /// <summary>
        /// This returns the rule that needs to be checked for the GenericProcessUnit
        /// Currently it returns the ReactorProcessUnitRule if pu is a reactor otherwise
        /// it returns the GenericProcessUnitRule
        /// </summary>
        /// <param name="pu">The process unit that will be checked with the rule returned</param>
        /// <returns>GenericProcessUnitRule which could be a ReactorProcessUnitRule since it inherients from GenericProcessUnitRule</returns>
        public static GenericProcessUnitRule GetProcessUnitRule(ProcessUnitControl pu)
        {
            GenericProcessUnitRule puRule;

            if (pu.ProcessUnit is Core.Reactor)
            {
                puRule = new ReactorProcessUnitRule();
            }
            else if (pu.ProcessUnit is Core.HeatExchangerNoUtility)
            {
                puRule = new HeatExchangerWithoutUtilityProcessUnitRule();
            }
            else
            {
                puRule = new GenericProcessUnitRule();
            }
            return puRule;
        }
    }
}