/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ChemProV.PFD;
using ChemProV.PFD.EquationEditor;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.Validation.Rules.Adapters.Table;
using ChemProV.Validation.Rules.EquationRules;
using ChemProV.Validation.Rules.ProcessUnitRules;

namespace ChemProV.Validation.Rules
{
    /// <summary>
    /// This class is responsible for checking all rules it needs to when passed a list of iprocessUnits or tables or equations. And then reporting the errors in a list of strings
    /// </summary>
    public class RuleManager
    {
        public event EventHandler Solvable = delegate { };

        OptionDifficultySetting currentDifficultySetting = OptionDifficultySetting.MaterialBalance;

        public OptionDifficultySetting CurrentDifficultySetting
        {
            get { return currentDifficultySetting; }
            set { currentDifficultySetting = value; }
        }

        /// <summary>
        /// This class builds the ListOfFeedbackMessages so the MainPage has a list of the errors.
        /// </summary>
        public List<Tuple<object, string>> ListOfFeedbackMessages = new List<Tuple<object, string>>();

        private int ruleNumber = 1;

        /// <summary>
        /// This is built by buildFeedbackMessages this combines the equationDict, processUnit and TableDict.
        /// </summary>
        private Dictionary<object, List<string>> EveryoneDict = new Dictionary<object, List<string>>();

        private Dictionary<string, GenericTableData> tableDict = new Dictionary<string, GenericTableData>();

        private static RuleManager instance;

        /// <summary>
        /// This is called when we want to check the processUnits validity.  Then it calls buildFeedbackMessage so that,
        /// a a new EveryoneDict can be made with the new data.
        /// </summary>
        /// <param name="iProcessUnits">This is a list of all the iProcessUnits to be checked typicall all ProcessUnits</param>
        private void CheckProcessUnits(IEnumerable<IProcessUnit> iProcessUnits)
        {
            IRule rule;

            foreach (IProcessUnit ipu in iProcessUnits)
            {
                if (!(ipu is TemporaryProcessUnit))
                {
                    rule = ProcessUnitRuleFactory.GetProcessUnitRule(ipu);
                    rule.Target = ipu;
                    rule.CheckRule();
                    foreach (ValidationResult vr in rule.ValidationResults)
                    {
                        if (!EveryoneDict.ContainsKey(vr.Target))
                        {
                            EveryoneDict.Add(vr.Target, new List<string>());
                        }
                        if (!EveryoneDict[vr.Target].Contains(vr.Message))
                        {
                            EveryoneDict[vr.Target].Add("[" + ruleNumber + "]\n-" + vr.Message + "\n");
                            ruleNumber++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This is called when we want to check the tables validity.  Then it calls buildFeedbackMessage so that,
        /// a a new EveryoneDict can be made with the new data.
        /// </summary>
        /// <param name="tables">This is a list of PropertiesWindow to be checked typically all of them</param>
        private void CheckChemicalStreamPropertiesWindowFeedback(IEnumerable<IPfdElement> tables)
        {
            IRule rule = new TableRule();

            List<string> nonUniqueNames = new List<string>();
            List<IPropertiesWindow> listOfTables = new List<IPropertiesWindow>();

            foreach (IPropertiesWindow table in tables)
            {
                rule.Target = table;
                rule.CheckRule();

                ITableAdapter tableAdapter = TableAdapterFactory.CreateTableAdapter(table);
                int i = 0;
                int items = tableAdapter.GetRowCount();
                TableType tableType;
                string label, units, quantity, compound, temp;

                while (i < items)
                {
                    tableType = tableAdapter.GetTableType();
                    label = tableAdapter.GetLabelAtRow(i);
                    units = tableAdapter.GetUnitAtRow(i);
                    quantity = tableAdapter.GetQuantityAtRow(i);
                    compound = tableAdapter.GetCompoundAtRow(i);

                    if (currentDifficultySetting == OptionDifficultySetting.MaterialAndEnergyBalance)
                    {
                        temp = tableAdapter.GetTemperature();
                    }
                    else
                    {
                        //we dont need temp to just zero it out
                        temp = "0";
                    }

                    if (!tableDict.Keys.Contains(label))
                    {
                        tableDict.Add(label, new GenericTableData(table, tableType, label, units, quantity, compound, temp));
                    }
                    else
                    {
                        if (!nonUniqueNames.Contains(label))
                        {
                            nonUniqueNames.Add(label);
                        }
                        listOfTables.Add(table);
                    }
                    i++;
                }

                foreach (ValidationResult vr in rule.ValidationResults)
                {
                    if (!EveryoneDict.ContainsKey(vr.Target))
                    {
                        EveryoneDict.Add(vr.Target, new List<string>());
                    }
                    EveryoneDict[vr.Target].Add("[" + ruleNumber + "]\n-" + vr.Message + "\n");
                    ruleNumber++;
                }
            }
            if (nonUniqueNames.Count > 0)
            {
                ValidationResult vr = (new ValidationResult(listOfTables, ErrorMessageGenerator.GenerateMesssage(Validation.ErrorMessages.NonUniqueNames, nonUniqueNames.ToArray())));
                if (!EveryoneDict.ContainsKey(vr.Target))
                {
                    EveryoneDict.Add(vr.Target, new List<string>());
                }
                EveryoneDict[vr.Target].Add("[" + ruleNumber + "]\n-" + vr.Message + "\n");
                ruleNumber++;
            }
        }

        private ObservableCollection<EquationData> replaceUserDefinedVariables(ObservableCollection<EquationData> equations)
        {
            //This needs to be written to pull the userDefinedEquations out then to insert them into the other equations when used
            /*
            foreach (EquationData eqData in equations)
            {
                int i = 0;
                while (i < tuple.Item2.Count)
                {
                    IEquationToken token = tuple.Item2[i];
                    if (token is VariableToken)
                    {
                        foreach (Tuple<string, Equation> userDefinedVariable in userDefinedVariables)
                        {
                            if (token.Value == userDefinedVariable.Item1)
                            {
                                int index = tuple.Item2.IndexOf(token);

                                tuple.Item2.RemoveAt(index);

                                //decrement i because we just made the list smaller
                                i--;

                                foreach (IEquationToken item in userDefinedVariable.Item2.EquationTokens)
                                {
                                    //we need to do a deep copy because we will manipulate the contents of the tokens
                                    //and they shouldnt change what the equation has.  i.e. subsitution
                                    if (item is OperatorToken)
                                    {
                                        tuple.Item2.Insert(index, new OperatorToken(item.Value));
                                    }
                                    else if (item is VariableToken)
                                    {
                                        tuple.Item2.Insert(index, new VariableToken(item.Value));
                                    }
                                    else
                                    {
                                        throw new Exception("Unknown Token");
                                    }

                                    index++;

                                    //increment i because we just made the list bigger
                                    i++;
                                }
                            }
                        }
                    }
                    i++;
                }
            }

            return null;
            */

            //this is temp until func is redone
            return null;
        }

        /// <summary>
        /// This is called when we want to check the semantics of the equations.  The syntax is checked within the equation
        /// editor code already.  It keeps a list to all the equations so we do not have to pass it anything.
        /// </summary>
        private void CheckEquationSemantics(ObservableCollection<EquationData> equations, IList<Tuple<string, Equation>> userDefinedVariables, List<IProcessUnit> processUnits)
        {
            EquationRule rule = new EquationRule();

            replaceUserDefinedVariables(equations);

            rule.listOfEquations = equations;
            rule.Target = equations;
            rule.DictionaryOfTableData = tableDict;
            rule.ProcessUnits = processUnits;
            rule.CheckRule();

            foreach (ValidationResult vr in rule.ValidationResults)
            {
                if (!EveryoneDict.ContainsKey(vr.Target))
                {
                    EveryoneDict.Add(vr.Target, new List<string>());
                }
                EveryoneDict[vr.Target].Add("[" + ruleNumber + "]\n-" + vr.Message + "\n");
                ruleNumber++;
            }
        }

        private void Equations_Solvable(object sender, EventArgs e)
        {
            Solvable(this, e);
        }

        /// <summary>
        /// This checks all the rules for all IPfdElements in pfdElements all the equations in equations
        /// </summary>
        /// <param name="pfdElements">list of all pfdElements that need to be checked</param>
        /// <param name="equations">list of all equations that need to be checked</param>
        public void Validate(IEnumerable<IPfdElement> pfdElements, ObservableCollection<EquationData> equations, IList<Tuple<string, Equation>> userDefinedVariables)
        {
            //clear out the dictionary before we begin adding new stuff
            EveryoneDict.Clear();
            tableDict.Clear();

            if (pfdElements != null)
            {
                //pull out process units from the list of pfd elements
                var processUnits = from c in pfdElements
                                   where c is IProcessUnit
                                   select c as IProcessUnit;

                //and properties tables
                var tables = from c in pfdElements
                             where c is IPropertiesWindow
                             select c;

                //run the rule checker

                //start at 1
                ruleNumber = 1;

                CheckProcessUnits(processUnits);
                CheckChemicalStreamPropertiesWindowFeedback(tables);

                if (EveryoneDict.Count == 0)
                {
                    CheckEquationSemantics(equations, userDefinedVariables, processUnits.ToList());
                }
            }
        }

        /*
        private IList<Tuple<string, Equation>> deepCopy(IList<Tuple<string, Equation>> equations)
        {
            foreach (Tuple<string, Equation> tuple in equations)
            {
                Tuple<string, Equation> tupleDeepCopy;

                foreach (IEquationToken token in tuple.Item2)
                {
                    if (token is OperatorToken)
                    {
                        equationDeepCopy.Add(new OperatorToken(token.Value));
                    }
                    else if (token is VariableToken)
                    {
                        equationDeepCopy.Add(new VariableToken(token.Value));
                    }
                    else
                    {
                        throw new Exception("Unknown token");
                    }
                }
            }

            tupleDeepCopy = new Tuple<object, ObservableCollection<IEquationToken>>(tuple.Item1, equationDeepCopy);

            equationsDeepCopy.Add(tupleDeepCopy);
        }
        */

        /// <summary>
        /// This returns a Dictionary which as its key is an object that broke one or more rule.
        /// The data is a List of strings which are the messages associated with the rules it broke
        /// </summary>
        public Dictionary<object, List<string>> ErrorMessages
        {
            get
            {
                return EveryoneDict;
            }
        }

        private RuleManager()
        {
        }

        /// <summary>
        /// Used to get at the single instance of this object
        /// </summary>
        /// <returns></returns>
        public static RuleManager GetInstance()
        {
            if (instance == null)
            {
                instance = new RuleManager();
            }
            return instance;
        }
    }
}