/*
Copyright 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ChemProV.Core
{
    public class DegreesOfFreedomAnalysis : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private ObservableCollection<BasicComment> m_comments = new ObservableCollection<BasicComment>();

        private bool m_commentsVisible = false;

        private string m_text = null;

        public DegreesOfFreedomAnalysis() { }

        /// <summary>
        /// Gets the list of comments. Note that modifying the returned list will NOT invoke the 
        /// PropertyChanged event. But you can attach observers to the returned object if 
        /// desired to monitor changes in the collection.
        /// Each DegreesOfFreedomAnalysis object has one comment collection and a direct 
        /// reference to it is returned here. The collection is never re-created internally and 
        /// its lifespan is the same as that of the DegreesOfFreedomAnalysis parent object.
        /// </summary>
        public ObservableCollection<BasicComment> Comments
        {
            get
            {
                return m_comments;
            }
        }

        public bool CommentsVisible
        {
            get
            {
                return m_commentsVisible;
            }
            set
            {
                if (m_commentsVisible == value)
                {
                    // No change
                    return;
                }

                m_commentsVisible = value;

                // Invoke PropertyChanged if non-null
                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CommentsVisible"));
                }
            }
        }

        public string Text
        {
            get
            {
                return m_text;
            }
            set
            {
                if (null == m_text)
                {
                    if (null == value)
                    {
                        // No change: was null and is being set to null
                        return;
                    }

                    // Change: was null and is being set to non-null
                    m_text = value;

                    if (null != PropertyChanged)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Text"));
                    }
                }
                else
                {
                    if (m_text.Equals(value))
                    {
                        // No change
                        return;
                    }

                    // Change
                    m_text = value;

                    if (null != PropertyChanged)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Text"));
                    }
                }
            }
        }
    }
}
