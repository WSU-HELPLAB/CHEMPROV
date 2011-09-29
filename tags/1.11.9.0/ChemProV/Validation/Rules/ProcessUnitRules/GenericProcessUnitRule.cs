/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Streams.PropertiesWindow;

using ChemProV.Validation.Rules.Adapters.Table;

namespace ChemProV.Validation.Rules.ProcessUnitRules
{
    /// <summary>
    /// This is the GenericProcessUnitRule, it checks the basic rules for IProcessUnits
    /// </summary>
    public class GenericProcessUnitRule : IRule
    {
        /// <summary>
        /// This is a reference to the IProcessUnit being checked
        /// </summary>
        protected IProcessUnit target;

        /// <summary>
        /// List of compounds coming into the target IProcessUnit.  Declared as
        /// a instance var in order to reduce the number of times that the
        /// dictionary will have to be built.
        /// </summary>
        protected Dictionary<string, StreamComponent> incomingCompounds;

        /// <summary>
        /// List of compounds coming into the target IProcessUnit.  Declared as
        /// a instance var in order to reduce the number of times that the
        /// dictionary will have to be built.
        /// </summary>
        protected Dictionary<string, StreamComponent> outgoingCompounds;

        protected ObservableCollection<ValidationResult> results;

        /// <summary>
        /// This is the constructor for the GenericProcessUnitRule
        /// </summary>
        public GenericProcessUnitRule()
        {
            results = new ObservableCollection<ValidationResult>();
        }

        /// <summary>
        /// Called to perform the underlying rule validation logic
        /// </summary>
        public virtual void CheckRule()
        {
            //clear any prevous messages
            results.Clear();

            ValidationResult result = ValidationResult.Empty;

            //check overall flow rates
            result = CheckOverallFlowRate();
            if (!result.IsEmpty)
            {
                results.Add(result);
                return;
            }

            //check overall units
            result = CheckOverallUnits();
            if (!result.IsEmpty)
            {
                results.Add(result);
                return;
            }

            //make sure that we're accounting for everything coming in to the PU
            result = CheckIncomingCompoundBalance();
            if (!result.IsEmpty)
            {
                results.Add(result);
                return;
            }

            //make sure that we're accounting for everything going out of the PU
            result = CheckOutgoingCompoundBalance();
            if (!result.IsEmpty)
            {
                results.Add(result);
                return;
            }

            //check individual compound flow rates
            result = CheckCompoundFlowRate();
            if (!result.IsEmpty)
            {
                results.Add(result);
                return;
            }
        }

        /// <summary>
        /// Checks to make sure that each compound quantity is conserved
        /// </summary>
        /// <returns></returns>
        protected virtual ValidationResult CheckCompoundFlowRate()
        {
            //holds the components that aren't equal
            List<string> unequalComponents = new List<string>();
            foreach (string key in incomingCompounds.Keys)
            {
                if (incomingCompounds[key].CompareTo(outgoingCompounds[key]) != 0)
                {
                    unequalComponents.Add(key);
                }
            }

            //if the number of unequal compounds is anything greater than zero, then we have a
            //problem
            if (unequalComponents.Count != 0)
            {
                string message = ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Individual_Flowrate_Mismatch);

                //this should hit both incoming and outgoing streams, so we need to create a new
                //list that merges the two streams
                List<IStream> mergedList = new List<IStream>(target.IncomingStreams.Count + target.OutgoingStreams.Count);
                foreach (IStream stream in target.IncomingStreams)
                {
                    mergedList.Add(stream);
                }
                foreach (IStream stream in target.OutgoingStreams)
                {
                    mergedList.Add(stream);
                }

                //with the lists merged, we can now return the correct result
                return new ValidationResult(mergedList, message);
            }

            //if we've gotten this far, then we must be good to go
            return ValidationResult.Empty;
        }

        /// <summary>
        /// Checks to make sure that everything that enters the process unit also leaves the
        /// process unit
        /// </summary>
        /// <returns></returns>
        protected ValidationResult CheckIncomingCompoundBalance()
        {
            //make sure that we have the same amount and types of compounds
            List<string> missingOutgoingCompounds = new List<string>();

            //check the incoming list against the outgoing list
            foreach (string key in incomingCompounds.Keys)
            {
                if (!outgoingCompounds.ContainsKey(key))
                {
                    missingOutgoingCompounds.Add(key);
                }
            }

            //if the count is greater than one, then we have a problem
            if (missingOutgoingCompounds.Count > 0)
            {
                string message = ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Missing_Incoming_Compounds, missingOutgoingCompounds.ToArray());
                return new ValidationResult(target.IncomingStreams, message);
            }

            //otherwise, we're in the clear
            return ValidationResult.Empty;
        }

        /// <summary>
        /// Checks to make sure that everything leaving a process unit also enters that
        /// process unit.
        /// </summary>
        /// <returns></returns>
        protected ValidationResult CheckOutgoingCompoundBalance()
        {
            //make sure that we have the same amount and types of compounds
            List<string> missingIncomingCompounds = new List<string>();

            //check outgoing list against the incoming list for holes
            foreach (string key in outgoingCompounds.Keys)
            {
                if (!incomingCompounds.ContainsKey(key))
                {
                    missingIncomingCompounds.Add(key);
                }
            }

            //if the count is greater than one, then we have a problem
            if (missingIncomingCompounds.Count > 0)
            {
                string message = ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Missing_Outgoing_Compounds, missingIncomingCompounds.ToArray());
                return new ValidationResult(target.OutgoingStreams, message);
            }

            //otherwise, we're in the clear
            return ValidationResult.Empty;
        }

        /// <summary>
        /// This is the rule that checks to see if the Overall Units are the same
        /// </summary>
        /// <returns></returns>
        protected ValidationResult CheckOverallUnits()
        {
            //loop through all incoming & outgoing streams.  If one of the overall units doesn't match,
            //then we have a problem

            //prime the loop
            string comparisonUnit = "";
            bool mismatchFound = false;
            if (target.IncomingStreams.Count > 0)
            {
                int i = 0;
                while (i < target.OutgoingStreams.Count)
                {
                    ITableAdapter tableAdapter = TableAdapterFactory.CreateTableAdapter(target.IncomingStreams[0].Table);
                    if (tableAdapter.GetTableType() == TableType.Chemcial)
                    {
                        comparisonUnit = tableAdapter.GetUnitAtRow(0);
                        break;
                    }
                    i++;
                }
            }
            else if (target.OutgoingStreams.Count > 0)
            {
                int i = 0;
                while (i < target.OutgoingStreams.Count)
                {
                    ITableAdapter tableAdapter = TableAdapterFactory.CreateTableAdapter(target.OutgoingStreams[0].Table);
                    if (tableAdapter.GetTableType() == TableType.Chemcial)
                    {
                        comparisonUnit = tableAdapter.GetUnitAtRow(0);
                        break;
                    }
                    i++;
                }
            }
            if (comparisonUnit == "")
            {
                //if we don't have any incoming or outgoing chemical streams, return with no errors as
                //we don't have anything to check
                return ValidationResult.Empty;
            }

            //if we found a mismatch, we need to target all streams, so create a master
            //stream list
            List<IStream> masterList = new List<IStream>();

            //loop through incoming & outgoing streams
            foreach (IStream stream in target.IncomingStreams)
            {
                ITableAdapter tableAdapter = TableAdapterFactory.CreateTableAdapter(stream.Table);

                if (tableAdapter.GetTableType() == TableType.Chemcial)
                {
                    //add to the master list
                    masterList.Add(stream);

                    //if units aren't the same, then we need to throw an error
                    if (comparisonUnit.CompareTo(tableAdapter.GetUnitAtRow(0)) != 0)
                    {
                        mismatchFound = true;
                    }
                }
            }
            foreach (IStream stream in target.OutgoingStreams)
            {
                ITableAdapter tableAdapter = TableAdapterFactory.CreateTableAdapter(stream.Table);

                if (tableAdapter.GetTableType() == TableType.Chemcial)
                {
                    //add to the master list
                    masterList.Add(stream);

                    //if units aren't the same, then we need to throw an error
                    if (comparisonUnit.CompareTo(tableAdapter.GetUnitAtRow(0)) != 0)
                    {
                        mismatchFound = true;
                    }
                }
            }

            //did we trip our flag?
            if (mismatchFound)
            {
                string message = ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Overall_Units_Mismatch);
                return new ValidationResult(masterList, message);
            }

            return ValidationResult.Empty;
        }

        /// <summary>
        /// Checks to make sure that the flow rate of the overall streams match
        /// </summary>
        /// <returns></returns>
        protected virtual ValidationResult CheckOverallFlowRate()
        {
            //variables used to store incoming and outgoing flow rates.
            StreamComponent incomingFlow = TallyOverallFlowRate(target.IncomingStreams);
            StreamComponent outogingFlow = TallyOverallFlowRate(target.OutgoingStreams);

            //if they're equal then we're good, otherwise return an error
            if (incomingFlow.CompareTo(outogingFlow) == 0)
            {
                return ValidationResult.Empty;
            }
            else
            {
                string message = ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Overall_Flowrate_Mismatch);
                return new ValidationResult(target.OutgoingStreams, message);
            }
        }

        /// <summary>
        /// Nice helper function that tallies up compounds for a set of streams
        /// </summary>
        /// <param name="streams"></param>
        /// <returns></returns>
        protected virtual Dictionary<string, StreamComponent> TallyCompounds(IList<IStream> streams)
        {
            Dictionary<string, StreamComponent> compounds = new Dictionary<string, StreamComponent>(5);

            //tally up flow rates for each compound
            foreach (IStream stream in streams)
            {
                ITableAdapter tableAdapter = TableAdapterFactory.CreateTableAdapter(stream.Table);

                //start at index value = 1 as we're assuming that 1 is the header row, which we don't
                //check in this particular rule (see CheckOverallFlowRate())
                for (int i = 1; i < tableAdapter.GetRowCount(); i++)
                {
                    string compound = tableAdapter.GetCompoundAtRow(i);
                    string quantity = tableAdapter.GetQuantityAtRow(i);
                    string units = tableAdapter.GetUnitAtRow(i);

                    if (compound != null)
                    {
                        if (!compounds.ContainsKey(compound))
                        {
                            compounds[compound] = new StreamComponent();
                            compounds[compound].Name = compound;
                        }
                        compounds[compound].AddValue(quantity, units);
                    }
                }
            }
            return compounds;
        }

        /// <summary>
        /// Returns the overall flow rate for a set of streams
        /// </summary>
        /// <param name="streams">This is a list of streams whos Overall will be add together</param>
        /// <returns>Returns a StreamComponent which contains the results</returns>
        private StreamComponent TallyOverallFlowRate(IList<IStream> streams)
        {
            StreamComponent component = new StreamComponent();

            //tally up flow rates coming into this compound
            foreach (IStream stream in streams)
            {
                ITableAdapter tableAdapter = TableAdapterFactory.CreateTableAdapter(stream.Table);
                if (tableAdapter.GetTableType() == TableType.Chemcial)
                {
                    component.AddValue(tableAdapter.GetQuantityAtRow(0));
                }
            }
            return component;
        }

        /// <summary>
        /// Shorthand method for calculating equivalance between two doubles.  Uses a tolerance
        /// for more reliable checking.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="tolerance">The tolerance that we'll check to.  Defaults to 0.0001</param>
        /// <returns></returns>
        private bool isDoubleEqual(double a, double b, double tolerance = 0.0001)
        {
            if ((a - tolerance) < b
                &&
                (a + tolerance) > b
              )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// A list of validation results associated with a particular rule
        /// </summary>
        public ObservableCollection<ValidationResult> ValidationResults
        {
            get
            {
                return results;
            }
        }

        /// <summary>
        /// Gets or set's the rule's target.  Note that in this implementation, the target is assumed
        /// to be an IProcessUnit
        /// </summary>
        public object Target
        {
            get
            {
                return target;
            }
            set
            {
                target = value as IProcessUnit;

                if (target != null)
                {
                    //must regenerate the list of incoming / outgoing compounds
                    incomingCompounds = TallyCompounds(target.IncomingStreams);
                    outgoingCompounds = TallyCompounds(target.OutgoingStreams);
                }
            }
        }

        /// <summary>
        /// Internal class used by the parent class
        /// </summary>
        protected class StreamComponent : IComparable
        {
            /// <summary>
            /// This is set to true whenever a WildCard is found
            /// </summary>
            public bool WildCardFound
            {
                get;
                set;
            }

            private double value;

            /// <summary>
            /// This is the total so far
            /// </summary>
            public double Value
            {
                get
                {
                    return value;
                }
            }

            /// <summary>
            /// This is the name of the compound
            /// </summary>
            public string Name
            {
                get;
                set;
            }

            /// <summary>
            /// This converts units to a number and adds it to value, unless it is a ? then sets whildcard to true
            /// </summary>
            /// <param name="amount">This the desired total</param>
            /// <param name="units">a string which must be an double represented as a string or a "?"</param>
            public void AddValue(object amount, string units = "")
            {
                //if we're trying to add a wildcard
                if (amount.ToString().CompareTo("?") == 0)
                {
                    WildCardFound = true;
                    return;
                }

                //if the units are a wildcard
                if (units.CompareTo("?") == 0)
                {
                    WildCardFound = true;
                    return;
                }

                //else, try to parse
                double temp = 0.0;
                if (Double.TryParse(amount.ToString(), out temp))
                {
                    value += temp;
                }
            }

            /// <summary>
            /// This sets value to 0
            /// </summary>
            public void ClearValue()
            {
                value = 0.0;
            }

            /// <summary>
            /// This compares the passed in object to itself in regards to the value.  If both have wild cards then true,
            /// if 1 does and that one has a bigger value then true and if neither have wildcards then if values are the same
            /// then true otherwise it is false.
            /// </summary>
            /// <param name="obj">the object being compared too</param>
            /// <returns>-1 if obj is not a StreamComponent or false, 0 if true</returns>
            public int CompareTo(object obj)
            {
                //return -1 if we're not comparing the same thing so GTFO
                if (!(obj is StreamComponent))
                {
                    return -1;
                }

                //comparing stream weights is kind of cookey.  In the simplest case,
                //all we need to do is compare two double values.  However, with the common
                //case, we deal with wildcards, which can be any value.  When dealing
                //with wildcards, we return equal when the supplied object (THE one supplied
                //in the parameter list) is larger than the other object.  Otherwise, return false.
                StreamComponent other = obj as StreamComponent;
                if (!this.WildCardFound && !other.WildCardFound)
                {
                    //case 1: just compare double values.
                    return this.Value.CompareTo(other.Value);
                }
                else if (!this.WildCardFound && other.WildCardFound)
                {
                    //case 2: if the other's widcard is set, make sure that it's larger
                    if (other.Value < this.Value)
                    {
                        return 0;
                    }
                    else
                    {
                        return -1;
                    }
                }
                else if (this.WildCardFound && !other.WildCardFound)
                {
                    //case 3: If we don't know our value, we must be equal!
                    if (this.Value < other.Value)
                    {
                        return 0;
                    }
                    else
                    {
                        return -1;
                    }
                }
                else if (this.WildCardFound && other.WildCardFound)
                {
                    //case 4: wildcards on both sides no clue so return true
                    return 0;
                }
                //catch-all: return 0?
                return 0;
            }
        }
    }
}