/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL) v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

namespace ChemProV.UI
{
    public class ConstantsTableData
    {
        private string constant;

        public string Constant
        {
            get { return constant; }
            set { constant = value; }
        }

        private string symbol;

        public string Symbol
        {
            get { return symbol; }
            set { symbol = value; }
        }

        public ConstantsTableData(string constant, string symbol)
        {
            this.constant = constant;
            this.symbol = symbol;
        }
    }
}