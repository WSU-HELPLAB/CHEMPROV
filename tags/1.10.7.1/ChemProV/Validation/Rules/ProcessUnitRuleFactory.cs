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

namespace ChemProV.Validation.Rules
{
    public class ProcessUnitRuleFactory
    {
        private ProcessUnitRuleFactory()
        {
        }

        public static GenericProcessUnitRule GetProcessUnitRule(IProcessUnit pu)
        {
            GenericProcessUnitRule puRule; 

            if(pu.Description == "Reactor")
            {
                puRule = new ReactorProcessUnitRule();
            }
            else
            {
                puRule = new GenericProcessUnitRule();
            }
            return puRule;
        }
    }
}
