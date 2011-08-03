/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ChemProV.PFD.EquationEditor;
using ChemProV.PFD.EquationEditor.Tokens;
using ChemProV.PFD.Streams.PropertiesWindow;

namespace ChemProV.Validation.Rules.EquationRules
{
    /// <summary>
    /// This checks the semantics of an equation, are the variable names valid, are percents used correctly, is the equation valid
    /// </summary>
    public class EquationSemanticsRule
    {
        private EquationData equationData;

        /// <summary>
        /// This is Dictionary that for the key uses a label name and for the data uses
        /// GenericTableData which is the data associated with that label
        /// </summary>
        public Dictionary<string, GenericTableData> DictionaryOfTableData
        {
            get;
            set;
        }

        /// <summary>
        /// This is parsed text for the equation
        /// </summary>
        public EquationData EquationData
        {
            get { return equationData; }
        }

        /// <summary>
        /// This is the Equation being checked.
        /// </summary>
        public object Target
        {
            get { return equationData.EquationReference; }
        }

        private List<string> variableNames;
        private ObservableCollection<IEquationToken> equationTokens;

        /// <summary>
        /// This is the constructor for the EquationSemanticRule
        /// </summary>
        /// <param name="equation">the parsed text for the equation to be checked</param>
        /// <param name="variableNames">the variableNames used in equation</param>
        /// <param name="target">a reference to the Equation itself</param>
        public EquationSemanticsRule(EquationData equationData, List<string> variableNames)
        {
            this.equationData = equationData;
            this.variableNames = variableNames;
            this.equationTokens = equationData.EquationTokens;
        }

        /// <summary>
        /// This checks all the rules for the semantics of an equation,
        /// valid variable names valid
        /// correct use of percents
        /// valid equation, must be sum of one table equals the overall, the sum of a compound over an IPU, sum of overalls over an IPU
        /// </summary>
        /// <returns></returns>
        public ValidationResult CheckRule()
        {
            ValidationResult vr;

            //aka is 'supposed' to be a heat equation?
            if (EquationData.Type.Classification == EquationClassification.Energy)
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
                    return vr;
                }
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

                vr = ChemicalEquationValidation();

                if (!vr.IsEmpty)
                {
                    return vr;
                }
            }
            return ValidationResult.Empty;
        }

        private ValidationResult isValidEnthalpyEqualsQ()
        {
            ValidationResult vr;
            int index = 0;
            int endOfEntropyCalc = 0;
            int locationOfQ = 0;
            //7 for the sum of Entropy 1 for = sign and 1 for Q
            if (equationTokens.Count > 2)
            {
                if (equationTokens[1].Value == "=")
                {
                    //In this form "Q=Entropy"
                    locationOfQ = 0;
                    index = 2;
                    endOfEntropyCalc = equationTokens.Count;
                }
                else
                {
                    //In this form "Entropy=Q"
                    index = 0;
                    locationOfQ = equationTokens.Count - 1;
                    endOfEntropyCalc = equationTokens.Count - 3;

                    //check to make sure the token before Q is equal sign
                    if (equationTokens[equationTokens.Count - 2].Value != "=")
                    {
                        //error
                        return new ValidationResult(equationData.EquationReference, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
                    }
                }

                //check to make sure Q is right
                if (DictionaryOfTableData.ContainsKey(equationTokens[locationOfQ].Value))
                {
                    if (DictionaryOfTableData[equationTokens[locationOfQ].Value].Tabletype != TableType.Heat)
                    {
                        //error because what we think to be Q is not from a heat table
                        return new ValidationResult(equationData.EquationReference, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
                    }
                }
                else
                {
                    try
                    {
                        double.Parse(equationTokens[locationOfQ].Value);
                    }
                    catch
                    {
                        //error because Q is neither a valid label or a number
                        return new ValidationResult(equationData.EquationReference, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
                    }
                }

                while (index < endOfEntropyCalc)
                {
                    try
                    {
                        index = sumOfEntropy(index);
                        index++;
                    }
                    catch (ValidationResultException validationException)
                    {
                        return validationException.ValidationResult;
                    }

                    if (index + 1 < endOfEntropyCalc)
                    {
                        if (equationTokens[index].Value == "+" || equationTokens[index].Value == "-")
                        {
                            //after this it will start to what should be the start of the next Entropy calc.
                            index++;
                        }
                        else
                        {
                            return new ValidationResult(Target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
                        }
                    }
                }
            }
            return ValidationResult.Empty;
        }

        private void ThrowInvalidHeatEquationException()
        {
            throw new ValidationResultException(new ValidationResult(Target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation)));
        }

        private void ThrowUnknownConstantException()
        {
            throw new ValidationResultException(new ValidationResult(Target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Unknown_Constant)));
        }

        private int sumOfEntropy(int index)
        {
            string currentAbbr;

            //rather than making sure we don't go out side of the index if we do we catch it and then return the Invalid_Heat_Equation error
            try
            {
                if (equationTokens[index].Value.Length != 4)
                {
                    //error
                    ThrowInvalidHeatEquationException();
                }
                if (equationTokens[index].Value[0] != 'H' && equationTokens[index].Value[1] != 'f')
                {
                    //error
                    ThrowInvalidHeatEquationException();
                }
                currentAbbr = equationTokens[index].Value[2].ToString();
                currentAbbr += equationTokens[index].Value[3].ToString();

                if (double.IsNaN(CompoundFactory.GetElementsOfCompound(CompoundFactory.GetCompoundNameFromAbbr(currentAbbr)).HeatFormation))
                {
                    ThrowUnknownConstantException();
                }
                index++;

                if (equationTokens[index].Value != "+")
                {
                    //error
                    ThrowInvalidHeatEquationException();
                }

                index++;

                if (equationTokens[index].Value.Length != 4)
                {
                    //error
                    ThrowInvalidHeatEquationException();
                }
                if (equationTokens[index].Value[0] != 'C' && equationTokens[index].Value[1] != 'p')
                {
                    //error
                    ThrowInvalidHeatEquationException();
                }
                if (currentAbbr != equationTokens[index].Value[2].ToString() + equationTokens[index].Value[3].ToString())
                {
                    //error
                    throw new ValidationResultException(new ValidationResult(Target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Incorrect_Abbrv)));
                }

                if (double.IsNaN(CompoundFactory.GetElementsOfCompound(CompoundFactory.GetCompoundNameFromAbbr(currentAbbr)).HeatCapacity))
                {
                    ThrowUnknownConstantException();
                }

                index++;

                if (equationTokens[index].Value != "*")
                {
                    //error
                    ThrowInvalidHeatEquationException();
                }

                index++;

                if (equationTokens[index].Value[0] != 'T')
                {
                    try
                    {
                        double.Parse(equationTokens[index].Value);
                        //number so ok
                    }
                    catch
                    {
                        //not number not temp label
                        //error
                        ThrowInvalidHeatEquationException();
                    }
                }

                index++;

                if (equationTokens[index].Value != "-")
                {
                    //error
                    ThrowInvalidHeatEquationException();
                }

                index++;

                int twentyFive;
                int.TryParse(equationTokens[index].Value, out twentyFive);
                if (twentyFive != 25)
                {
                    //error
                    ThrowInvalidHeatEquationException();
                }

                index++;

                if (equationTokens[index].Value != "*")
                {
                    //error
                    ThrowInvalidHeatEquationException();
                }

                index++;

                if (!DictionaryOfTableData.ContainsKey(equationTokens[index].Value))
                {
                    //error
                    ThrowInvalidHeatEquationException();
                }
                if ((DictionaryOfTableData[equationTokens[index].Value].Compound != CompoundFactory.GetCompoundNameFromAbbr(currentAbbr)))
                {
                    //error
                    throw new ValidationResultException(new ValidationResult(Target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Incorrect_Abbrv)));
                }
            }
            catch
            {
                ThrowInvalidHeatEquationException();
            }
            return index;
        }

        private ValidationResult isValidSumOfTemp()
        {
            int i = 0;
            bool equalsSignFound = false;
            ValidationResult vr;
            while (i + 8 < equationTokens.Count)
            {
                //is it in this format Hf?? + Cp?? * temp - 298 * moles + *something* + ... = Hf?? + Cp?? * temp - 298 * moles + *same thing* + ...
                try
                {
                    i += sumOfEntropy(i);
                }
                catch (ValidationResultException validationException)
                {
                    return validationException.ValidationResult;
                }

                if (i == equationTokens.Count)
                {
                    if (equalsSignFound)
                    {
                        return ValidationResult.Empty;
                    }
                    else
                    {
                        //error
                        return new ValidationResult(Target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
                    }
                }

                if (equationTokens[i].Value == "=")
                {
                    equalsSignFound = true;
                }
                else if (equationTokens[i].Value != "+" && equationTokens[i].Value != "-")
                {
                    //error
                    return new ValidationResult(Target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
                }
                i++;
            }

            //error
            return new ValidationResult(Target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InValid_Heat_Equation));
        }

        private bool isSumOfTemp()
        {
            //we know for a sumOFQ that it will be Q=Entropy or Entropy=Q and for SumOfTemp Entropy=Entropy
            //Since Q is only one token but Entropy is 8 we can do this check to see if it is Q=Entropy or Entropy=Q
            if (equationTokens[1].Value == "=" || equationTokens[equationTokens.Count - 2].Value == "=")
            {
                return false;
            }
            return true;
        }

        private ValidationResult UnitsMatch()
        {
            return ValidationResult.Empty;
        }

        private ValidationResult percentUsage()
        {
            int i = 0;
            while (i < equationTokens.Count)
            {
                IEquationToken token = equationTokens[i];
                if (token is VariableToken)
                {
                    GenericTableData data;
                    try
                    {
                        DictionaryOfTableData.TryGetValue(token.Value, out data);

                        if (data.Units == "?")
                        {
                            if (i + 4 < equationTokens.Count)
                            {
                                double notUsed;
                                if (!(equationTokens[i + 1].Value == "/" && equationTokens[i + 2].Value == "100" && equationTokens[i + 3].Value == "*" && (DictionaryOfTableData.ContainsKey(equationTokens[i + 4].Value) || double.TryParse(equationTokens[i + 4].Value, out notUsed))))
                                {
                                    return new ValidationResult(Target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Incorrect_Use_Of_Percent));
                                }
                            }
                            else
                            {
                                return new ValidationResult(Target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Incorrect_Use_Of_Percent));
                            }
                        }
                    }
                    catch
                    {
                        //it is a number so don't do anything
                    }
                }
                i++;
            }

            return ValidationResult.Empty;
        }

        private ValidationResult ChemicalEquationValidation()
        {
            ValidationResult vr;
            vr = CheckSameUnits();
            if (vr != ValidationResult.Empty)
            {
                return vr;
            }
            if (equationData.Type.Classification == EquationClassification.Overall)
            {
                if (OverallSum() != true)
                {
                    return new ValidationResult(equationData.EquationReference, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Not_Overall));
                }
            }
            else if (equationData.Type.Classification == EquationClassification.Compound)
            {
                vr = CheckSameCompound(equationData.Type.Content as string);
                if (vr != ValidationResult.Empty)
                {
                    return vr;
                }
            }
            else if (equationData.Type.Classification == EquationClassification.Element)
            {
                string elementName = (equationData.Type.Content as string);

                elementName = elementName.Remove(elementName.Length - 3);

                vr = CheckSameElement(elementName);
                if (vr != ValidationResult.Empty)
                {
                    return vr;
                }
            }
            else
            {
                throw new Exception("Unknown Chemical EquationClassification");
            }

            return ValidationResult.Empty;
        }

        private List<Element> findPossibleElements(IEquationToken tableData, IEquationToken scalar = null)
        {
            Dictionary<Element, int> elements;
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

        private ValidationResult CheckSameElement(string element)
        {
            int i = 0;

            while (i < variableNames.Count)
            {
                string token = variableNames[i];

                if (DictionaryOfTableData[token].Compound != "Overall")
                {
                    var elements = CompoundFactory.GetElementsOfCompound(DictionaryOfTableData[token].Compound).elements;
                    var d = (from c in elements where c.Key.Name == element select c);

                    if (d == null || d.Count() == 0)
                    {
                        return new ValidationResult(equationData.EquationReference, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.More_Than_One_Element, element, token));
                    }
                }
                i++;
            }
            return ValidationResult.Empty;
        }

        private ValidationResult CheckSameUnits()
        {
            string units = null;
            foreach (string s in variableNames)
            {
                GenericTableData data = DictionaryOfTableData[s];
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
                    return new ValidationResult(Target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Inconsistant_Units, units, thisUnits));
                }
            }

            return ValidationResult.Empty;
        }

        private ValidationResult CheckSameCompound(string compound)
        {
            int i = 0;
            bool compoundUsed = false;
            while (i < variableNames.Count)
            {
                string varName = variableNames[i];

                GenericTableData data = DictionaryOfTableData[varName];

                if (compound != data.Compound && data.Compound != "Overall")
                {
                    return new ValidationResult(equationData.EquationReference, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.More_Than_One_Compound, compound, data.Compound));
                }
                else if (compound == data.Compound)
                {
                    compoundUsed = true;
                }

                i++;
            }

            if (compoundUsed == false)
            {
                return new ValidationResult(equationData.EquationReference, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Compound_Not_Used));
            }

            return ValidationResult.Empty;
        }

        private bool isSumOfOneTable()
        {
            int equalSignLocation = findEqualsSign(equationTokens);
            ObservableCollection<IEquationToken> lhs = new ObservableCollection<IEquationToken>(equationTokens);
            ObservableCollection<IEquationToken> rhs = new ObservableCollection<IEquationToken>(equationTokens);

            if (equalSignLocation != 0)
            {
                //Now for lhs remove everything at and past the location of the equalSign
                for (int j = equalSignLocation; j < lhs.Count; lhs.RemoveAt(j))
                {
                }
                //Remove everything from 0 to equalSignLocation + 1 to get rid of the left hind side and the equalSign .
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
                            //ok so definitely adding the parts of a table to get the whole
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
                            //ok so definitely adding the parts of a table to get the whole
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
            foreach (string varName in variableNames)
            {
                GenericTableData data = DictionaryOfTableData[varName];
                if ("Overall" != data.Compound)
                {
                    return false;
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

        private ValidationResult NameValidation()
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
                return new ValidationResult(Target, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Equation_Variable_Not_In_Tables, invalidNames.ToArray()));
            }

            else
            {
                return ValidationResult.Empty;
            }
        }
    }
}