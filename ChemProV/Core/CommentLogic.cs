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

namespace ChemProV.Core
{
    /// <summary>
    /// Provides static logic for comments/comment collections
    /// </summary>
    public static class CommentLogic
    {
        /// <summary>
        /// Searches a comment collection to see if is has a comment with text that matches the 
        /// provided value.
        /// </summary>
        public static bool ContainsCommentWithText(ICommentCollection collection, string commentText)
        {
            for (int i = 0; i < collection.CommentCount; i++)
            {
                if (collection.GetCommentAt(i).CommentText.Equals(commentText))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
