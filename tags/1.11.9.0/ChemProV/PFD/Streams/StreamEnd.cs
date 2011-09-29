/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;

namespace ChemProV.PFD.Streams
{
    /// <summary>
    /// This represents a the end of stream (either the source or the desination)
    /// </summary>
    public abstract class StreamEnd : IPfdElement
    {
        /// <summary>
        /// This is not currently used, but must have it since IPfdElement has it.
        /// </summary>
        public event EventHandler LocationChanged;

        public bool Selected
        {
            get
            {
                return stream.Selected;
            }
            set
            {
                stream.Selected = value;
            }
        }

        /// <summary>
        /// This is not currently used, but must have it since IPfdElement has it.
        /// </summary>
        public event EventHandler SelectionChanged;

        /// <summary>
        /// This the IStrem that is connected to the end.
        /// </summary>
        protected IStream stream;

        public IStream Stream
        {
            get { return stream; }
            set { stream = value; }
        }

        public StreamEnd()
        {
        }

        #region IPfdElement Members

        /// <summary>
        /// StreamEnds don't really need an Id, so always return 0.
        /// </summary>
        public string Id
        {
            get
            {
                return "0";
            }
            set
            {
            }
        }

        #endregion IPfdElement Members

        public void HighlightFeedback(bool highlight)
        {
            stream.HighlightFeedback(highlight);
        }

        public void SetFeedback(string feedbackMessage, int errorNumber)
        {
            stream.SetFeedback(feedbackMessage, errorNumber);
        }

        public void RemoveFeedback()
        {
            stream.RemoveFeedback();
        }
    }
}