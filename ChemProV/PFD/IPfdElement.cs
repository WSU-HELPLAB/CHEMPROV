/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;

namespace ChemProV.PFD
{
    public interface IPfdElement
    {
        /// <summary>
        /// Gets or sets the GenericProcessUnit's unique ID number
        /// </summary>
        String Id
        {
            get;
            set;
        }

        /// <summary>
        /// highlight or unhighlights the feedback area
        /// </summary>
        /// <param name="highlight">true if you want highlight, false if u want to unhighlight</param>
        void HighlightFeedback(bool highlight);

        void SetFeedback(string feedbackMessage, int errorNumber);

        void RemoveFeedback();

        /// <summary>
        /// Indicates whether or not the IPfdElement has been selected.
        /// </summary>
        Boolean Selected
        {
            get;
            set;
        }

        /// <summary>
        /// Fired whenever the stream's selection status changes
        /// </summary>
        event EventHandler SelectionChanged;
    }
}