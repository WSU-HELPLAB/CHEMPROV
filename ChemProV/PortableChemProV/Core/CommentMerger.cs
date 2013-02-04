/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/
using System;
using System.Collections.Generic;
using System.IO;
using ChemProV.Logic;
using ChemProV.Logic.Equations;

namespace ChemProV.Core
{
    /// <summary>
    /// A UI-independent (therefore Silverlight indepedent) comment merger
    /// </summary>
    public static class CommentMerger
    {
        /// <summary>
        /// Merges comments from two Xml document streams into a new output Xml document stream. Comment 
        /// merging is not commutative. One document must be considered to be the parent and another a 
        /// child. ALL content from the parent will appear in the output. Comments from the child 
        /// document will only be written to the output if they exist for shared entities. That is, if  
        /// there is a comment in the child document that is tied to a process unit with Id=GPU_30, then 
        /// it will only be written the output document if the parent also contained a process unit with 
        /// the same Id.
        /// </summary>
        public static void Merge(Stream parent, string parentUserNameIfNotInXml, Stream child,
            string childUserNameIfNotInXml, Stream output)
        {
            // We need all streams to be non-null
            if (null == parent)
            {
                throw new ArgumentNullException(
                    "Parent stream for comment merging cannot be null");
            }
            if (null == child)
            {
                throw new ArgumentNullException(
                    "Child stream for comment merging cannot be null");
            }
            if (null == output)
            {
                throw new ArgumentNullException(
                    "Output stream for comment merging cannot be null");
            }
            
            // Load the workspaces from the streams
            Workspace wsParent = new Workspace();
            wsParent.Load(parent);
            Workspace wsChild = new Workspace();
            wsChild.Load(child);

            // What we will do in this method is alter wsParent to contain relevant content from the 
            // child workspace and then save it to the output stream.

            // Start by setting user names for comments in both workspaces. We leave the user names 
            // alone if they are not null or empty but otherwise we set them to the values specified 
            // by the caller.
            SetUserNameIfAbsent(wsParent, parentUserNameIfNotInXml);
            SetUserNameIfAbsent(wsChild, childUserNameIfNotInXml);

            // Start with the free-floating sticky note comments. We want to take the ones from the 
            // child and add them into the parent. But we want to avoid duplicates in the process.
            foreach (StickyNote sn in wsChild.StickyNotes)
            {
                // If they have the same text and location then we'll skip
                if (WorkspaceUtility.ContainsFFSNWithValues(wsParent, sn.Text,
                    new MathCore.Vector(sn.LocationX, sn.LocationY)))
                {
                    continue;
                }

                // Add it to the parent
                wsParent.StickyNotes.Add(sn);
            }

            // Next do process units in the child
            foreach (AbstractProcessUnit apuChild in wsChild.ProcessUnits)
            {
                AbstractProcessUnit apuParent = wsParent.GetProcessUnit(apuChild.Id);

                // If the parent workspace doesn't contain a process unit with the same ID then we 
                // skip it
                if (null == apuParent)
                {
                    continue;
                }

                foreach (StickyNote comment in apuChild.Comments)
                {
                    if (WorkspaceUtility.CollectionContainsItemWithText(apuParent.Comments, comment.Text))
                    {
                        // Skip it if there's already a comment with the same text
                        continue;
                    }
                    
                    // Add it to the parent process unit
                    apuParent.Comments.Add(comment);
                }
            }

            // Now do streams in the child
            foreach (AbstractStream sChild in wsChild.Streams)
            {
                AbstractStream sParent = wsParent.GetStream(sChild.Id);

                // If the parent workspace doesn't contain a stream with the same ID then we 
                // skip it
                if (null == sParent)
                {
                    continue;
                }

                foreach (StickyNote comment in sChild.Comments)
                {
                    if (WorkspaceUtility.CollectionContainsItemWithText(sParent.Comments, comment.Text))
                    {
                        // Skip it if there's already a comment with the same text
                        continue;
                    }

                    // Add the comment to the parent stream
                    sParent.Comments.Add(comment);
                }
            }

            // Equation comments need to be merged as well
            foreach (EquationModel emChild in wsChild.Equations)
            {
                // Get the equation object in the parent with the same ID
                EquationModel emParent = wsParent.Equations.GetById(emChild.Id);

                // If we can't find it then move on to the next
                if (null == emParent)
                {
                    continue;
                }

                // Now add each comment in the child that isn't already in the parent
                foreach (BasicComment bcChild in emChild.Comments)
                {
                    if (!emParent.ContainsComment(bcChild.CommentText))
                    {
                        emParent.Comments.Add(bcChild);
                    }
                }
            }
            
            // Lastly we deal with the comments for the degrees of freedom analysis. We only 
            // merge in comments from the child if the analysis text is the same in both.
            if (wsParent.DegreesOfFreedomAnalysis.Text == wsChild.DegreesOfFreedomAnalysis.Text)
            {
                foreach (BasicComment bcChild in wsChild.DegreesOfFreedomAnalysis.Comments)
                {
                    if (!wsParent.DegreesOfFreedomAnalysis.ContainsComment(bcChild.CommentText))
                    {
                        wsParent.DegreesOfFreedomAnalysis.Comments.Add(bcChild);
                    }
                }
            }

            // Now that we have everything merged into the parent workspace, we just save it to the 
            // output stream
            wsParent.Save(output);
        }

        /// <summary>
        /// Goes through a list of StickyNote objects and sets the user name to the specified value 
        /// if the StickyNote's user name is null or empty.
        /// </summary>
        private static void SetUserNameIfAbsent(IList<StickyNote> commentList, string userName)
        {
            foreach (StickyNote sn in commentList)
            {
                if (string.IsNullOrEmpty(sn.UserName))
                {
                    sn.UserName = userName;
                }
            }
        }

        /// <summary>
        /// Checks for any comments in the entire workspace that lack a user name and replaces the 
        /// user name with the specified string.
        /// </summary>
        private static void SetUserNameIfAbsent(Workspace workspace, string userName)
        {
            // We will only be setting user names if they are null or empty so there's 
            // no point in 'changing' null or empty strings to a null or empty string.
            if (string.IsNullOrEmpty(userName))
            {
                // Ignore
                return;
            }
            
            foreach (AbstractStream stream in workspace.Streams)
            {
                SetUserNameIfAbsent(stream.Comments, userName);
            }
            foreach (AbstractProcessUnit apu in workspace.ProcessUnits)
            {
                SetUserNameIfAbsent(apu.Comments, userName);
            }

            foreach (EquationModel equation in workspace.Equations)
            {
                foreach (BasicComment comment in equation.Comments)
                {
                    if(string.IsNullOrEmpty(comment.CommentUserName))
                    {
                        comment.CommentUserName = userName;
                    }
                }
            }
            
            // Go through the free-floating sticky notes in the workspace as well
            SetUserNameIfAbsent(workspace.StickyNotes, userName);
        }
    }
}
