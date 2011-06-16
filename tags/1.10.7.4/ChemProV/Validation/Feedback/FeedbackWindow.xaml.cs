/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/
using System;
using System.Collections;
using System.Collections.Generic;
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

using ChemProV.UI;
using ChemProV.UI.DrawingCanvas;
using ChemProV.PFD;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Streams.PropertiesTable.Chemical;
using ChemProV.PFD.Streams.PropertiesTable;
using ChemProV.Validation;
using ChemProV.Validation.Feedback;
using ChemProV.Validation.Rules;
using ChemProV.PFD.EquationEditor;

namespace ChemProV.Validation.Feedback
{
    enum changeFeedback
    {
        HighlightFeedback,
        SetFeedback,
        RemoveFeedback
    }

    public partial class FeedbackWindow : UserControl
    {
        /// <summary>
        /// This the class that controls the feedbackwindow
        /// </summary>
        public FeedbackWindow()
        {
            InitializeComponent();
            FeedbackScrollViewer.DataContext = this;
        }

        private List<Feedback> listOfFeedback = new List<Feedback>();
        private Feedback selectedFeedback;

        /// <summary>
        /// This returns the selectedFeedback or setting it will cause the old value to be unhighlighted both the feedback.TextBlock
        /// as well as its target.  Then it will highlight the new value that it is being changed too both the TextBlock and the target.
        /// </summary>
        public Feedback SelectedFeedback
        {
            get { return selectedFeedback; }
            set
            {
                if (selectedFeedback != null)
                {
                    //we gotta changed the old one back to white

                    //Set the textbox back to white
                    selectedFeedback.boarder.Background = new SolidColorBrush(Colors.White);
                    ApplyFeedback(changeFeedback.HighlightFeedback, selectedFeedback.target, false);
                }
                if (value != null)
                {
                    //we gotta changed the new one to yellow
                    value.boarder.Background = new SolidColorBrush(Colors.Yellow);
                    ApplyFeedback(changeFeedback.HighlightFeedback, value.target, true);
                }

                selectedFeedback = value;
            }
        }

        /// <summary>
        /// This function sets what should be in the feedback window as well as setting the corrosponding object in either the equation textbox or 
        /// </summary>
        /// <param name="messages"></param>
        public void updateFeedbackWindow(Dictionary<object, List<string>> messages)
        {
            //Set the SelectedFeedback to null since we will removing everything.
            SelectedFeedback = null;

            //We need to remove the old feedback stuff since we will add the "new" feedback later
            foreach (Feedback fb in listOfFeedback)
            {
                FeedBackStackPanel.Children.Remove(fb.textBlock);
                ApplyFeedback(changeFeedback.RemoveFeedback, fb.target);
            }
            listOfFeedback.Clear();
            FeedBackStackPanel.Children.Clear();
            if (messages != null)
            {
                foreach (object key in messages.Keys)
                {
                    //sometimes, the key can be a list of objects.  In this scenario, we just pass the list
                    AttachFeedbackMessage(key, String.Join("\n", messages[key].ToArray()));
                }
            }
        }

        /// <summary>
        /// This functions attaches the feedback messages to their targets
        /// </summary>
        /// <param name="target"></param>
        /// <param name="message"></param>
        private void AttachFeedbackMessage(object target, string message)
        {
            Feedback fb = new Feedback(target, message);
            Regex reg = new Regex(@"^\[\d+\]");
            string result;
            int errorNumber = -1;
            
            FeedBackStackPanel.Children.Add(fb.boarder);
            listOfFeedback.Add(fb);

            result = reg.Match(message).Value;

            //ok got the result should be something like [###], now take of the brackets
            result = result.Remove(0,1);
            result = result.Remove(result.Count<char>() - 1, 1);
            Int32.TryParse(result, out errorNumber);

            //set the event listner
            fb.textBlock.MouseLeftButtonDown += new MouseButtonEventHandler(textBox_MouseLeftButtonDown);

            //chop up the feedback message into 80-character bits
            string[] wrappedText = Wrap(fb.textBlock.Text, 80);
            string feedbackText = string.Join("\n", wrappedText);

            //since we are setting the feedback we highlight bool is not used so does not matter what it is.
            ApplyFeedback(changeFeedback.SetFeedback, fb.target, false, feedbackText, errorNumber);
        }

        void textBox_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            //need to find which textbox this in our list of textboxes
            foreach (Feedback fb in listOfFeedback)
            {
                if (fb.textBlock == sender)
                {
                    if (SelectedFeedback == fb)
                    {
                        SelectedFeedback = null;
                        break;
                    }
                    else
                    {
                        SelectedFeedback = fb;
                        break;
                    }
                }
            }
        }

        void ApplyFeedback(changeFeedback action, object target, bool highlight = false, string message = "", int errorNumber = 0)
        {
            if (target is IEnumerable<IStream>)
            {
                foreach (IStream stream in (target as IEnumerable<IStream>))
                {
                    if (action == changeFeedback.HighlightFeedback)
                    {
                        stream.HighlightFeedback(highlight);
                    }
                    else if (action == changeFeedback.SetFeedback)
                    {
                        stream.SetFeedback(message, errorNumber);
                    }
                    else if(action == changeFeedback.RemoveFeedback)
                    {
                        stream.RemoveFeedback();
                    }
                    
                }
            }
            else if (target is IEnumerable<IPropertiesTable>)
            {
                foreach (IPropertiesTable table in (target as IEnumerable<IPropertiesTable>))
                {
                    if (action == changeFeedback.HighlightFeedback)
                    {
                        table.HighlightFeedback(highlight);
                    }
                    else if (action == changeFeedback.SetFeedback)
                    {
                        table.SetFeedback(message, errorNumber);
                    }
                    else if (action == changeFeedback.RemoveFeedback)
                    {
                        table.RemoveFeedback();
                    }
                }
            }
            else if (target is IPfdElement)
            {
                IPfdElement ipfd = target as IPfdElement;

                if (action == changeFeedback.HighlightFeedback)
                {
                    ipfd.HighlightFeedback(highlight);
                }
                else if (action == changeFeedback.SetFeedback)
                {
                    ipfd.SetFeedback(message, errorNumber);
                }
                else if (action == changeFeedback.RemoveFeedback)
                {
                    ipfd.RemoveFeedback();
                }
            }
            else if (target is Equation)
            {
                Equation equation = target as Equation;
                if (action == changeFeedback.HighlightFeedback)
                {
                    equation.HighlightFeedback(highlight);
                }
                else if (action == changeFeedback.SetFeedback)
                {
                    equation.SetFeedback(message, errorNumber);
                }
                else if (action == changeFeedback.RemoveFeedback)
                {
                    equation.RemoveFeedback();
                }
            }
        }

        /// <summary>
        /// Wraps the supplied string every maxLength characters.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public string[] Wrap(string text, int maxLength)
        {
            //text = text.Replace("\n", " ");
            text = text.Replace("\r", " ");
            text = text.Replace(".", ". ");
            text = text.Replace(">", "> ");
            text = text.Replace("\t", " ");
            text = text.Replace(",", ", ");
            text = text.Replace(";", "; ");
            text = text.Replace("<br>", " ");
            text = text.Replace(" ", " ");

            string[] Words = text.Split(' ');
            int currentLineLength = 0;
            List<string> Lines = new List<string>(text.Length / maxLength);
            string currentLine = "";
            bool InTag = false;

            foreach (string currentWord in Words)
            {
                //ignore html
                if (currentWord.Length > 0)
                {

                    if (currentWord.Substring(0, 1) == "<")
                        InTag = true;

                    if (InTag)
                    {
                        //handle filenames inside html tags
                        if (currentLine.EndsWith("."))
                        {
                            currentLine += currentWord;
                        }
                        else
                            currentLine += " " + currentWord;

                        if (currentWord.IndexOf(">") > -1)
                            InTag = false;
                    }
                    else
                    {
                        if (currentLineLength + currentWord.Length + 1 < maxLength)
                        {
                            currentLine += " " + currentWord;
                            currentLineLength += (currentWord.Length + 1);
                        }
                        else
                        {
                            Lines.Add(currentLine);
                            currentLine = currentWord;
                            currentLineLength = currentWord.Length;
                        }
                    }
                }

            }
            if (currentLine != "")
                Lines.Add(currentLine);

            string[] textLinesStr = new string[Lines.Count];
            Lines.CopyTo(textLinesStr, 0);
            return textLinesStr;
        }
    }
}
