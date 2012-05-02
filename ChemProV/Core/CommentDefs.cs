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
        bool AddComment(IComment comment);
        
        int CommentCount
        {
            get;
        }

        /// <summary>
        /// Gets the comment at the specified index. If the index is invalid, then null is returned.
        /// </summary>
        IComment GetCommentAt(int index);

        /// <summary>
        /// Inserts a comment into the collection
        /// </summary>
        /// <param name="comment">Comment to insert</param>
        /// <param name="insertionIndex">Insertion index. This must be in the range [0, CommentCount].</param>
        /// <returns>True on success, false on failure.</returns>
        bool InsertCommentAt(IComment comment, int insertionIndex);

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
        bool ReplaceCommentAt(int index, IComment newComment);
    }

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
}
