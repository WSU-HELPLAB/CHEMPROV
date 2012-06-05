﻿/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using ChemProV.PFD.EquationEditor.Models;
using ChemProV.PFD.StickyNote;

// Dependencies:
//  1. StickyNote_UIIndependent.cs
//  2. EquationModel.cs (and all its dependencies)

namespace ChemProV.Core
{
    /// <summary>
    /// A UI-independent (therefore Silverlight indepedent) comment merger
    /// </summary>
    public static class CommentMerger
    {
        /// <summary>
        /// Loads comments from the document, adding the user name to the XML tree where necessary
        /// </summary>
        private static StickyNote_UIIndependent[] LoadCommentsFrom(XDocument doc, string userNameIfNotInXml)
        {
            List<StickyNote_UIIndependent> comments = new List<StickyNote_UIIndependent>();
            
            // Process units first...
            XElement processUnits = doc.Descendants("ProcessUnits").ElementAt(0);
            foreach (XElement unit in processUnits.Elements())
            {
                XElement cmtsElement = unit.Element("Comments");
                if (null != cmtsElement)
                {
                    foreach (XElement cmt in cmtsElement.Elements())
                    {
                        // Ensure that it has a user name
                        SetUserNameIfAbsent(cmt, userNameIfNotInXml);
                        
                        StickyNote_UIIndependent sn = new StickyNote_UIIndependent(
                            cmt, unit.Attribute("Id").Value);

                        comments.Add(sn);
                    }
                }
            }
            
            // ... then streams ...
            XElement streamList = doc.Descendants("Streams").ElementAt(0);
            foreach (XElement stream in streamList.Elements())
            {
                // Load any comments that are present
                XElement cmtsElement = stream.Element("Comments");
                if (null != cmtsElement)
                {
                    foreach (XElement cmt in cmtsElement.Elements())
                    {
                        // Ensure that it has a user name
                        SetUserNameIfAbsent(cmt, userNameIfNotInXml);
                        
                        StickyNote_UIIndependent sn = new StickyNote_UIIndependent(
                            cmt, stream.Attribute("Id").Value);

                        comments.Add(sn);
                    }
                }
            }
            
            // ... and end with free-floating sticky notes
            XElement stickyNoteList = doc.Descendants("StickyNotes").ElementAt(0);
            foreach (XElement note in stickyNoteList.Elements())
            {
                // Ensure that it has a user name
                SetUserNameIfAbsent(note, userNameIfNotInXml);
                
                comments.Add(new PFD.StickyNote.StickyNote_UIIndependent(note, null));
            }

            return comments.ToArray();
        }

        /// <summary>
        /// Builds a dictionary that maps an equation model ID key to an equation model from 
        /// the document
        /// </summary>
        private static Dictionary<int, EquationModel> LoadEquations(XDocument doc)
        {
            Dictionary<int, EquationModel> d = new Dictionary<int, EquationModel>();

            XElement eqs = doc.Descendants("Equations").ElementAt(0);
            if (null == eqs) { return d; }

            // Iterate through <EquationModel> elements
            foreach (XElement em in eqs.Elements("EquationModel"))
            {
                EquationModel eqModel = EquationModel.FromXml(em);
                if (!string.IsNullOrEmpty(eqModel.Equation))
                {
                    d[eqModel.Id] = eqModel;
                }
            }


            return d;
        }
        
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
            // Load the two XML documents
            parent.Position = 0;
            child.Position = 0;
            XDocument docParent = XDocument.Load(parent);
            XDocument docChild = XDocument.Load(child);

            // Make sure both are non-null
            if (null == docChild || null == docParent)
            {
                throw new Exception("XDocument load error in comment merging");
            }

            // Start by loading all comments from the "child". Note that this doesn't include 
            // equation annotations
            List<StickyNote_UIIndependent> childComments = new List<StickyNote_UIIndependent>();
            childComments.AddRange(LoadCommentsFrom(docChild, childUserNameIfNotInXml));

            // Also load all comments from the parent
            List<StickyNote_UIIndependent> parentComments = new List<StickyNote_UIIndependent>();
            parentComments.AddRange(LoadCommentsFrom(docParent, parentUserNameIfNotInXml));

            // Remove all child comments from the list that are duplicates (same text and parent ID)
            for (int i = 0; i < childComments.Count; i++)
            {
                foreach (StickyNote_UIIndependent tempParent in parentComments)
                {
                    if (tempParent.ParentId == childComments[i].ParentId &&
                        tempParent.Text == childComments[i].Text)
                    {
                        childComments.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }

            // We will build the merged document in "docParent". What we have to do is iterate through 
            // process units, streams, free-floating sticky notes, and equation annotations and do 
            // merges where appropriate.

            // Start with the process units. For each one we need to see if there are relevant comments 
            // to merge in from the child.
            XElement processUnits = docParent.Descendants("ProcessUnits").ElementAt(0);
            foreach (XElement unit in processUnits.Elements())
            {
                // Before we attempt to merge in child comments, we should check to see if we have 
                // a parent user name. If we do, we should add it to comments that have no user name
                if (!string.IsNullOrEmpty(parentUserNameIfNotInXml))
                {
                    XElement parentUnitCmtsEl = unit.Element("Comments");
                    if (null != parentUnitCmtsEl)
                    {
                        foreach (XElement parentUnitComment in parentUnitCmtsEl.Elements("Comment"))
                        {
                            XElement userName = parentUnitComment.Element("UserName");
                            if (null == userName)
                            {
                                parentUnitComment.Add(new XElement("UserName", parentUserNameIfNotInXml));
                            }
                            else if (string.IsNullOrEmpty(userName.Value))
                            {
                                userName.Value = parentUserNameIfNotInXml;
                            }
                        }
                    }
                }
                
                // We've already removed duplicates from the child list, so if we have something in 
                // that list with a matching ID then we add it
                foreach (StickyNote_UIIndependent sn in childComments)
                {
                    if (sn.ParentId == unit.Attribute("Id").Value)
                    {
                        // Get the <Comments> element
                        XElement cmtElement = unit.Element("Comments");

                        // If one doesn't exist then create it now
                        if (null == cmtElement)
                        {
                            cmtElement = new XElement("Comments");
                            unit.Add(cmtElement);
                        }

                        // Write the comment XML
                        sn.WriteElement(cmtElement);
                    }
                }
            }

            // Do a similar thing with streams
            foreach (XElement stream in docParent.Descendants("Streams").ElementAt(0).Elements())
            {
                // Before we attempt to merge in child comments, we should check to see if we have 
                // a parent user name. If we do, we should add it to comments that have no user name
                if (!string.IsNullOrEmpty(parentUserNameIfNotInXml))
                {
                    XElement parentUnitCmtsEl = stream.Element("Comments");
                    if (null != parentUnitCmtsEl)
                    {
                        foreach (XElement parentUnitComment in parentUnitCmtsEl.Elements("Comment"))
                        {
                            XElement userName = parentUnitComment.Element("UserName");
                            if (null == userName)
                            {
                                parentUnitComment.Add(new XElement("UserName", parentUserNameIfNotInXml));
                            }
                            else if (string.IsNullOrEmpty(userName.Value))
                            {
                                userName.Value = parentUserNameIfNotInXml;
                            }
                        }
                    }
                }
                
                // We've already removed duplicates from the child list, so if we have something in 
                // that list with a matching ID then we add it
                foreach (StickyNote_UIIndependent sn in childComments)
                {
                    if (sn.ParentId == stream.Attribute("Id").Value)
                    {
                        // Get the <Comments> element
                        XElement cmtElement = stream.Element("Comments");

                        // If one doesn't exist then create it now
                        if (null == cmtElement)
                        {
                            cmtElement = new XElement("Comments");
                            stream.Add(cmtElement);
                        }

                        // Write the comment XML
                        sn.WriteElement(cmtElement);
                    }
                }
            }

            // Add all free-floating sticky notes
            XElement stickyNoteXmlParent = docParent.Descendants("StickyNotes").ElementAt(0);
            foreach (StickyNote_UIIndependent sn in childComments)
            {
                if (!string.IsNullOrEmpty(sn.ParentId))
                {
                    continue;
                }

                sn.WriteElement(stickyNoteXmlParent);
            }

            // Now we need to take care of equation annotations. Start by loading from the 
            // child document
            Dictionary<int, EquationModel> childModels = LoadEquations(docChild);
            // Next find <Equations> node in parent XML
            XElement eqs = docParent.Descendants("Equations").ElementAt(0);
            if (null != eqs)
            {
                // Iterate through <EquationModel> elements in the parent
                foreach (XElement em in eqs.Elements("EquationModel"))
                {
                    int parentElementId = Convert.ToInt32(em.Attribute("Id").Value);
                    // Find the child equation model with the same ID
                    if (!childModels.ContainsKey(parentElementId))
                    {
                        // No match -> continue
                        continue;
                    }

                    // Get the annotation from the matching child
                    string childAnno = childModels[parentElementId].Annotation;
                    // If it's null or empty then we don't need to merge
                    if (string.IsNullOrEmpty(childAnno))
                    {
                        continue;
                    }

                    // Get the parent annotation element
                    XElement annotationElement = em.Element("Annotation");
                    if (null == annotationElement)
                    {
                        throw new Exception(
                            "Element \"EquationModel\" is missing child \"Annotation\" element");
                    }
                    // Make sure it's not null (so we can append to it)
                    if (null == annotationElement.Value)
                    {
                        annotationElement.Value = string.Empty;
                    }

                    // Merge the annotations
                    if (!string.IsNullOrEmpty(childUserNameIfNotInXml))
                    {
                        annotationElement.Value += "\r\n\r\n--- " + childUserNameIfNotInXml + 
                            " ---\r\n" + childAnno;
                    }
                    else
                    {
                        annotationElement.Value += "\r\n\r\n--- (unknown user) ---\r\n" + childAnno;
                    }                    
                }
            }

            // Save the modified parent to the output stream
            docParent.Save(output);
        }

        private static void SetUserNameIfAbsent(XElement cmtOrSticky, string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                // Ignore
                return;
            }
            
            // Make sure we have the correct type of XElement
            string elNameLwr = cmtOrSticky.Name.LocalName.ToLower();
            if (!elNameLwr.Equals("stickynote") && !elNameLwr.Equals("comment"))
            {
                throw new ArgumentException("XElement for a sticky note must be either a " +
                    "<Comment> or <StickyNote> element. Element was named: " + elNameLwr + 
                    "\r\nMethod: SetUserNameIfAbsent");
            }
            
            XElement userNameEl = cmtOrSticky.Element("UserName");
            if (null == userNameEl)
            {
                cmtOrSticky.Add(new XElement("UserName", userName));
            }
            else if (string.IsNullOrEmpty(userNameEl.Value))
            {
                userNameEl.Value = userName;
            }
        }
    }
}