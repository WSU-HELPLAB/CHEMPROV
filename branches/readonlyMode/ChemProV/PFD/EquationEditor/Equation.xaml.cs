/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Windows.Data;
using ChemProV.Validation;
using ChemProV.PFD.EquationEditor.Tokens;

namespace ChemProV.PFD.EquationEditor
{
    public partial class Equation : UserControl, IXmlSerializable, IComparable
    {

        public event EventHandler MyTextChanged = delegate { };
        public event EventHandler EquationTokensChagned = delegate { };
        public event EventHandler ReceivedFocus = delegate { };

        /// <summary>
        /// Keeps track of the process unit's unique ID number.  Needed when parsing
        /// to/from XML for saving and loading
        /// </summary>
        private static int equationIdCounter = 0;
        private string equationId;

        private FrameworkElement equationTextBox;

        public FrameworkElement EquationTextBox
        {
            get { return equationTextBox; }
            set { equationTextBox = value; }
        }

        public ObservableCollection<string> VariableNames = new ObservableCollection<string>();

        public Tuple<object, ObservableCollection<IEquationToken>> EquationTokens;

        private bool isReadOnly = false;

        public string EquationText
        {
            get
            {
                if (isReadOnly)
                {
                    return (equationTextBox as Label).Content as string;
                }
                else
                {
                    return (equationTextBox as TextBox).Text;
                }
            }
            set 
            {
                if (isReadOnly)
                {
                    (equationTextBox as Label).Content = value;
                }
                else
                {
                    (equationTextBox as TextBox).Text = value;
                }
            }
        }

        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set 
            { 
                isReadOnly = value;
                setupEquationTextBox();
            }
        }

        private void setupEquationTextBox()
        {
            if (equationTextBox != null)
            {
                this.stackPanel.Children.Remove(equationTextBox);
            }
            if (isReadOnly)
            {
                Label lb = new Label() { FontSize = 11, MinWidth = 200, MaxWidth = 270, MinHeight=20 };
                equationTextBox = lb;
                Border br = new Border() { Child = lb, BorderBrush = new SolidColorBrush(Color.FromArgb(255,70,130,180)), BorderThickness = new Thickness(1) };
                stackPanel.Children.Insert(1, br);
            }
            else
            {
                TextBox tb = new TextBox() { FontSize = 11, AcceptsReturn = false, MinWidth = 200, MaxWidth = 270, TextWrapping = TextWrapping.Wrap };
                equationTextBox = tb;
                stackPanel.Children.Insert(1, tb);

                (EquationTextBox as TextBox).TextChanged += new TextChangedEventHandler(EquationTextBox_TextChanged);
                EquationTextBox.KeyDown += new KeyEventHandler(EquationTextBox_KeyDown);
                EquationTextBox.GotFocus += new RoutedEventHandler(EquationTextBox_GotFocus);
            }
        }

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

        void LocalInit(bool isReadOnly)
        {
            this.isReadOnly = isReadOnly;
            setupEquationTextBox();
            equationIdCounter++;
            equationId = "Eq_" + equationIdCounter;
            EquationTokens = new Tuple<object, ObservableCollection<IEquationToken>>(this, new ObservableCollection<IEquationToken>());
        }

        /// <summary>
        /// We use this as a hack because TextChanged does not fire if Cntrl V is used so we call it ourselves
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EquationTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            //if a key combo was pressed that could change the tokens then assume it chagned
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V ||
                Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z ||
                Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Y ||
                Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.X)
            {
                EquationTextBox_TextChanged(sender, EventArgs.Empty as TextChangedEventArgs);
            }
        }

        void EquationTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ReceivedFocus(this, e);
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

            while(checkMessage[0] != '-')
            {
                checkMessage = checkMessage.Remove(0, 1);
            }
            checkMessage = checkMessage.Remove(0, 1);


            if (checkMessage == ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Solvable) + "\n")
            {
                (parent as EquationEditor).ChangeSolveabiltyStatus(true);
            }
            else
            {
                (parent as EquationEditor).ChangeSolveabiltyStatus(false);
            }
            Silverlight.Controls.ToolTip tooltip = new Silverlight.Controls.ToolTip();
            EquationFeedback.Text = "[" + errorNumber + "]";
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

        private List<string> parseText(string text)
        {
            string unit = "";
            List<string> parsedText = new List<string>();
            Stack<char> parenStack = new Stack<char>();
            Regex operationTerminal = new Regex("[+|//|-|*]");
            bool foundEqualSign = false;
            int i = 0;
            while (i < text.Count<char>())
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
                    while (i < text.Count<char>() && parenStack.Count != 0)
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

                //else because if we remove index it will automatically set index to the next 1 dont wanna increment twice
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
                EquationTokens.Item2.Add(new OperatorToken(text));
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
                    (EquationTextBox as Label).Background = new SolidColorBrush(Colors.White);
                    (EquationTextBox as Label).Foreground = new SolidColorBrush(Colors.Black);
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
                    (EquationTextBox as Label).Background = new SolidColorBrush(Colors.Red);
                    (EquationTextBox as Label).Foreground = new SolidColorBrush(Colors.White);
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
            //if paren this means it is recervisve so we gotta deal with it.
            if (text[0] == '(')
            {
                //set it to 1 past the first element to cut of the (
                text = text.Remove(0, 1);
                text = text.Remove(text.Count<char>() - 1, 1);
                //NEED TO TRIM OF '(' before giving to parseText
                return recersiveVar(parseText(text));
            }
            else
            {
                //if no paren then we just assume it is a variable if it aint then we will flag it later when we check again the table names.
                VariableNames.Add(text);
                EquationTokens.Item2.Add(new VariableToken(text));
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

            //we gotta clear EquationTokens because we are about to go find them again.
            EquationTokens.Item2.Clear();

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
            writer.WriteString(EquationText);
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if(obj is Equation)
            {
                Equation other = obj as Equation;
                return Id.CompareTo(other.Id);
            }
            else
            {
                return -1;
            }
        }

        #endregion
    }
}