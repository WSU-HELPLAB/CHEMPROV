/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using ChemProV.PFD.EquationEditor.Tokens;
using ChemProV.Validation;
using Antlr.Runtime;
using System.IO;
using ChemProV.Grammars;
using Antlr.Runtime.Tree;

namespace ChemProV.PFD.EquationEditor
{
    public partial class Equation : UserControl, IXmlSerializable, IComparable, INotifyPropertyChanged
    {
        #region Delegates

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public event EventHandler MyTextChanged = delegate { };
        public event EventHandler EquationTokensChagned = delegate { };
        public event EventHandler ReceivedFocus = delegate { };

        #endregion Delegates

        #region Fields

        /// <summary>
        /// Keeps track of the process unit's unique ID number.  Needed when parsing
        /// to/from XML for saving and loading
        /// </summary>
        private static int equationIdCounter = 0;
        private string equationId;

        private bool isReadOnly = false;

        private ObservableCollection<ComboBoxEquationTypeItem> equationTypes = new ObservableCollection<ComboBoxEquationTypeItem>();

        public ObservableCollection<ComboBoxEquationTypeItem> EquationTypes
        {
            get { return equationTypes; }
        }

        #endregion Fields

        #region Properties

        public ComboBoxEquationTypeItem SelectedItem
        {
            get { return EquationType.SelectedItem as ComboBoxEquationTypeItem; }
        }

        public string EquationText
        {
            get
            {
                return EquationTextBox.Text;
            }
            set
            {
                EquationTextBox.Text = value;
            }
        }

        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set
            {
                isReadOnly = value;
                if (IsReadOnly)
                {
                    EquationTextBox.Visibility = System.Windows.Visibility.Collapsed;
                    TokenListControl.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    EquationTextBox.Visibility = System.Windows.Visibility.Visible;
                    TokenListControl.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Gets or sets the equation's unique ID number
        /// </summary>
        public String Id
        {
            get
            {
                return equationId;
            }
            set
            {
                equationId = value;
            }
        }

        public ObservableCollection<string> VariableNames
        {
            get;
            set;
        }

        public ObservableCollection<IEquationToken> EquationTokens
        {
            get;
            set;
        }

        #endregion Properties

        #region Constructor

        public Equation()
        {
            InitializeComponent();

            LocalInit(false);
        }

        public Equation(bool isReadOnly)
        {
            InitializeComponent();
            LocalInit(isReadOnly);
        }

        #endregion Constructor

        #region Initializes

        private void LocalInit(bool isReadOnly)
        {
            this.isReadOnly = isReadOnly;
            this.DataContext = this;
            equationIdCounter++;
            equationId = "Eq_" + equationIdCounter;
            EquationTokens = new ObservableCollection<IEquationToken>();
            TokenListControl.ItemsSource = EquationTokens;
            VariableNames = new ObservableCollection<string>();
            EquationType.SelectionChanged += new SelectionChangedEventHandler(EquationType_SelectionChanged);
            EquationTextBox.TextChanged += new TextChangedEventHandler(EquationTextBox_TextChanged);
            EquationTextBox.KeyDown += new KeyEventHandler(EquationTextBox_KeyDown);
            EquationTextBox.GotFocus += new RoutedEventHandler(EquationTextBox_GotFocus);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(EquationClick);
        }

        void EquationClick(object sender, MouseButtonEventArgs e)
        {
            ReceivedFocus(this, EventArgs.Empty);
        }

        #endregion Initializes

        #region Public Methods

        public void UpdateEquationTypeComboBox(ObservableCollection<ComboBoxEquationTypeItem> newItems)
        {
            ComboBoxEquationTypeItem currentSelected = EquationType.SelectedItem as ComboBoxEquationTypeItem;

            equationTypes.Clear();

            ComboBoxEquationTypeItem newCurrentSelected = null;
            foreach (var item in newItems)
            {
                ComboBoxEquationTypeItem temp = new ComboBoxEquationTypeItem(item.Classification, item.Content as string);
                equationTypes.Add(temp);
                if (currentSelected != null && currentSelected.Classification == item.Classification && (currentSelected.Content as string) == (item.Content as string))
                {
                    newCurrentSelected = temp;
                }
            }

            PropertyChanged(this, new PropertyChangedEventArgs("EquationTypes"));

            if (newCurrentSelected != null)
            {
                EquationType.SelectedIndex = equationTypes.IndexOf(newCurrentSelected);
            }
            else
            {
                if (equationTypes.Count > 0)
                {
                    EquationType.SelectedIndex = 0;
                }
                else
                {
                    EquationType.SelectedIndex = -1;
                }
            }
        }

        /// <summary>
        /// highlight or unhighlights the feedback area
        /// </summary>
        /// <param name="highlight">true if you want highlight, false if u want to unhighlight</param>
        public void HighlightFeedback(bool highlight)
        {
            if (highlight)
            {
                this.EquationFeedback.Background = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                this.EquationFeedback.Background = new SolidColorBrush(Colors.White);
            }
        }

        public void SetFeedback(string message, int errorNumber)
        {
            //make and set tooltip
            DependencyObject parent = this.Parent;
            string checkMessage = message;

            while (!(parent is EquationEditor))
            {
                parent = (parent as FrameworkElement).Parent;
            }

            while (checkMessage[0] != '-')
            {
                checkMessage = checkMessage.Remove(0, 1);
            }
            checkMessage = checkMessage.Remove(0, 1);

            if (checkMessage.Trim() == ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Solvable).Trim())
            {
                (parent as EquationEditor).ChangeSolveabiltyStatus(true);
            }
            else
            {
                (parent as EquationEditor).ChangeSolveabiltyStatus(false);
            }
            Silverlight.Controls.ToolTip tooltip = new Silverlight.Controls.ToolTip();

            if (EquationFeedback.Text == null || EquationFeedback.Text == "")
            {
                EquationFeedback.Text = "[" + errorNumber + "]";
            }
            else
            {
                EquationFeedback.Text = EquationFeedback.Text.Remove(EquationFeedback.Text.Length - 1) + "," + errorNumber + "]";
            }
            EquationFeedback.Visibility = Visibility.Visible;
            tooltip.InitialDelay = new Duration(new TimeSpan(0, 0, 1));
            tooltip.DisplayTime = new Duration(new TimeSpan(1, 0, 0));
            tooltip.Content = message;
            Silverlight.Controls.ToolTipService.SetToolTip(EquationFeedback, tooltip);
        }

        public void RemoveFeedback()
        {
            EquationFeedback.Text = "";
            EquationFeedback.Visibility = Visibility.Collapsed;
            Silverlight.Controls.ToolTipService.SetToolTip(EquationFeedback, null);
        }

        public void EquationTextChanged()
        {
            EquationTextBox_TextChanged(this.EquationTextBox, EventArgs.Empty as TextChangedEventArgs);
        }

        #endregion Public Methods

        #region Private Functions

        private void EquationType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EquationTokensChagned(this, EventArgs.Empty);
        }

        /// <summary>
        /// We use this as a hack because TextChanged does not fire if Cntrl V is used so we call it ourselves
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EquationTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            //if a key combo was pressed that could change the tokens then assume it changed
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V ||
                Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z ||
                Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Y ||
                Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.X)
            {
                EquationTextBox_TextChanged(sender, EventArgs.Empty as TextChangedEventArgs);
            }
        }

        private void EquationTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ReceivedFocus(this, e);
        }

        private List<string> parseText(string text)
        {
            string unit = "";
            List<string> parsedText = new List<string>();
            Stack<char> parenStack = new Stack<char>();
            Regex operationTerminal = new Regex("[+|//|-|*]");
            bool foundEqualSign = false;
            int i = 0;
            while (i < text.Length)
            {
                if ((operationTerminal.IsMatch(text[i] + "")) || text[i] == ' ' || text[i] == '-')
                {
                    parsedText.Add(unit);
                    parsedText.Add(text[i] + "");
                    unit = "";
                }
                else if (text[i] == '=')
                {
                    if (foundEqualSign == false)
                    {
                        parsedText.Add(unit);
                        parsedText.Add(text[i] + "");
                        unit = "";
                        foundEqualSign = true;
                    }
                    else
                    {
                        highlightText(false);
                    }
                }
                else if (text[i] == '(')
                {
                    parenStack.Push(text[i]);
                    i++;
                    parsedText.Add(unit);
                    unit = "(";
                    while (i < text.Length && parenStack.Count != 0)
                    {
                        if (text[i] == '(')
                        {
                            parenStack.Push(text[i]);
                        }
                        else if (text[i] == ')')
                        {
                            parenStack.Pop();
                        }
                        unit += text[i];
                        if (parenStack.Count != 0)
                        {
                            i++;
                        }
                    }
                    if (parenStack.Count != 0)
                    {
                        //PARENS NOT RIGHT
                        parsedText.Clear();
                        highlightText(false);
                        return parsedText;
                    }
                    else
                    {
                        parsedText.Add(unit);
                        unit = "";
                    }
                }
                else
                {
                    unit += text[i];
                }
                i++;
            }
            parsedText.Add(unit);
            return trim(parsedText);
        }

        private List<string> trim(List<string> parsedText)
        {
            int i = 0;
            while (i < parsedText.Count)
            {
                if (parsedText[i] == "" || parsedText[i] == " ")
                {
                    parsedText.RemoveAt(i);
                }

                //else because if we remove index it will automatically set index to the next 1 don't want to increment twice
                else
                {
                    i++;
                }
            }
            return parsedText;
        }

        private bool isOperation(string text)
        {
            if (text[0] == '-' || text[0] == '*' || text[0] == '+' || text[0] == '/' || text[0] == '=')
            {
                EquationTokens.Add(new OperatorToken(text));
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool recersiveVar(List<string> parsedText)
        {
            bool isValid = true;
            int i = 0;

            if (parsedText.Count > 0)
            {
                isValid = isVar(parsedText[i]);
                if (isValid == false)
                {
                    return false;
                }
            }
            i++;
            while (i < parsedText.Count)
            {
                if (isOperation(parsedText[i]) == false)
                {
                    return false;
                }
                i++;
                if (i >= parsedText.Count)
                {
                    return false;
                }
                if (isVar(parsedText[i]) == false)
                {
                    return false;
                }
                i++;
            }
            return isValid;
        }

        private void highlightText(bool isValid)
        {
            //set the color of the background and foreground based on if it is valid or not.
            if (isValid)
            {
                if (isReadOnly)
                {
                    //(EquationTextBox as Label).Background = new SolidColorBrush(Colors.White);
                    //(EquationTextBox as Label).Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    (EquationTextBox as TextBox).Background = new SolidColorBrush(Colors.White);
                    (EquationTextBox as TextBox).Foreground = new SolidColorBrush(Colors.Black);
                }
            }
            else
            {
                if (isReadOnly)
                {
                    //(EquationTextBox as Label).Background = new SolidColorBrush(Colors.Red);
                    //(EquationTextBox as Label).Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    (EquationTextBox as TextBox).Background = new SolidColorBrush(Colors.Red);
                    (EquationTextBox as TextBox).Foreground = new SolidColorBrush(Colors.White);
                }
            }
        }

        private bool isVar(string text)
        {
            //if parentese this means it is recursive so we got to deal with it.
            if (text[0] == '(')
            {
                //set it to 1 past the first element to cut of the (
                text = text.Remove(0, 1);
                text = text.Remove(text.Length - 1, 1);
                //NEED TO TRIM OF '(' before giving to parseText
                return recersiveVar(parseText(text));
            }
            else
            {
                //if no parentese then we just assume it is a variable if it isn't then we will flag it later when we check again the table names.
                VariableNames.Add(text);
                EquationTokens.Add(new VariableToken(text));
                return true;
            }
        }

        private void EquationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //we fire MyTextChanged because SL's textChanged may not have fired if cntrl-V was used
            MyTextChanged(this, EventArgs.Empty);

            string text = (sender as TextBox).Text;

            //assume validity / set the color back to normal so we only need to change it if it is wrong
            highlightText(true);

            //we got to clear EquationTokens because we are about to go find them again.
            EquationTokens.Clear();

            List<string> parsedText = parseText(text);
            int i = 0;
            if (parsedText.Count > 0)
            {
                if (isVar(parsedText[i]) == false)
                {
                    //raise flag not valid
                    highlightText(false);
                    return;
                }
                i++;
                while (i < parsedText.Count)
                {
                    if (isOperation(parsedText[i]) == false)
                    {
                        //raise flag not valid
                        highlightText(false);
                        return;
                    }
                    i++;
                    if (i >= parsedText.Count)
                    {
                        return;
                    }
                    if (isVar(parsedText[i]) == false)
                    {
                        //raise flag not valid
                        highlightText(false);
                        return;
                    }
                    i++;
                }
            }

            EquationTokensChagned(this, new EventArgs());
        }

        #endregion Private Functions

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            //parsing of XML data is handled in the main page
        }

        /// <summary>
        /// Called when we try to serialize this object
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlWriter writer)
        {
            //simply write what's in our equation text box
            writer.WriteAttributeString("EquationText", EquationText);
            writer.WriteStartElement("EquationType");
            writer.WriteAttributeString("SelectedItemContent", (EquationType.SelectedItem as ComboBoxEquationTypeItem).Content as string);
            writer.WriteAttributeString("SelectedItemClassification", ((int)(EquationType.SelectedItem as ComboBoxEquationTypeItem).Classification).ToString());
            writer.WriteEndElement();
        }

        #endregion IXmlSerializable Members

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj is Equation)
            {
                Equation other = obj as Equation;
                return Id.CompareTo(other.Id);
            }
            else
            {
                return -1;
            }
        }

        #endregion IComparable Members
    }
}