/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System;

namespace ChemProV.PFD
{
    public interface IPfdElement
    {
        /// <summary>
        /// Fired when the PFD element's location is changed
        /// </summary>
        event EventHandler LocationChanged;

        /// <summary>
        /// Gets or sets the IProcessUnit's unique ID number
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