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


using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.PFD.Streams.PropertiesWindow.Chemical;
using ChemProV.PFD;
using ChemProV.Validation.Rules.Adapters.Table;
using ChemProV.PFD.EquationEditor.Tokens;

namespace ChemProV.Validation.Rules
{
    /// <summary>
    /// This checks the semantics of an equation, are the variable names valid, are percents used correctly, is the equation valid
    /// </summary>
    public class EquationSemanticsRule
    {
        private ObservableCollection<IEquationToken> equation;

        /// <summary>
        /// This is Dictionary that for the key uses a label name and for the data uses
        /// GenericTableData which is the data associated with that label
        /// </summary>
        public Dictionary<string, GenericTableData> DictionaryOfTableData;

        /// <summary>
        /// This is parsed text for the equation
        /// </summary>
        public ObservableCollection<IEquationToken> Equation
        {
            get { return equation; }
            set { equation = value; }
        }

        private object target;

        /// <summary>
        /// This is the Equation being checked.
        /// </summary>
        public object Target
        {
            get { return target; }
            set { target = value; }
        }

        public List<Tuple<object, ObservableCollection<IEquationToken>>> ChemicalEquations;
        public List<Tuple<object, ObservableCollection<IEquationToken>>> HeatEquations;

        private List<string> variableNames;

        /// <summary>
        /// This is the constructor for the EquationSemanticRule
        /// </summary>
        /// <param name="equation">the parsed text for the equation to be checked</param>
        /// <param name="variableNames">the variableNames used in equation</param>
        /// <param name="target">a reference to the Equation itself</param>
        public EquationSemanticsRule(ObservableCollection<IEquationToken> equation, List<string> variableNames, object target)
        {
            this.equation = equation;
            this.variableNames = variableNames;
            this.target = target;
        }

        /// <summary>
        /// This checks all the rules for the semantics of an equation,
        /// valid variable names valid
        /// correct use of percents
        /// valid equation, must be sum of one table equals the overal, the sum of a compound over an IPU, sum of overals over an IPU
        /// </summary>
        /// <returns></returns>
        public ValidationResult CheckRule()
        {
            ValidationResult vr;

            if (isEnergyEquation())
            {
                vr = NameValidation();
                if (!vr.IsEmpty)
                {
                    return vr;
                }

                vr = UnitsMatch();
                if (!vr.IsEmpty)
                {
                    return vr;
                }

                if (isSumOfTemp())
                {
                    vr = isValidSumOfTemp();
                    if (!vr.IsEmpty)
                    {
                        return vr;
                    }
                }

                //Then must be the equation Enthalpy(delta H) = Q
                else
                {
                    vr = isValidEnthalpyEqualsQ();
                }
                if (!vr.IsEmpty)
                {
                }

                HeatEquations.Add(new Tuple<object, ObservableCollection<IEquationToken>>(target, equation));
            }

                //This is for chemical Equations
            else
            {
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
                ChemicalEquations.Add(new Tuple<object, ObservableCollection<IEquationToken>>(target, equation));
            }
            return ValidationResult.Empty;
        }

        ValidationResult isValidEnthalpyEqualsQ()
        {
            ValidationResult vr;

            //7 for the sum of Enthropy 1 for = sign and 1 for Q
            if (equation.Count >= 9)
            {
                if (equation[1].Value == "=")
                {
                    vr = sumOfEnthropy(2);
                    if (!vr.IsEmpty)
                    {
                        return vr;
                    }
                    if (DictionaryOfTableData.ContainsKey(equation[0].Value))
                    {
                        if (DictionaryOfTableData[equation[0].Value].Tabletype != TableType.Heat)
                        {
                            //error
                            return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
                        }
                    }
                    else
                    {
                        //error
                        return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
                    }
                }
                else
                {
                    vr = sumOfEnthropy(0);
                    if (!vr.IsEmpty)
                    {
                        return vr;
                    }

                    if (DictionaryOfTableData.ContainsKey(equation[9].Value))
                    {
                        if (DictionaryOfTableData[equation[9].Value].Tabletype != TableType.Heat)
                        {
                            //error
                            return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
                        }
                    }
                    else
                    {
                        //error
                        return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
                    }
                }

            }
            else
            {
                return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
            }
            return ValidationResult.Empty;
        }

        private ValidationResult sumOfEnthropy(int index)
        {
            string currentAbbr;
            if (equation[index].Value.Length != 4)
            {
                //error
                return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
            }
            if (equation[index].Value[0] != 'H' && equation[index].Value[1] != 'f')
            {
                //error
                return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
            }
            currentAbbr = equation[index].Value[2].ToString();
            currentAbbr += equation[index].Value[3].ToString();

            if (double.IsNaN(CompoundFactory.GetElementsOfCompound(CompoundFactory.GetCompoundNameFromAbbr(currentAbbr)).HeatFormation))
            {
                return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Unkown_Constant));
            }
            index++;

            if (equation[index].Value != "+")
            {
                //error
                return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
            }

            index++;

            if (equation[index].Value.Length != 4)
            {
                //error
                return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
            }
            if (equation[index].Value[0] != 'C' && equation[index].Value[1] != 'p')
            {
                //error
                return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
            }
            if (currentAbbr != equation[index].Value[2].ToString() + equation[index].Value[3].ToString())
            {
                //error
                return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Incorrect_Abbrv));
            }

            if (double.IsNaN(CompoundFactory.GetElementsOfCompound(CompoundFactory.GetCompoundNameFromAbbr(currentAbbr)).HeatCapacity))
            {
                return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Unkown_Constant));
            }

            index++;

            if (equation[index].Value != "*")
            {
                //error
                return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
            }

            index++;

            if (equation[index].Value[0] != 'T')
            {
                try
                {
                    double.Parse(equation[index].Value);
                    //number so ok
                }
                catch
                {
                    //not number not temp label
                    //error
                    return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
                }
            }

            index++;

            if (equation[index].Value != "-")
            {
                //error
                return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
            }

            index++;

            int twoNintyEight;
            int.TryParse(equation[index].Value, out twoNintyEight);
            if (twoNintyEight != 298)
            {
                //error
                return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
            }

            index++;

            if (equation[index].Value != "*")
            {
                //error
                return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
            }

            index++;

            if (!DictionaryOfTableData.ContainsKey(equation[index].Value))
            {
                //error
                return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
            }
            if ((DictionaryOfTableData[equation[index].Value].Compound != CompoundFactory.GetCompoundNameFromAbbr(currentAbbr)))
            {
                //error
                return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Incorrect_Abbrv));
            }
            return ValidationResult.Empty;
        }

        ValidationResult isValidSumOfTemp()
        {
            int i = 0;
            bool equalsSignFound = false;
            ValidationResult vr;
            while (i + 8 < equation.Count)
            {
                //is it in this format Hf?? + Cp?? * temp - 298 * moles + *samething* + ... = Hf?? + Cp?? * temp - 298 * moles + *samething* + ...
                vr = sumOfEnthropy(i);
                i += 9;
                if (!vr.IsEmpty)
                {
                    return vr;
                }

                if (i == equation.Count)
                {
                    if (equalsSignFound)
                    {
                        return ValidationResult.Empty;
                    }
                    else
                    {
                        //error
                        return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
                    }
                }

                if (equation[i].Value == "=")
                {
                    equalsSignFound = true;
                }
                else if (equation[i].Value != "+")
                {
                    //error
                    return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
                }
                i++;
            }

            //error
            return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
        }

        bool isSumOfTemp()
        {
            foreach (IEquationToken token in equation)
            {
                if (token is VariableToken)
                {
                    if (DictionaryOfTableData.ContainsKey(token.Value))
                    {
                        if (DictionaryOfTableData[token.Value].Tabletype == TableType.Heat)
                        {
                            return false;
                        }
                    }
                }

            }
            return true;
        }

        ValidationResult UnitsMatch()
        {
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
                    GenericTableData data;
                    try
                    {
                        DictionaryOfTableData.TryGetValue(token.Value, out data);

                        if (data.Units == "?")
                        {
                            if (i + 4 < equation.Count)
                            {
                                double notUsed;
                                if (!(equation[i + 1].Value == "/" && equation[i + 2].Value == "100" && equation[i + 3].Value == "*" && (DictionaryOfTableData.ContainsKey(equation[i + 4].Value) || double.TryParse(equation[i + 4].Value, out notUsed))))
                                {
                                    return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Incorrect_Use_Of_Percent));
                                }
                            }
                            else
                            {
                                return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Incorrect_Use_Of_Percent));
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
                    return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.More_Than_One_Unit));
                }
            }
            else
            {
                //Everything should be the same compound need to check that
                if (!CheckSameCompound())
                {
                    if (!CheckSameElement())
                    {
                        return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.More_Than_One_Compound));
                    }
                }

                //need to check that all units are correct
                if (!CheckSameUnits())
                {
                    return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.More_Than_One_Unit));
                }
            }
            return ValidationResult.Empty;
        }

        private bool isValidEnergyEquation()
        {
            return true;
        }


        private bool isEnergyEquation()
        {
            foreach (IEquationToken token in equation)
            {
                if (token is VariableToken)
                {
                    try
                    {
                        double.Parse(token.Value);
                        //can parse it so number so doesn't tell us if this is heat or not
                    }
                    catch
                    {
                        if (token.Value[0] == 'T' || token.Value[0] == 'H' || (token.Value.Length > 1 && (token.Value[0].ToString() + token.Value[1].ToString() == "Cp")))
                        {
                            return true;
                        }
                        if (DictionaryOfTableData.ContainsKey(token.Value))
                        {
                            if (DictionaryOfTableData[token.Value].Tabletype == TableType.Heat)
                            {
                                return true;
                            }

                        }
                    }

                }
            }
            return false;
        }

        List<Element> findPossibleElements(IEquationToken tableData, IEquationToken scalar = null)
        {
            List<KeyValuePair<Element, int>> elements;
            List<Element> possibleElements = new List<Element>();
            int atoms = 1;
            elements = CompoundFactory.GetElementsOfCompound(DictionaryOfTableData[tableData.Value].Compound).elements;
            if (scalar != null)
            {
                try
                {
                    atoms = (int)double.Parse(scalar.Value);
                }
                catch
                {
                }
            }
            foreach (KeyValuePair<Element, int> element in elements)
            {
                if (element.Value == atoms)
                {
                    possibleElements.Add(element.Key);
                }
            }

            return possibleElements;
        }

        bool CheckSameElement()
        {
            List<Element> possibleElementsSoFar = new List<Element>();
            List<Element> currentpossibleElements = new List<Element>();
            int i = 0;

            while (i < equation.Count)
            {
                IEquationToken token = equation[i];
                if (token is VariableToken)
                {
                    try
                    {
                        double.Parse(token.Value);
                        //this is a number so we dont care about it
                    }
                    catch
                    {
                        //this is not a number so must be table entry.
                        if (i + 2 < equation.Count)
                        {
                            if (equation[i + 1].Value == "*")
                            {
                                currentpossibleElements = findPossibleElements(equation[i], equation[i + 2]);
                            }
                            else if (equation[i + 1].Value == "/")
                            {
                                if (i + 6 > equation.Count)
                                {
                                    if (i + 4 > equation.Count)
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        currentpossibleElements = findPossibleElements(equation[i], new VariableToken("1"));
                                        i += 4;
                                    }
                                }
                                else
                                {
                                    currentpossibleElements = findPossibleElements(equation[i], equation[i + 6]);
                                    i += 5;
                                }
                            }
                            else
                            {
                                currentpossibleElements = findPossibleElements(equation[i]);
                            }
                        }
                        else
                        {
                            currentpossibleElements = findPossibleElements(equation[i]);
                        }
                        int j = 0;

                        if (possibleElementsSoFar.Count == 0)
                        {
                            possibleElementsSoFar = currentpossibleElements;
                        }
                        else
                        {
                            //This preforms an intersection on the two lists
                            while (j < possibleElementsSoFar.Count)
                            {
                                if (!(currentpossibleElements.Contains(possibleElementsSoFar[j])))
                                {
                                    possibleElementsSoFar.RemoveAt(j);
                                }
                                else
                                {
                                    j++;
                                }
                            }
                            if (possibleElementsSoFar.Count == 0)
                            {
                                return false;
                            }
                        }
                    }
                }
                i++;
            }
            if (possibleElementsSoFar.Count > 1)
            {
                return false;
            }
            return true;
        }

        bool CheckSameUnits()
        {
            string units = null;
            foreach (string s in variableNames)
            {
                GenericTableData data;
                try
                {
                    DictionaryOfTableData.TryGetValue(s, out data);
                    string thisUnits = data.Units;

                    if (units == null)
                    {

                        if (thisUnits != "?")
                        {
                            units = thisUnits;
                        }
                    }
                    else if (units != thisUnits && thisUnits != "?")
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
                    GenericTableData data;
                    try
                    {
                        DictionaryOfTableData.TryGetValue(token.Value, out data);
                        if (compound == null)
                        {
                            compound = data.Compound;
                        }
                        else if (compound != data.Compound)
                        {
                            return false;
                        }
                        if (data.Units == "?")
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
                for (int j = equalSignLocation; j < lhs.Count; lhs.RemoveAt(j))
                {
                }
                //Reomve everything from 0 to equalSignLocation + 1 to get rid of the left hind side and the equalSign .
                for (int j = 0; j < equalSignLocation + 1; rhs.RemoveAt(0))
                {
                    j++;
                }

                if (lhs.Count == 1)
                {
                    GenericTableData data;
                    try
                    {
                        DictionaryOfTableData.TryGetValue(lhs[0].Value, out data);

                        if ("Overall" == data.Compound)
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
                    GenericTableData data;
                    try
                    {
                        DictionaryOfTableData.TryGetValue(rhs[0].Value, out data);

                        if ("Overall" == data.Compound)
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
                GenericTableData data;
                try
                {
                    DictionaryOfTableData.TryGetValue(s, out data);
                    if ("Overall" != data.Compound)
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
                bool valid = false;
                if (s.Length == 4)
                {
                    if (s[0] == 'H' && s[1] == 'f' || s[0] == 'C' && s[1] == 'p')
                    {
                        if ("" != CompoundFactory.GetCompoundNameFromAbbr(s[2].ToString() + s[3]))
                        {
                            valid = true;
                        }
                    }
                }
                if (valid == false)
                {
                    if (s[0] == 'T')
                    {
                        string temp = s.Remove(0, 1);
                        if (!DictionaryOfTableData.ContainsKey(temp))
                        {
                            invalidNames.Add(s);
                        }
                    }
                    else if (!DictionaryOfTableData.ContainsKey(s))
                    {
                        //so not in our dictionary and not a number so not valid
                        invalidNames.Add(s);
                    }
                }
            }

            if (invalidNames.Count > 0)
            {
                return new ValidationResult(target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Equation_Variable_Not_In_Tables, invalidNames.ToArray()));
            }
            else
            {
                return ValidationResult.Empty;
            }
        }


    }
}