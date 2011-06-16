/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using ChemProV.PFD.EquationEditor.Tokens;
using ChemProV.PFD.Streams.PropertiesWindow;

namespace ChemProV.PFD.EquationEditor
{
    public partial class EquationEditor : UserControl, IXmlSerializable
    {
        #region Delegates

        public event EventHandler EquationTokensChanged = delegate { };

        #endregion Delegates

        #region Fields

        private object selectedTool;

        private Equation selectedEquation;

        private IList<string> compounds;
        private List<string> elements = new List<string>();
        private List<string> equationTypes;

        private bool isReadOnly = false;

        private ObservableCollection<Tuple<object, ObservableCollection<IEquationToken>>> equationTokens = new ObservableCollection<Tuple<object, ObservableCollection<IEquationToken>>>();

        #endregion Fields

        #region Properties

        public List<string> EquationTypes
        {
            get { return equationTypes; }
            set { equationTypes = value; }
        }

        public object SelectedTool
        {
            get { return selectedTool; }
            set { selectedTool = value; }
        }

        public OptionDifficultySetting CurrentDifficultySetting
        {
            get;
            set;
        }

        /// <summary>
        /// A reference, to a list of references, which themselves are a list of references of strings representing the variable names used
        /// </summary>
        public ObservableCollection<Tuple<object, ObservableCollection<IEquationToken>>> EquationTokens
        {
            get { return equationTokens; }
            set { equationTokens = value; }
        }

        //private EquationValidationChecker variableNameExisitanceRule = EquationValidationChecker.GetInstance();

        public IList<string> Compounds
        {
            get { return compounds; }
            set
            {
                compounds = value;
                updateCompounds();
            }
        }

        public List<Equation> Equations
        {
            get
            {
                var eqs = from c in EquationStackPanel.Children where c is Equation select c as Equation;
                return new List<Equation>(eqs);
            }
        }

        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set
            {
                isReadOnly = value;
                foreach (Equation eq in EquationStackPanel.Children)
                {
                    eq.IsReadOnly = value;
                }
            }
        }

        #endregion Properties

        #region Constructor

        public EquationEditor()
        {
            InitializeComponent();
            Equation eq = new Equation(isReadOnly);
            //variableNameExisitanceRule.ListofEquationsFromEquations = EquationTokens;
            EquationTokens.CollectionChanged += new NotifyCollectionChangedEventHandler(EquationTokens_CollectionChanged);

            //this makes the first text box listen for when it has been changed and when it is it makes a new text box and sets that one to listen for the same thing
            RegisterEquationListeners(eq);
            selectedEquation = eq;
            eq.MyTextChanged += new EventHandler(eq_MyTextChanged);
            EquationTokens.Add(eq.EquationTokens);
            EquationStackPanel.Children.Add(eq);
            EquationScrollViewer.DataContext = this;
        }

        #endregion Constructor

        #region Methods

        public void InsertConstant(string constant)
        {
            selectedEquation.EquationText += " " + constant;
            EquationTextChanged(selectedEquation, EventArgs.Empty);

            //check to see if it is the last one if so need to call the eq_TextInputStart
            if (EquationStackPanel.Children[EquationStackPanel.Children.Count - 1] == selectedEquation)
            {
                eq_MyTextChanged(selectedEquation, EventArgs.Empty as TextCompositionEventArgs);
            }
        }

        public void ChangeSolveabiltyStatus(bool solvable)
        {
            if (solvable)
            {
                //white space to make it more centered.
                EquationTitle.Content = "Equations (Solvable)";
            }
            else
            {
                EquationTitle.Content = "Equations (Not Solvable)";
            }
        }

        /// <summary>
        /// Will remove all existing equations currently listed in the equation editor.
        /// </summary>
        public void ClearEquations()
        {
            //remove event handlers
            foreach (UIElement element in EquationStackPanel.Children)
            {
                if (element is Equation)
                {
                    Equation eq = element as Equation;
                    UnregisterEquationListneres(eq);
                }
            }

            //clear existing children
            EquationTokens.CollectionChanged -= new NotifyCollectionChangedEventHandler(EquationTokens_CollectionChanged);
            EquationTokens.Clear();
            EquationTokens.CollectionChanged += new NotifyCollectionChangedEventHandler(EquationTokens_CollectionChanged);
            this.EquationStackPanel.Children.Clear();

            //add new child
            Equation newEq = new Equation(isReadOnly);

            //attach event listeners
            RegisterEquationListeners(newEq);
            newEq.MyTextChanged += new EventHandler(eq_MyTextChanged);

            //add to the list of equations
            EquationStackPanel.Children.Add(newEq);
            EquationTokens.Add(newEq.EquationTokens);
        }

        public void LoadXmlElements(XElement doc)
        {
            //clear the equation stack
            EquationStackPanel.Children.Clear();

            //got to unregister the notify changed property or it will break
            EquationTokens.CollectionChanged -= new NotifyCollectionChangedEventHandler(EquationTokens_CollectionChanged);
            EquationTokens.Clear();
            EquationTokens.CollectionChanged += new NotifyCollectionChangedEventHandler(EquationTokens_CollectionChanged);

            //pull out the equations
            XElement equations = doc.Descendants("Equations").ElementAt(0);
            foreach (XElement xmlEquation in equations.Elements())
            {
                //create the equation
                Equation eq = new Equation(isReadOnly);

                //attach event listeners
                RegisterEquationListeners(eq);

                //add to the list of equations
                EquationStackPanel.Children.Add(eq);
                EquationTokens.Add(eq.EquationTokens);

                //set the equation's value
                eq.EquationText = xmlEquation.Value.ToString();

                eq.EquationTextChanged();

                //tell object that an equation's text has changed
                //EquationTextChanged(eq, EventArgs.Empty);
            }

            //the last equation added needs to have a special event listener attached
            (EquationStackPanel.Children.ElementAt(EquationStackPanel.Children.Count - 1) as Equation).MyTextChanged += new EventHandler(eq_MyTextChanged);
        }

        #endregion Methods

        #region Private Helpers

        private void eq_MyTextChanged(object sender, EventArgs e)
        {
            Equation eq = new Equation(isReadOnly);

            //this makes the text box that just fired stop listening since it is no longer the last one
            ((Equation)sender).MyTextChanged -= new EventHandler(eq_MyTextChanged);

            //this makes the new text box start listening whenever it has been fired
            eq.MyTextChanged += new EventHandler(eq_MyTextChanged);
            RegisterEquationListeners(eq);
            EquationStackPanel.Children.Add(eq);
            EquationTokens.Add(eq.EquationTokens);
        }

        private void eq_ReceivedFocus(object sender, EventArgs e)
        {
            selectedEquation = sender as Equation;
        }

        private void updateCompounds()
        {
            elements.Clear();
            foreach (string compoundstr in compounds)
            {
                Compound compound = CompoundFactory.GetElementsOfCompound((compoundstr).ToLower());

                foreach (KeyValuePair<Element, int> element in compound.elements)
                {
                    if (!elements.Contains(element.Key.Name))
                    {
                        elements.Add(element.Key.Name);
                    }
                }
            }
            equationTypes = new List<string>();

            equationTypes.Add("Not Selected");

            equationTypes.Add("Variable Definition");

            equationTypes.Add("Overall");

            equationTypes.AddRange(compounds);

            if (CurrentDifficultySetting != OptionDifficultySetting.MaterialBalance)
            {
                equationTypes.AddRange(elements);
            }

            if (CurrentDifficultySetting == OptionDifficultySetting.MaterialAndEnergyBalance)
            {
                equationTypes.Add("Energy");
            }

            foreach (Equation eq in Equations)
            {
                eq.UpdateEquationTypes(equationTypes);
            }
        }

        private void EquationTokens_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            EquationTokensChanged(sender, new EventArgs());
        }

        /// <summary>
        /// Called whenever a user navigates away from an equation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void eq_LostFocus(object sender, RoutedEventArgs e)
        {
            Equation senderEq = sender as Equation;

            //if the sender is not the last equation in the list  and it empty, remove it
            if (senderEq.CompareTo(EquationStackPanel.Children.ElementAt(EquationStackPanel.Children.Count - 1)) != 0)
            {
                if (senderEq.EquationText.Trim().Length == 0)
                {
                    //remove from the stack panel
                    EquationStackPanel.Children.Remove(senderEq);

                    //remove event handlers
                    UnregisterEquationListneres(senderEq);

                    //remove from set of equation tokens
                    EquationTokens.Remove(senderEq.EquationTokens);
                }
            }
        }

        private void EquationTextChanged(object sender, EventArgs e)
        {
            EquationTokensChanged(sender, e);
        }

        /// <summary>
        /// Created to consolidate all event listener attachments into a single location
        /// rather than having it spread all over the file.
        /// </summary>
        /// <param name="eq">The equation that we'd like to attach events to.</param>
        private void RegisterEquationListeners(Equation eq)
        {
            eq.EquationTokensChagned += new EventHandler(EquationTextChanged);
            eq.LostFocus += new RoutedEventHandler(eq_LostFocus);
            eq.ReceivedFocus += new EventHandler(eq_ReceivedFocus);
        }

        /// <summary>
        /// The inverse of RegisterEquationListeners: unregisters all event listeners from
        /// a given equation
        /// </summary>
        /// <param name="eq"></param>
        private void UnregisterEquationListneres(Equation eq)
        {
            eq.LostFocus -= new RoutedEventHandler(eq_LostFocus);
            eq.EquationTokensChagned -= new EventHandler(EquationTextChanged);
            eq.MyTextChanged -= new EventHandler(eq_MyTextChanged);
            eq.ReceivedFocus -= new EventHandler(eq_ReceivedFocus);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!(double.IsNaN(this.ActualWidth)))
            {
                foreach (Equation eq in EquationStackPanel.Children)
                {
                    eq.MaxWidth = this.ActualWidth;
                }
            }
        }

        #endregion Private Helpers

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            //not responsible for reading of XML data.  Handled in LoadXmlElements.
        }

        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Equation));

            writer.WriteStartElement("Equations");

            //loop through our list of equations except the last one
            foreach (Tuple<object, ObservableCollection<IEquationToken>> equationTuple in EquationTokens)
            {
                serializer.Serialize(writer, (equationTuple.Item1 as Equation));
            }
            writer.WriteEndElement();
        }

        #endregion IXmlSerializable Members
    }
}