using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

using ChemProV.PFD;
using ChemProV.PFD.EquationEditor;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Streams.PropertiesTable;
using ChemProV.PFD.EquationEditor.Tokens;

using ChemProV.PFD.Streams.PropertiesTable.Chemical;

namespace ChemProV.Validation.Rules
{
    /// <summary>
    /// This class is responsible for checking all rules it needs to when passed a list of iprocessUnits or tables or equations. And then reporting the errors in a list of strings
    /// </summary>
    public class RuleManager
    {

        /// <summary>
        /// This class builds the ListOfFeedbackMessages so the MainPage has a list of the errors.
        /// </summary>
        public List<Tuple<object, string>> ListOfFeedbackMessages = new List<Tuple<object, string>>();

        private int ruleNumber = 1;

        /// <summary>
        /// This is built by buildFeedbackMessages this combines the equationDict, processUnit and TableDict.
        /// </summary>
        private Dictionary<object, List<string>> EveryoneDict = new Dictionary<object, List<string>>();

        private Dictionary<string, ChemicalStreamData> tableDict = new Dictionary<string, ChemicalStreamData>();

        private static RuleManager instance;

        /// <summary>
        /// This is called when we want to check the processUnits validity.  Then it calls buildFeedbackMessage so that,
        /// a a new EveryoneDict can be made with the new data.
        /// </summary>
        /// <param name="iProcessUnits">This is a list of all the iProcessUnits to be checked typicall all ProcessUnits</param>
        private void CheckProcessUnits(IEnumerable<IPfdElement> iProcessUnits)
        {
            IRule rule = new ProcessUnitRule();

            foreach (IProcessUnit ipu in iProcessUnits)
            {
                if (!(ipu is TemporaryProcessUnit))
                {
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
                            EveryoneDict[vr.Target].Add("[" + ruleNumber + "]\n\r" + vr.Message);
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
        /// <param name="tables">This is a list of propertiesTable to be checked typicall all of them</param>
        private void CheckChemicalStreamPropertiesTableFeedback(IEnumerable<IPfdElement> tables)
        {
            IRule rule = new TableRule();

            foreach (IPropertiesTable table in tables)
            {
                rule.Target = table;
                rule.CheckRule();

                //THIS NEEDS FIXED WHEN EQUATIONS GET FIXED TO USE ADAPTER
                int i = 0;
                int items = (table.PropertiesTable.ItemsSource as ObservableCollection<ChemicalStreamData>).Count;
                foreach (ChemicalStreamData data in table.PropertiesTable.ItemsSource as ObservableCollection<ChemicalStreamData>)
                {
                    if (i < items - 1)
                    {
                        if (!tableDict.ContainsKey(data.Label))
                        {
                            tableDict.Add(data.Label, data);
                        }
                    }
                    i++;
                }

                foreach (ValidationResult vr in rule.ValidationResults)
                {
                    if (!EveryoneDict.ContainsKey(vr.Target))
                    {
                        EveryoneDict.Add(vr.Target, new List<string>());
                    }
                    EveryoneDict[vr.Target].Add("[" + ruleNumber + "]\n\r" + vr.Message);
                    ruleNumber++;
                }
            }
        }

        /// <summary>
        /// This is called when we want to check the semantics of the equations.  The syntax is checked within the equation
        /// editor code already.  It keeps a list to all the equations so we do not have to pass it anything.
        /// </summary>
        private void CheckEquationSemantics(ObservableCollection<Tuple<object, ObservableCollection<IEquationToken>>> equations)
        {
            EquationRule rule = new EquationRule();
            rule.listOfEquations = equations;
            rule.Target = equations;
            rule.DictionaryOfTableData = tableDict;
            rule.CheckRule();

            foreach (ValidationResult vr in rule.ValidationResults)
            {
                if (!EveryoneDict.ContainsKey(vr.Target))
                {
                    EveryoneDict.Add(vr.Target, new List<string>());
                }
                EveryoneDict[vr.Target].Add("[" + ruleNumber + "]\n\r" + vr.Message);
                ruleNumber++;
            }
        }

        public void Validate(IEnumerable<IPfdElement> pfdElements, ObservableCollection<Tuple<object, ObservableCollection<IEquationToken>>> equations)
        {
            //clear out the dictionary before we begin adding new stuff
            EveryoneDict.Clear();
            tableDict.Clear();
            if (pfdElements != null)
            {
                //pull out process units from the list of pfd elements
                var processUnits = from c in pfdElements
                                   where c is IProcessUnit
                                   select c;


                //and properties tables
                var tables = from c in pfdElements
                             where c is IPropertiesTable
                             select c;

                //run the rule checker

                //start at 1
                ruleNumber = 1;

                CheckProcessUnits(processUnits);
                CheckChemicalStreamPropertiesTableFeedback(tables);
                CheckEquationSemantics(equations);
            }
        }

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
