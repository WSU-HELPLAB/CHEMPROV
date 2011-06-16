/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ChemProV.Validation.Feedback
{
    /// <summary>
    /// This contains the reference to the textblock in the feedbackwindow as well as the assoicated object which broke the rule
    /// </summary>
    public class Feedback
    {
        /// <summary>
        /// This is the target which needs a feedback icon to appear.
        /// </summary>
        public object target;
        
        /// <summary>
        /// Textblock is what holds the text
        /// </summary>
        public TextBlock textBlock;

        /// <summary>
        /// The boarder is used so we can change the background make it yellow or white if selected or not.
        /// </summary>
        public Border boarder;

        /// <summary>
        /// This is the constructor for a feedback.
        /// </summary>
        /// <param name="target">the object(s) which broke the rule</param>
        /// <param name="s">the message for the tooltip and textBlock</param>
        public Feedback(object target, string s)
        {
            textBlock = new TextBlock();
            boarder = new Border();
            textBlock.Text = s;
            boarder.Child = textBlock;
            textBlock.TextWrapping = TextWrapping.Wrap;
            this.target = target;
            
        }
    }
}
