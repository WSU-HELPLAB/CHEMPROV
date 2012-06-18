/*
Copyright 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds
using ChemProV.PFD.EquationEditor.Models;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace ChemProV.Core
{
    /// <summary>
    /// Class to store workspace data without dependencies on Silverlight
    /// It's incomplete at this point, but it's created with the intention of eventually having 
    /// all core logic in Silverlight independent code and the Silverlight stuff would then just 
    /// be a UI-layer on top of this.
    /// All UI controls should hook up events as needed to monitor changes in the workspace that 
    /// they need to know about.
    /// </summary>
    public class Workspace
    {
        /// <summary>
        /// Degrees of freedom analysis object
        /// </summary>
        private DegreesOfFreedomAnalysis m_dfAnalysis = new DegreesOfFreedomAnalysis();

        protected EquationCollection m_equations = new EquationCollection();

        public Workspace() { }

        public void Clear()
        {
            m_equations.Clear();
            m_dfAnalysis.Comments.Clear();
            m_dfAnalysis.CommentsVisible = false;
            m_dfAnalysis.Text = string.Empty;
        }

        public DegreesOfFreedomAnalysis DegreesOfFreedomAnalysis
        {
            get
            {
                return m_dfAnalysis;
            }
        }

        /// <summary>
        /// Gets the collection of equations in the workspace.
        /// </summary>
        public EquationCollection Equations
        {
            get
            {
                return m_equations;
            }
        }

        public void Load(XDocument doc)
        {
            // Start by clearing
            m_equations.Clear();
            m_dfAnalysis.Comments.Clear();

            // Load equations
            XElement equations = doc.Descendants("Equations").ElementAt(0);
            foreach (XElement xmlEquation in equations.Elements())
            {
                EquationModel rowModel = EquationModel.FromXml(xmlEquation);
                m_equations.Add(rowModel);
            }

            // Check for degrees of freedom analysis
            XElement df = doc.Element("ProcessFlowDiagram").Element("DegreesOfFreedomAnalysis");
            if (null != df)
            {
                m_dfAnalysis.Text = df.Element("Text").Value;

                foreach (XElement el in df.Elements("Comment"))
                {
                    string userName = string.Empty;
                    XAttribute userAttr = el.Attribute("UserName");
                    if (null != userAttr)
                    {
                        userName = userAttr.Value;
                    }
                    m_dfAnalysis.Comments.Add(new Core.BasicComment(el.Value, userName));
                }
            }
            else
            {
                m_dfAnalysis.Text = string.Empty;
            }
        }

        /// <summary>
        /// TEMPORARY until further refactoring. In the future this class should have 1 load and 1 save method that 
        /// load from/save to streams.
        /// Writes the degrees of freedom analysis data to XML. This includes the degrees of freedom text as well 
        /// as any accompanying comments.
        /// </summary>
        public void WriteDegreesOfFreedomAnalysis(XmlWriter writer)
        {
            writer.WriteStartElement("DegreesOfFreedomAnalysis");
            writer.WriteElementString("Text", m_dfAnalysis.Text);
            foreach (Core.BasicComment bc in m_dfAnalysis.Comments)
            {
                writer.WriteStartElement("Comment");
                if (!string.IsNullOrEmpty(bc.CommentUserName))
                {
                    writer.WriteAttributeString("UserName", bc.CommentUserName);
                }
                writer.WriteValue(bc.CommentText);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }
}
