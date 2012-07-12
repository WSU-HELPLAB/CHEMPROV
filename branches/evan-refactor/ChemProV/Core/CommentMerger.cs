/*
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
using ChemProV.Logic.Equations;
using ChemProV.Logic;

namespace ChemProV.Core
{
    /// <summary>
    /// A UI-independent (therefore Silverlight indepedent) comment merger
    /// </summary>
    public static class CommentMerger
    {
        private static XElement FirstNodeOrNull(XDocument doc, string nodeName)
        {
            if (doc.Descendants(nodeName).Count() > 0)
            {
                return doc.Descendants(nodeName).ElementAt(0);
            }

            return null;
        }
        
        /// <summary>
        /// Loads comments from the document, adding the user name to the XML tree where necessary
        /// </summary>
        private static StickyNote[] LoadCommentsFrom(XDocument doc, string userNameIfNotInXml)
        {
            List<StickyNote> comments = new List<StickyNote>();
            
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
                        
                        StickyNote sn = new StickyNote(
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
                        
                        StickyNote sn = new StickyNote(
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
                
                comments.Add(new Logic.StickyNote(note, null));
            }

            return comments.ToArray();
        }

        /// <summary>
        /// Builds a dictionary that maps an equation model ID key to an equation model from 
        /// the document
        /// </summary>
        private static Dictionary<int, EquationModel> LoadEquations(XDocument doc, string userNameIfNotInXml)
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

                // Fill in the user names in the comments if they are null or empty
                foreach (BasicComment bc in eqModel.Comments)
                {
                    if (string.IsNullOrEmpty(bc.CommentUserName))
                    {
                        bc.CommentUserName = userNameIfNotInXml;
                    }
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
            List<StickyNote> childComments = new List<StickyNote>();
            childComments.AddRange(LoadCommentsFrom(docChild, childUserNameIfNotInXml));

            // Also load all comments from the parent
            List<StickyNote> parentComments = new List<StickyNote>();
            parentComments.AddRange(LoadCommentsFrom(docParent, parentUserNameIfNotInXml));

            // Remove all child comments from the list that are duplicates (same text and parent ID)
            for (int i = 0; i < childComments.Count; i++)
            {
                foreach (StickyNote tempParent in parentComments)
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
                foreach (StickyNote sn in childComments)
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
                foreach (StickyNote sn in childComments)
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
            foreach (StickyNote sn in childComments)
            {
                if (!string.IsNullOrEmpty(sn.ParentId))
                {
                    continue;
                }

                sn.WriteElement(stickyNoteXmlParent);
            }

            // Now we need to take care of equation comments (annotations). Start by loading from 
            // the child document
            Dictionary<int, EquationModel> childModels = LoadEquations(docChild, childUserNameIfNotInXml);
            // Next find <Equations> node in parent XML
            XElement eqs = docParent.Descendants("Equations").ElementAt(0);
            if (null != eqs)
            {
                // Iterate through <EquationModel> elements in the parent. These can have 0 or more 
                // <Annotation> children that represent comments for that equation
                foreach (XElement em in eqs.Elements("EquationModel"))
                {
                    int parentElementId = Convert.ToInt32(em.Attribute("Id").Value);
                    // Find the child equation model with the same ID
                    if (!childModels.ContainsKey(parentElementId))
                    {
                        // No match -> continue
                        continue;
                    }

                    // Get the model from the matching child
                    EquationModel childModel = childModels[parentElementId];
                    // If there are 0 comments then we don't need to merge
                    if (0 == childModel.Comments.Count)
                    {
                        continue;
                    }

                    // Add user name to XML elements in parent if need be
                    foreach (XElement parentAnnoEl in em.Elements("Annotation"))
                    {
                        XAttribute attr = parentAnnoEl.Attribute("UserName");
                        if (null == attr)
                        {
                            parentAnnoEl.SetAttributeValue("UserName", parentUserNameIfNotInXml);
                        }
                    }

                    // Load the parent equation model from the XML node
                    EquationModel parentModel = EquationModel.FromXml(em);

                    // Add all comments to the parent XML that don't already exist
                    foreach (BasicComment bcChild in childModel.Comments)
                    {
                        // We don't want to add the comment if there is an existing comment with 
                        // the exact same comment text value
                        if (!parentModel.ContainsComment(bcChild.CommentText))
                        {
                            XElement annoElement = new XElement("Annotation");
                            annoElement.Value = bcChild.CommentText;
                            if (!string.IsNullOrEmpty(bcChild.CommentUserName))
                            {
                                annoElement.SetAttributeValue("UserName", bcChild.CommentUserName);
                            }
                            em.Add(annoElement);
                        }
                    }                
                }
            }

            // Now do comments for the degrees of freedom analysis. We ONLY do this if the text for 
            // each analysis is the same in the two documents. The commens are considered to be specific 
            // to the analysis, so it wouldn't make sense to include comments from the child if the child 
            // has a different analysis.
            XElement dfParentEl = FirstNodeOrNull(docParent, "DegreesOfFreedomAnalysis");
            XElement dfChildEl = FirstNodeOrNull(docChild, "DegreesOfFreedomAnalysis");
            if (null != dfParentEl && null != dfChildEl)
            {
                string parentDFText = dfParentEl.Element("Text").Value;
                string childDFText = dfChildEl.Element("Text").Value;
                if (null != parentDFText && null != childDFText && parentDFText.Equals(childDFText))
                {
                    // First load the comments from the parent into memory
                    List<string> existing = new List<string>();
                    foreach (XElement parentDFCommentEl in dfParentEl.Elements("Comment"))
                    {
                        if (!string.IsNullOrEmpty(parentDFCommentEl.Value))
                        {
                            existing.Add(parentDFCommentEl.Value);
                        }

                        // See if it has a "UserName" attribute. If not, we want to add one, provided 
                        // we have one that was passed into this method.
                        if (!string.IsNullOrEmpty(parentUserNameIfNotInXml))
                        {
                            XAttribute tempAttr = parentDFCommentEl.Attribute("UserName");
                            if (null == tempAttr)
                            {
                                parentDFCommentEl.SetAttributeValue("UserName", parentUserNameIfNotInXml);
                            }
                        }
                    }

                    // Now add any comments from the children that aren't in the parent
                    foreach (XElement childDFComment in dfChildEl.Elements("Comment"))
                    {
                        string childDFCommentString = childDFComment.Value;
                        string childDFUserName = null;
                        XAttribute tempAttr = childDFComment.Attribute("UserName");
                        if (null != tempAttr)
                        {
                            childDFUserName = tempAttr.Value;
                        }
                        else
                        {
                            childDFUserName = childUserNameIfNotInXml;
                        }
                        
                        if (!string.IsNullOrEmpty(childDFCommentString) && 
                            !existing.Contains(childDFCommentString))
                        {
                            XElement dfCommentEl = new XElement("Comment");
                            dfCommentEl.Value = childDFCommentString;
                            if (!string.IsNullOrEmpty(childDFUserName))
                            {
                                dfCommentEl.SetAttributeValue("UserName", childDFUserName);
                            }
                            dfParentEl.Add(dfCommentEl);
                        }
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
