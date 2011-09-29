/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

namespace ChemProV.PFD.EquationEditor.Tokens
{
    public class VariableToken : IEquationToken
    {
        public string Value
        {
            get;
            set;
        }

        public VariableToken(string text)
        {
            Value = text;
        }
    }
}