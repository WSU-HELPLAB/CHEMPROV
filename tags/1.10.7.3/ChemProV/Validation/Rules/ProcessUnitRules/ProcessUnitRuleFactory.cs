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

using ChemProV.PFD.ProcessUnits;

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
        /// This returns the rule that needs to be checked for the IProcessUnit
        /// Currently it returns the ReactorProcessUnitRule if pu is a reactor otherwise
        /// it returns the GenericProcessUnitRule
        /// </summary>
        /// <param name="pu">The process unit that will be checked with the rule returned</param>
        /// <returns>GenericProcessUnitRule which could be a ReactorProcessUnitRule since it inherients from GenericProcessUnitRule</returns>
        public static GenericProcessUnitRule GetProcessUnitRule(IProcessUnit pu)
        {
            GenericProcessUnitRule puRule; 

            if(pu.Description == ProcessUnitDescriptions.Reactor)
            {
                puRule = new ReactorProcessUnitRule();
            }
            else if (pu.Description == ProcessUnitDescriptions.HeatExchangerNoUtility)
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
