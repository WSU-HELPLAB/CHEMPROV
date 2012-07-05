/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// E.O.
// This file contatins defintions for objects relevant to adding comments to items 
// in ChemProV.

using System;
using System.Xml.Serialization;

namespace ChemProV.Core
{
    public interface IComment
    {
        string CommentText
        {
            get;
        }

        string CommentUserName
        {
            get;
        }
    }

    /// <summary>
    /// Provides a basic implementation of the IComment interface. The object is mutable and has events 
    /// for when properties change.
    /// </summary>
    public class BasicComment : IComment
    {
        private string m_text;

        private string m_user;

        public event EventHandler OnTextChanged = null;

        public event EventHandler OnUserNameChanged = null;

        public BasicComment() : this(string.Empty, string.Empty) { }
        
        public BasicComment(string text, string userName)
        {
            m_text = text;
            m_user = userName;
        }
        
        public string CommentText
        {
            get { return m_text; }
            set
            {
                // See if this changes the comment text
                if ((null != m_text && !m_text.Equals(value)) ||
                    (null == m_text && null != value))
                {
                    m_text = value;

                    if (null != OnTextChanged)
                    {
                        OnTextChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public string CommentUserName
        {
            get { return m_user; }
            set
            {
                // See if this changes the comment user name
                if ((null != m_user && !m_user.Equals(value)) ||
                    (null == m_user && null != value))
                {
                    m_user = value;

                    if (null != OnUserNameChanged)
                    {
                        OnUserNameChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public bool Equals(BasicComment other)
        {
            if (null == other)
            {
                return false;
            }

            bool usersMatch;
            if (string.IsNullOrEmpty(other.m_user))
            {
                usersMatch = string.IsNullOrEmpty(this.m_user);
            }
            else
            {
                usersMatch = other.m_user.Equals(this.m_user);
            }

            return other.m_text.Equals(this.m_text) && usersMatch;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as BasicComment);
        }
    }

}
