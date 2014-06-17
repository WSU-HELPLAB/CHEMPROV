/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System.Collections.ObjectModel;
using ChemProV.Logic;
using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.Validation.Rules.Adapters.Table;

namespace ChemProV.Validation.Rules
{
    /// <summary>
    /// This is the class that checks all rules dealing with Tables
    /// </summary>
    public class TableRule : IRule
    {
        /// <summary>
        /// This is the construct for TableRule
        /// </summary>
        public TableRule()
        {
        }

        /// <summary>
        /// This checks all the rules for the table that target points too if target does not point to a table returns does not check anything
        /// </summary>
        public void CheckRule()
        {
            //Clear the list from last time
            ValidationResults.Clear();

            // Make sure the target is not null and it is an StreamPropertiesTable
            if (target != null && target is StreamPropertiesTable)
            {
                ITableAdapter tableAdapter = TableAdapterFactory.CreateTableAdapter(
                    target as StreamPropertiesTable);

                ValidationResult vr;

                if (StreamType.Chemical == (target as StreamPropertiesTable).StreamType)
                {
                    vr = unitsAreConsistant(tableAdapter);

                    if (!vr.IsEmpty)
                    {
                        ValidationResults.Add(vr);
                    }

                    vr = sumOfPartsEqualsTotalQuantity(tableAdapter);

                    if (!vr.IsEmpty)
                    {
                        ValidationResults.Add(vr);
                    }
                }
            }
        }

        /// <summary>
        /// This rule checks to see if the units are consistant
        /// </summary>
        /// <param name="tableAdapter">This is the tableAdapter that is used to get data from the table
        /// referenced within tableAdapter</param>
        /// <returns>returns empty ValidationResult if rule is not broken</returns>
        protected ValidationResult unitsAreConsistant(ITableAdapter tableAdapter)
        {
            if (tableAdapter.GetRowCount() == 1)
            {
                //Only one row so it must be true;
                return ValidationResult.Empty;
            }

            string firstRowUnits = tableAdapter.GetUnitAtRow(0);

            string secondRowUnits = tableAdapter.GetUnitAtRow(1);

            //the firstRowUnits must be equal too the secondRowUnits unless secondRowUnits is wildcard
            if (firstRowUnits != secondRowUnits && secondRowUnits != "?")
            {
                return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Inconsistant_Units, firstRowUnits, secondRowUnits));
            }

            //start index at 2
            int i = 2;

            while (i < tableAdapter.GetRowCount())
            {
                //All subrows must be in percent if the second row is or match the first and second row but we know they are equal
                if (secondRowUnits != tableAdapter.GetUnitAtRow(i))
                {
                    return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Inconsistant_Units, secondRowUnits, tableAdapter.GetUnitAtRow(i)));
                }
                i++;
            }

            return ValidationResult.Empty;
        }

        /// <summary>
        /// This adds all of the components together and make sure they equal the overal or less if a wild card is found
        /// </summary>
        /// <param name="tableAdapter">This is used to get infomation form the table that tableAdapter has as a reference</param>
        /// <returns>Empty ValidationResult is returned if it passes</returns>
        protected ValidationResult sumOfPartsEqualsTotalQuantity(ITableAdapter tableAdapter)
        {
            float overalQuantity;

            if (tableAdapter.GetRowCount() == 1)
            {
                //Only one row so it must be true;
                return ValidationResult.Empty;
            }

            //? is wildcard for percent here cheating a little
            if (tableAdapter.GetUnitAtRow(1) == "?")
            {
                //using percents so the sum must at up to 100 (100%)
                overalQuantity = 100;
            }
            else
            {
                try
                {
                    float qty;
                    //make sure quantity is a number to avoid a format exception error
                    bool isNum = float.TryParse(tableAdapter.GetQuantityAtRow(0), out qty);
                    if (isNum)
                        overalQuantity = qty;
                    else
                        return ValidationResult.Empty;
                    //overalQuantity = float.Parse(tableAdapter.GetQuantityAtRow(0));
                }
                catch
                {
                    //Not a number and the only thing the table accepts that isn't a number is ?
                    //since we do not know the total we cannot see if the sum equals the total so assume true
                    return ValidationResult.Empty;
                }
                //So didn't return so overalQuantity must be equal to the overal Quantity
            }

            //at this point overalQuantity could equal 100 or the overal Quantity but we dont care the sume of the parts must
            //equal whatever number it is.

            bool gotQuestionMark = false;
            float sumPartsQuantity = 0;
            int i = 1;

            //the adapter gets rid of the extra row
            while (i < tableAdapter.GetRowCount())
            {
                try
                {
                    float sumPart;
                    bool isNum = float.TryParse(tableAdapter.GetQuantityAtRow(i), out sumPart);
                    if (isNum)
                        sumPartsQuantity += sumPart;
                    else
                        gotQuestionMark = true;
                    //sumPartsQuantity += float.Parse(tableAdapter.GetQuantityAtRow(i));                    
                }
                catch
                {
                    //the only thing that would make the parse fail is a questionMark
                    gotQuestionMark = true;
                }
                i++;
            }

            //Fails if either the sum is gerater than the overal or if sum does not equal overal and questionMark was not found
            if ((sumPartsQuantity > overalQuantity) || (gotQuestionMark == false && !(sumPartsQuantity + .1 > overalQuantity && sumPartsQuantity - .1 < overalQuantity)))
            {
                return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Sum_Does_Not_Equal_Total_Quantity));
            }

            return ValidationResult.Empty;
        }

        private ObservableCollection<ValidationResult> results = new ObservableCollection<ValidationResult>();

        /// <summary>
        /// This is a list of all the ValidationResult that were found during this rule being checked
        /// </summary>
        public ObservableCollection<ValidationResult> ValidationResults
        {
            get { return results; }
        }

        private object target;

        /// <summary>
        /// This is the IPropertiesWindow we will be checking to see if it complies with the rule
        /// </summary>
        public object Target
        {
            get
            {
                return target;
            }
            set
            {
                target = value;
            }
        }
    }
}