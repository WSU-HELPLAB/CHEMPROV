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
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace ChemProV.Core
{
    /// <summary>
    /// Interface for an object that can hold a collection of comments
    /// </summary>
    public interface ICommentCollection : IXmlSerializable
    {
        /// <summary>
        /// Adds a comment to the collection. If the comment cannot be added because the collection 
        /// is read-only or some other error occurs, then false is returned. Otherwise the comment 
        /// is added as the last item in the collection and true is returned.
        /// </summary>
        bool AddComment(Comment comment);
        
        int CommentCount
        {
            get;
        }

        /// <summary>
        /// Gets the comment at the specified index. If the index is invalid, then null is returned.
        /// </summary>
        Comment GetCommentAt(int index);

        /// <summary>
        /// Removes the comment at the specified index from the collection. If the collection is 
        /// read-only, the index is invalid, or the current user does not have sufficient 
        /// permissions then false is returned. Otherwise, the comment at the specified index is 
        /// removed from the collection and true is returned.
        /// </summary>
        bool RemoveCommentAt(int index);

        /// <summary>
        /// Replaces the comment at the specified index. If the collection is read-only, the index is 
        /// invalid, or the current user does not have sufficient permissions then false is returned. 
        /// Otherwise, the comment at the specified index is replaced and true is returned.
        /// </summary>
        bool ReplaceCommentAt(int index, Comment newComment);
    }

    /// <summary>
    /// Represents an immutable comment. At the time of this writing, we don't really have a strong idea 
    /// of how the comment system should be designed for ChemProV. Thus, it is expected that this class 
    /// may be extended in the future, but it MUST REMAIN IMMUTABLE. This is part of an effort to create 
    /// more solid code in ChemProV. Immutable comments will require the invokation of the ReplaceCommentAt 
    /// function in the comment collection to alter a comment. The implementation of that function can 
    /// include relevant permissions checks and whatnot.
    /// </summary>
    public class Comment
    {
        protected string m_text;
        
        protected string m_userName;

        public Comment(string userName, string text)
        {
            m_userName = userName;
            m_text = text;
        }

        public string Text
        {
            get
            {
                return m_text;
            }
        }

        public string UserName
        {
            get
            {
                return m_userName;
            }
        }
    }
}
