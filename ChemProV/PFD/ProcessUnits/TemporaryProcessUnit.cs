/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Specialized;

namespace ChemProV.PFD.ProcessUnits
{
    /// <summary>
    /// Temporary process units are those that need to disappear the moment they don't have
    /// any attached incoming/outgoing streams.
    /// </summary>
    public class TemporaryProcessUnit : GenericProcessUnit
    {
        public event EventHandler RemoveRequest = delegate { };

        private int m_maxIncomingStreams, m_maxOutgoingStreams, m_maxIncomingHeatStreams, m_maxOutgoingHeatStreams;

        /// <summary>
        /// A short description of the process unit.  Not more than a few words in length.
        /// </summary>
        private string m_description = "Temporary Process Unit";

        /// <summary>
        /// Default empty constructor
        /// </summary>
        public TemporaryProcessUnit(string iconSource, int maxIncomingStreams,
            int maxOutgoingStreams, int maxIncomingHeatStreams, int maxOutgoingHeatStreams,
            string description)
            : base(iconSource)
        {
            init();

            m_maxIncomingStreams = maxIncomingStreams;
            m_maxOutgoingStreams = maxOutgoingStreams;
            m_maxIncomingHeatStreams = maxIncomingHeatStreams;
            m_maxOutgoingHeatStreams = maxOutgoingHeatStreams;

            // Store the description
            m_description = description;
        }

        /// <summary>
        /// A short description of the process unit.  Not more than a few words in length.
        /// </summary>
        public override string Description
        {
            get
            {
                return m_description;
            }
        }

        /// <summary>
        /// To be called by constructor to set up object
        /// </summary>
        private void init()
        {
            incomingStreams.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ItemRemoved);
            outgoingStreams.CollectionChanged += new NotifyCollectionChangedEventHandler(ItemRemoved);
            this.Width = 1;
            this.Height = 1;
        }

        /// <summary>
        /// To be called whenever a stream is removed from the incoming/outgoing list.  If this number
        /// ever reaches 0 for both, then we need to let the owner know that we need to be removed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemRemoved(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IncomingStreams.Count == 0 && OutgoingStreams.Count == 0)
            {
                RemoveRequest(this, new EventArgs());
            }
        }

        public override int MaxIncomingStreams
        {
            get
            {
                return m_maxIncomingStreams;
            }
        }

        public override int MaxOutgoingStreams
        {
            get
            {
                return m_maxOutgoingStreams;
            }
        }

        public override int MaxIncomingHeatStreams
        {
            get
            {
                return m_maxIncomingHeatStreams;
            }
        }
        
        public override int MaxOutgoingHeatStreams
        {
            get
            {
                return m_maxOutgoingHeatStreams;
            }
        }
    }
}