/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using ChemProV.PFD;
using ChemProV.PFD.EquationEditor;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.UI;

namespace ChemProV.Validation.Feedback
{
    internal enum changeFeedback
    {
        HighlightFeedback,
        SetFeedback,
        RemoveFeedback
    }

    public enum FeedbackStatus
    {
        Errors,
        ChangedButNotChecked,
        NoErrors
    }

    public partial class FeedbackWindow : UserControl, IXmlSerializable
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

        private WorkSpace workSpaceReference;

        public WorkSpace WorkSpaceReference
        {
            get { return workSpaceReference; }
            set { workSpaceReference = value; }
        }

        public List<Feedback> ListOfFeedback
        {
            get { return listOfFeedback; }
        }

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
                    selectedFeedback.Border.Background = new SolidColorBrush(Colors.White);
                    ApplyFeedback(changeFeedback.HighlightFeedback, selectedFeedback.Target, false);
                }
                if (value != null)
                {
                    //we gotta changed the new one to yellow
                    value.Border.Background = new SolidColorBrush(Colors.Yellow);
                    ApplyFeedback(changeFeedback.HighlightFeedback, value.Target, true);
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
                FeedBackStackPanel.Children.Remove(fb.TextBlock);
                ApplyFeedback(changeFeedback.RemoveFeedback, fb.Target);
            }
            listOfFeedback.Clear();
            FeedBackStackPanel.Children.Clear();
            if (messages != null && messages.Count != 0)
            {
                foreach (object key in messages.Keys)
                {
                    //sometimes, the key can be a list of objects.  In this scenario, we just pass the list
                    AttachFeedbackMessage(key, String.Join("\n", messages[key].ToArray()));
                }

                string checkMessage = messages.Values.First<List<string>>()[0];

                while (checkMessage[0] != '-')
                {
                    checkMessage = checkMessage.Remove(0, 1);
                }
                checkMessage = checkMessage.Remove(0, 1);

                if (checkMessage.Trim() == ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Solvable).Trim())
                {
                    FeedbackStatusChanged(FeedbackStatus.NoErrors);
                }
                else
                {
                    FeedbackStatusChanged(FeedbackStatus.Errors);
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

            FeedBackStackPanel.Children.Add(fb);
            listOfFeedback.Add(fb);

            result = reg.Match(message).Value;

            //ok got the result should be something like [###], now take of the brackets
            result = result.Remove(0, 1);
            result = result.Remove(result.Count<char>() - 1, 1);
            Int32.TryParse(result, out errorNumber);

            //set the event listner
            fb.TextBlock.MouseLeftButtonDown += new MouseButtonEventHandler(textBox_MouseLeftButtonDown);

            //chop up the feedback message into 80-character bits
            string[] wrappedText = Wrap(fb.TextBlock.Text, 80);
            string feedbackText = string.Join("\n", wrappedText);

            //since we are setting the feedback we highlight bool is not used so does not matter what it is.
            ApplyFeedback(changeFeedback.SetFeedback, fb.Target, false, feedbackText, errorNumber);
        }

        private void textBox_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            //need to find which textbox this in our list of textboxes
            foreach (Feedback fb in listOfFeedback)
            {
                if (fb.TextBlock == sender)
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

        private void ApplyFeedback(changeFeedback action, object target, bool highlight = false, string message = "", int errorNumber = 0)
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
                    else if (action == changeFeedback.RemoveFeedback)
                    {
                        stream.RemoveFeedback();
                    }
                }
            }
            else if (target is IEnumerable<IPropertiesWindow>)
            {
                foreach (IPropertiesWindow table in (target as IEnumerable<IPropertiesWindow>))
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
            else if (target is EquationControl)
            {
                EquationControl equation = target as EquationControl;
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

        public void FeedbackStatusChanged(FeedbackStatus status)
        {
            if (status == FeedbackStatus.Errors)
            {
                FeedbackStatusEllipse.Fill = new SolidColorBrush(Colors.Red);
            }
            else if (status == FeedbackStatus.ChangedButNotChecked)
            {
                FeedbackStatusEllipse.Fill = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                FeedbackStatusEllipse.Fill = new SolidColorBrush(Colors.Green);
            }
        }

        public void LoadXmlElements(XElement doc)
        {
            XElement feedback = doc.Descendants("Feedback").FirstOrDefault();
            if (feedback != null)
            {
                foreach (XElement xmlFeedback in feedback.Elements())
                {
                    string[] targetIds = (xmlFeedback.Attribute("target").Value as string).Split(", ".ToCharArray());
                    object targets;
                    if (workSpaceReference.GetobjectFromId(targetIds[0]) is IStream)
                    {
                        List<IStream> list = new List<IStream>();
                        foreach (string targetID in targetIds)
                        {
                            list.Add(workSpaceReference.GetobjectFromId(targetID) as IStream);
                        }
                        targets = list;
                    }
                    else if (workSpaceReference.GetobjectFromId(targetIds[0]) is IPropertiesWindow)
                    {
                        List<IPropertiesWindow> list = new List<IPropertiesWindow>();
                        foreach (string targetID in targetIds)
                        {
                            list.Add(workSpaceReference.GetobjectFromId(targetID) as IPropertiesWindow);
                        }
                        targets = list;
                    }
                    else
                    {
                        //must only be one thing so set it to tgars
                        targets = workSpaceReference.GetobjectFromId(targetIds[0]);
                    }

                    Feedback fb = new Feedback(targets, xmlFeedback.Attribute("message").Value);
                    fb.Id = xmlFeedback.Attribute("id").Value;
                }
            }
        }

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (Feedback feedback in listOfFeedback)
            {
                writer.WriteStartElement("Feedback");
                object target = feedback.Target;

                string targetIds = "";
                if (target is IEnumerable<IStream>)
                {
                    if ((target as IEnumerable<IStream>).Count<IStream>() > 0)
                    {
                        foreach (IStream stream in (target as IEnumerable<IStream>))
                        {
                            targetIds += ", " + stream.Id;
                        }
                        targetIds = targetIds.Remove(0, 2);
                    }
                }
                else if (target is IEnumerable<IPropertiesWindow>)
                {
                    if ((target as IEnumerable<IPropertiesWindow>).Count<IPropertiesWindow>() > 0)
                    {
                        foreach (IPropertiesWindow table in (target as IEnumerable<IPropertiesWindow>))
                        {
                            targetIds += ", " + table.Id;
                        }
                        targetIds = targetIds.Remove(0, 2);
                    }
                }
                else if (target is IPfdElement)
                {
                    targetIds = (target as IPfdElement).Id;
                }
                else if (target is EquationControl)
                {
                    targetIds = (target as EquationControl).Id;
                }

                writer.WriteAttributeString("id", feedback.Id);

                writer.WriteAttributeString("target", targetIds);

                writer.WriteAttributeString("message", feedback.TextBlock.Text);

                writer.WriteEndElement();
            }
        }

        #endregion IXmlSerializable Members
    }
}