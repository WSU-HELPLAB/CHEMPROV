using System;
using System.Collections.Generic;
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

using ChemProV.PFD;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Streams.PropertiesTable;
using ChemProV.PFD.Streams.PropertiesTable.Chemical;

using ChemProV.Validation;
using ChemProV.Validation.Rules.Adapters.Table;

namespace ChemProV.Validation.Rules
{
    public class ReactorProcessUnitRule : GenericProcessUnitRule
    {
        /// <summary>
        /// This overrides the GenericProcessUnitRules tally function as this tallies the elements not the compounds themselves
        /// </summary>
        /// <param name="streams"></param>
        /// <returns></returns>
        protected override Dictionary<string, StreamComponent> TallyCompounds(IList<IStream> streams)
        {
            Dictionary<string, StreamComponent> compounds = new Dictionary<string, StreamComponent>(5);

            //tally up flow rates for each compound
            foreach (IStream stream in streams)
            {
                ITableAdapter tableAdapter = TableAdapterFactory.CreateTableAdapter(stream.Table);

                //start at i value = 1 as we're assuming that 1 is the header row, which we don't
                //check in this particular rule (see CheckOverallFlowRate())
                for (int i = 1; i < tableAdapter.GetRowCount(); i++)
                {
                    string compound = tableAdapter.GetCompoundAtRow(i);
                    string quantity = tableAdapter.GetQuantityAtRow(i);
                    string units = tableAdapter.GetUnitAtRow(i);

                    if (compound != null)
                    {
                        List<KeyValuePair<Element, int>> elements = CompoundFactory.GetElementsOfCompound(compound);

                        //For the keyValuePair Element stores the name of the element and the int is the number of that
                        //perticular element in the compound example compound = water
                        //elements = <Hydrogren, 2>, <Oxygen, 1>
                        foreach (KeyValuePair<Element, int> element in elements)
                        {
                            if (!compounds.ContainsKey(element.Key.Name))
                            {
                                compounds[element.Key.Name] = new StreamComponent();
                                compounds[element.Key.Name].Name = element.Key.Name;
                            }

                            double numMoles;

                            try
                            {
                                numMoles = double.Parse(quantity);

                                compounds[element.Key.Name].AddValue(numMoles * element.Value, units);
                            }
                            catch
                            {
                                compounds[element.Key.Name].AddValue(quantity, units);
                            }
                        }
                    }
                }
            }
            return compounds;
        }

        /// <summary>
        /// This overrides the base class CheckRule function to extend it to do CheckMoles as well
        /// </summary>
        public override void CheckRule()
        {
            ValidationResult result = ValidationResult.Empty;

            result = CheckMoles();
            if (!result.IsEmpty)
            {
                base.ValidationResults.Add(result);
                return;
            }

            //This calls the CheckRule from GenericProcessUnit and it returns to here.
            base.CheckRule();
        }

        /// <summary>
        /// This checks to make sure each unit is moles or moles per minute uses target which is from the base class
        /// </summary>
        /// <returns></returns>
        private ValidationResult CheckMoles()
        {
            ValidationResult result = ValidationResult.Empty;

            result = CheckMoles(target.IncomingStreams);

            if (result != ValidationResult.Empty)
            {
                return result;
            }
            else
            {
                return CheckMoles(target.OutgoingStreams);
            }
        }

        /// <summary>
        /// This is our helper function to CheckMoles it takes a list of streams and checks to make sure every row in each table is moles or moles per minute
        /// </summary>
        /// <param name="streams"></param>
        /// <returns></returns>
        private ValidationResult CheckMoles(IList<IStream> streams)
        {
            ValidationResult result = ValidationResult.Empty;
            if (streams.Count > 0)
            {
                foreach (IStream stream in streams)
                {
                    ITableAdapter tableAdapter = TableAdapterFactory.CreateTableAdapter(stream.Table);
                    int i = 0;
                    while( i < tableAdapter.GetRowCount())
                    {
                        string units = tableAdapter.GetUnitAtRow(i);
                        if (units != "moles" && units != "moles per minute")
                        {
                            return new ValidationResult(stream.Table, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Not_In_Moles));
                        }
                        i++;
                    }
                }
            }
            else if (target.OutgoingStreams.Count > 0)
            {
            }
            else
            {
                //if we don't have any incoming or outgoing streams, return with no errors as 
                //we don't have anything to check
                return ValidationResult.Empty;
            }

            return result;
        }
    }
}
