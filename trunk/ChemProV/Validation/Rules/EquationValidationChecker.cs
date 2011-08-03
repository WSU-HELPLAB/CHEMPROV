/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
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
using ChemProV.PFD.EquationEditor.Tokens;

namespace ChemProV.Validation.Rules
{
    public class EquationValidationChecker
    {
        private ObservableCollection<IEquationToken> equation;

        public Dictionary<string, ChemicalStreamData> DictionaryOfTableData;

        public List<ObservableCollection<ChemicalStreamData>> ListofTablesData;

        public ObservableCollection<IEquationToken> Equation
        {
            get { return equation; }
            set { equation = value; }
        }

        private List<string> variableNames;

        public EquationValidationChecker(ObservableCollection<IEquationToken> equation, List<string> variableNames)
        {
            this.equation = equation;
            this.variableNames = variableNames;
        }
        
        public ValidationResult CheckRule()
        {
            ValidationResult vr;

            vr = NameValidation();

            if (!vr.IsEmpty)
            {
                return vr;
            }

            vr = percentUsage();

            if (!vr.IsEmpty)
            {
                return vr;
            }

            vr = EquationValidation();

            if (!vr.IsEmpty)
            {
                return vr;
            }

            return ValidationResult.Empty;
        }

        ValidationResult percentUsage()
        {
            int i = 0;
            while (i < equation.Count)
            {
                IEquationToken token = equation[i];
                if (token is VariableToken)
                {
                    ChemicalStreamData data;
                    try
                    {
                        DictionaryOfTableData.TryGetValue(token.Value, out data);

                        if (new UnitsFormatter().ConvertFromIntToString(data.Units) == "%")
                        {
                            if (i + 4 < equation.Count)
                            {
                                if (!(equation[i + 1].Value == "/" && equation[i + 2].Value == "100" && equation[i + 3].Value == "*" && DictionaryOfTableData.ContainsKey(equation[i + 4].Value)))
                                {
                                    return new ValidationResult(equation, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Incorrect_Use_Of_Percent));
                                }
                            }
                            else
                            {
                                return new ValidationResult(equation, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Incorrect_Use_Of_Percent));
                            }
                        }

                    }
                    catch
                    {
                        //it is a number so dont do anything
                    }
                }
                i++;
            }

            return ValidationResult.Empty;
        }

        ValidationResult EquationValidation()
        {
            if (isSumOfOneTable())
            {
                //TO DO: Check to make sure all the parts they are adding together are all members of the same table
                //Assume units are ok since they must be ok for a table to be valid
                return ValidationResult.Empty;
            }
            else if (OverallSum())
            {
                //All compounds are Overall must check units now
                if (!CheckSameUnits())
                {
                    return new ValidationResult(equation, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.More_Than_One_Unit));
                }
            }
            else
            {
                //Everything should be the same compound need to check that
                if (!CheckSameCompound())
                {
                    return new ValidationResult(equation, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.More_Than_One_Compound));
                }

                //need to check that all units are correct
                if (!CheckSameUnits())
                {
                    return new ValidationResult(equation, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.More_Than_One_Unit));
                }
            }
            return ValidationResult.Empty;
        }

        bool CheckSameUnits()
        {
            string units = null;
            foreach (string s in variableNames)
            {
                ChemicalStreamData data;
                try
                {
                    DictionaryOfTableData.TryGetValue(s, out data);
                    string thisUnits = new UnitsFormatter().ConvertFromIntToString(data.Units);

                    if (units == null)
                    {

                        if (thisUnits != "%")
                        {
                            units = thisUnits;
                        }
                    }
                    else if (units != thisUnits && thisUnits != "%")
                    {
                        return false;
                    }
                }
                catch
                {
                    //number so dont worry about it
                }
            }
            return true;
        }

        bool CheckSameCompound()
        {
            int i = 0;
            string compound = null;
            while (i < equation.Count)
            {
                IEquationToken token = equation[i];
                if (token is VariableToken)
                {
                    ChemicalStreamData data;
                    try
                    {
                        DictionaryOfTableData.TryGetValue(token.Value, out data);
                        if (compound == null)
                        {
                            compound = new CompoundFormatter().ConvertFromIntToString(data.Compound);
                        }
                        else if (compound != new CompoundFormatter().ConvertFromIntToString(data.Compound))
                        {
                            return false;
                        }
                        if (new UnitsFormatter().ConvertFromIntToString(data.Units) == "%")
                        {
                            //This is so we skip over the percent mainly the Overal so we dont have to deal with it
                            i += 4;
                        }

                    }
                    catch
                    {
                        //just to be safe but this should never catch anything
                    }
                }
                i++;
            }

            return true;
        }


        bool isSumOfOneTable()
        {

            int equalSignLocation = findEqualsSign(equation);
            ObservableCollection<IEquationToken> lhs = new ObservableCollection<IEquationToken>(equation);
            ObservableCollection<IEquationToken> rhs = new ObservableCollection<IEquationToken>(equation);

            if (equalSignLocation != 0)
            {

                //Now for lhs remove everything at and past the location of the equalSign
                for (int j = equalSignLocation; j < lhs.Count; j++)
                {
                    lhs.RemoveAt(j);
                }
                //Reomve everything from 0 to equalSignLocation + 1 to get rid of the left hind side and the equalSign .
                for (int j = 0; j < equalSignLocation + 1; j++)
                {
                    rhs.RemoveAt(j);
                }

                if (lhs.Count == 1)
                {
                    ChemicalStreamData data;
                    try
                    {
                        DictionaryOfTableData.TryGetValue(lhs[0].Value, out data);

                        if ("Overall" == new CompoundFormatter().ConvertFromIntToString(data.Compound))
                        {
                            //ok so definently adding the parts of a table to get the whole
                            return true;
                        }
                    }
                    catch
                    {
                        //not a valid equation??
                    }
                }
                else if (rhs.Count == 1)
                {
                    ChemicalStreamData data;
                    try
                    {
                        DictionaryOfTableData.TryGetValue(rhs[0].Value, out data);

                        if ("Overall" == new CompoundFormatter().ConvertFromIntToString(data.Compound))
                        {
                            //ok so definently adding the parts of a table to get the whole
                            return true;
                        }
                    }
                    catch
                    {
                        //not a valid equation??
                    }
                }
            }
            return false;
        }
        private bool OverallSum()
        {
            foreach (string s in variableNames)
            {
                ChemicalStreamData data;
                try
                {
                    DictionaryOfTableData.TryGetValue(s, out data);
                    if ("Overall" != new CompoundFormatter().ConvertFromIntToString(data.Compound))
                    {
                        return false;
                    }
                }
                catch
                {
                    //just to be safe but this should never catch anything
                }
            }
            return true;
        }


        private int findEqualsSign(ObservableCollection<IEquationToken> equation)
        {
            int i = 0;
            foreach (IEquationToken token in equation)
            {
                if (token.Value == "=")
                {
                    return i;
                }
                i++;
            }
            return (0);
        }


        ValidationResult NameValidation()
        {
            List<string> invalidNames = new List<string>();

            foreach (string s in variableNames)
            {
                if (!DictionaryOfTableData.ContainsKey(s))
                {
                    //so not in our dictionary and not a number so not valid
                    invalidNames.Add(s);
                }
            }

            if (invalidNames.Count > 0)
            {
                return new ValidationResult(equation, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Equation_Variable_Not_In_Tables, invalidNames.ToArray()));
            }
            else
            {
                return ValidationResult.Empty;
            }
        }


    }
}