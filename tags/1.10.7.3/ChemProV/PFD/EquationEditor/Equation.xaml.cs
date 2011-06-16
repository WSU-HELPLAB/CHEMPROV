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

using ChemProV.PFD.EquationEditor.Tokens;

namespace ChemProV.PFD.EquationEditor
{
    public partial class Equation : UserControl, IXmlSerializable, IComparable
    {

        /// <summary>
        /// Keeps track of the process unit's unique ID number.  Needed when parsing
        /// to/from XML for saving and loading
        /// </summary>
        private static int equationIdCounter = 0;
        private string equationId;

        public ObservableCollection<string> VariableNames = new ObservableCollection<string>();

        public Tuple<object, ObservableCollection<IEquationToken>> EquationTokens;

        public event EventHandler EquationTokensChagned = delegate { };

        public Equation()
        {
            InitializeComponent();
            equationIdCounter++;
            equationId = "Eq_" + equationIdCounter;
            EquationTokens = new Tuple<object, ObservableCollection<IEquationToken>>(this, new ObservableCollection<IEquationToken>());
            EquationTextBox.TextChanged += new TextChangedEventHandler(EquationTextBox_TextChanged);
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
            Regex operationTerminal = new Regex("[+|/|-|*]");
            bool foundEqualSign = false;
            int i = 0;
            while (i < text.Count<char>())
            {
                if ((operationTerminal.IsMatch(text[i] + "")) || text[i] == ' ')
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
                        i++;
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

            if (parsedText.Count < 0)
            {
                isVar(parsedText[i]);
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
                EquationTextBox.Background = new SolidColorBrush(Colors.White);
                EquationTextBox.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                EquationTextBox.Background = new SolidColorBrush(Colors.Red);
                EquationTextBox.Foreground = new SolidColorBrush(Colors.White);
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
            writer.WriteString(EquationTextBox.Text);
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