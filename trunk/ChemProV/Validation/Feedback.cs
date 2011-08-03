/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
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

namespace ChemProV.Validation
{
    public class Feedback
    {
        public object target;
        
        /// <summary>
        /// Textblock is what holds the text
        /// </summary>
        public TextBlock textBlock;

        /// <summary>
        /// The boarder is used so we can change the background make it yellow or white if selected or not.
        /// </summary>
        public Border boarder;

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
