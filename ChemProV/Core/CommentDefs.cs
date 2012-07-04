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
    ///// <summary>
    ///// Interface for an object that can hold a collection of comments
    ///// </summary>
    //public interface ICommentCollection
    //{
    //    /// <summary>
    //    /// Adds a comment to the collection. If the comment cannot be added because the collection 
    //    /// is read-only or some other error occurs, then false is returned. Otherwise the comment 
    //    /// is added as the last item in the collection and true is returned.
    //    /// </summary>
    //    bool AddComment(IComment comment);
        
    //    int CommentCount
    //    {
    //        get;
    //    }

    //    /// <summary>
    //    /// Gets the comment at the specified index. If the index is invalid, then null is returned.
    //    /// </summary>
    //    IComment GetCommentAt(int index);

    //    /// <summary>
    //    /// Inserts a comment into the collection
    //    /// </summary>
    //    /// <param name="comment">Comment to insert</param>
    //    /// <param name="insertionIndex">Insertion index. This must be in the range [0, CommentCount].</param>
    //    /// <returns>True on success, false on failure.</returns>
    //    bool InsertCommentAt(IComment comment, int insertionIndex);

    //    /// <summary>
    //    /// Removes the comment at the specified index from the collection. If the collection is 
    //    /// read-only, the index is invalid, or the current user does not have sufficient 
    //    /// permissions then false is returned. Otherwise, the comment at the specified index is 
    //    /// removed from the collection and true is returned.
    //    /// </summary>
    //    bool RemoveCommentAt(int index);

    //    /// <summary>
    //    /// Replaces the comment at the specified index. If the collection is read-only, the index is 
    //    /// invalid, or the current user does not have sufficient permissions then false is returned. 
    //    /// Otherwise, the comment at the specified index is replaced and true is returned.
    //    /// </summary>
    //    bool ReplaceCommentAt(int index, IComment newComment);
    //}

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
