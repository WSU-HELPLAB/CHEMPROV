/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Windows;
using System.Windows.Controls;

namespace ChemProV.Validation.Feedback
{
    /// <summary>
    /// This contains the reference to the textblock in the feedbackwindow as well as the assoicated object which broke the rule
    /// </summary>
    public class Feedback : UserControl
    {
        /// <summary>
        /// Keeps track of the process unit's unique ID number.  Needed when parsing
        /// to/from XML for saving and loading
        /// </summary>
        private static int feedbackIdCounter = 0;

        private string feedbackId;

        /// <summary>
        /// Gets or sets the equation's unique ID number
        /// </summary>
        public String Id
        {
            get
            {
                return feedbackId;
            }
            set
            {
                feedbackId = value;
            }
        }

        /// <summary>
        /// This is the target which needs a feedback icon to appear.
        /// </summary>
        private object target;

        public object Target
        {
            get { return target; }
            set { target = value; }
        }

        /// <summary>
        /// Textblock is what holds the text
        /// </summary>
        private TextBlock textBlock;

        public TextBlock TextBlock
        {
            get { return textBlock; }
            set { textBlock = value; }
        }

        /// <summary>
        /// The boarder is used so we can change the background make it yellow or white if selected or not.
        /// </summary>
        private Border border;

        public Border Border
        {
            get { return border; }
            set { border = value; }
        }

        /// <summary>
        /// This is the constructor for a feedback.
        /// </summary>
        /// <param name="target">the object(s) which broke the rule</param>
        /// <param name="s">the message for the tooltip and textBlock</param>
        public Feedback(object target, string s)
        {
            feedbackIdCounter++;
            feedbackId = "Fb_" + feedbackIdCounter;
            textBlock = new TextBlock();
            border = new Border();
            textBlock.Text = s;
            border.Child = textBlock;
            textBlock.TextWrapping = TextWrapping.Wrap;
            this.target = target;
            this.Content = new Border() { Child = border };
        }
    }
}