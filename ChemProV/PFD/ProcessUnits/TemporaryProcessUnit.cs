/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
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

        /// <summary>
        /// Default empty constructor
        /// </summary>
        public TemporaryProcessUnit()
            : base()
        {
            init();
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
    }
}