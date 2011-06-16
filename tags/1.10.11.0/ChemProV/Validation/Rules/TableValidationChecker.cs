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
using System.Collections.ObjectModel;
using System.Collections.Generic;

using ChemProV.PFD.Streams.PropertiesTable;
using ChemProV.PFD.Streams.PropertiesTable.Chemical;
using ChemProV.PFD;
using ChemProV.Validation.Rules.Adapters.Table;

namespace ChemProV.Validation.Rules
{
    public class ChemicalStreamPropertiesTableValidationChecker : IRule
    {
        private IPropertiesTable table;

        public IPropertiesTable Table
        {
            get { return table; }
            set { table = value; }
        }
        
        public ChemicalStreamPropertiesTableValidationChecker(ChemicalStreamPropertiesTable table)
        {
            this.table = table;
        }

        public void CheckRule()
        {
            ITableAdapter tableAdapter = TableAdapterFactory.CreateTableAdapter(table);

            ValidationResult vr;

            //Clear the list from last time
            ValidationResults.Clear();

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

        protected ValidationResult unitsAreConsistant(ITableAdapter tableAdapter)
        {
            

            //NOTE: tableAdapter gets rid of the last row for us so we can act like it isn't there
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
                return new ValidationResult(table, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Inconsistant_Units));
            }

            //start i at 2 
            int i = 2;

            while (i < tableAdapter.GetRowCount())
            {
                //All subrows must be in percent if the second row is or match the first and second row but we know they are equal
                if(secondRowUnits != tableAdapter.GetUnitAtRow(i))
                {
                    return new ValidationResult(table, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Inconsistant_Units));
                }
                i++;
            }

            return ValidationResult.Empty;
        }

        protected ValidationResult sumOfPartsEqualsTotalQuantity(ITableAdapter tableAdapter)
        {
            float overalQuantity;

            if (tableAdapter.GetRowCount() == 1)
            {
                //Only one row so it must be true;
                return ValidationResult.Empty;
            }

            //? is wildcard for percent here cheating a little
            if (tableAdapter.GetUnitAtRow(0) == "?")
            {
                //using percents so the sum must at up to 100 (100%)
                overalQuantity = 100;
            }
            else
            {
                try
                {
                    overalQuantity = float.Parse(tableAdapter.GetQuantityAtRow(0));
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
                    sumPartsQuantity += float.Parse(tableAdapter.GetQuantityAtRow(i));
                }
                catch
                {
                    //the only thing that would make the parse fail is a questionMark
                    gotQuestionMark = true;
                }
                i++;
            }

            //Fails if either the sum is gerater than the overal or if sum does not equal overal and questionMark was not found
            if ((sumPartsQuantity > overalQuantity) || (gotQuestionMark == false && sumPartsQuantity != overalQuantity))
            {
                return new ValidationResult(table, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Sum_Does_Not_Equal_Total_Quantity));   
            }

            return ValidationResult.Empty;
        }


        public ObservableCollection<ValidationResult> ValidationResults
        {
            get { throw new NotImplementedException(); }
        }

        private object target;

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
