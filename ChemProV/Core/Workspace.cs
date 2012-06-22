/*
Copyright 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ChemProV.PFD.EquationEditor.Models;

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
    public class Workspace : INotifyPropertyChanged
    {
        /// <summary>
        /// Degrees of freedom analysis object
        /// </summary>
        private DegreesOfFreedomAnalysis m_dfAnalysis = new DegreesOfFreedomAnalysis();

        private double m_equationEditorFontSize = 14.0;

        protected EquationCollection m_equations = new EquationCollection();

        /// <summary>
        /// This is the redo stack. When the "Redo()" function is called, the top collection will be 
        /// popped of this stack and executed. The return value from the execution function will be 
        /// pushed onto the undo stack.
        /// NEVER add anything to this stack or m_undos. Use the AddUndo function. The undo system 
        /// is intentially designed so that redos are created automatically in the Undo() function. 
        /// The Undo() function should be the ONLY place where you see m_redos.Push and the Redo() 
        /// function is the ONLY place where you should see m_redos.Pop().
        /// </summary>
        private Stack<UndoRedoCollection> m_redos = new Stack<UndoRedoCollection>();

        /// <summary>
        /// This is the undo stack. When the "Undo()" function is called, the top collection will be 
        /// popped of this stack and executed. The return value from the execution function will be 
        /// pushed onto the redo stack.
        /// </summary>
        private Stack<UndoRedoCollection> m_undos = new Stack<UndoRedoCollection>();

        public Workspace() { }

        /// <summary>
        /// Adds an undo action to the undo stack. You'll notice there is no AddRedo function. This is 
        /// intentional because upon execution of an undo (via a call to "Undo()") the redo action is 
        /// automatically created and pushed onto the redo stack.
        /// </summary>
        /// <param name="collection">Collection of undo actions to push.</param>
        /// <returns>True if the collection was successfully added to the stack, false otherwise.</returns>
        public bool AddUndo(UndoRedoCollection collection)
        {
            m_undos.Push(collection);

            // Adding a new undo clears the redo stack
            m_redos.Clear();

            // Send a notification that this may have changed the undo title
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("UndoTitle"));
            }

            return true;
        }

        public void Clear()
        {
            m_equations.Clear();
            m_dfAnalysis.Comments.Clear();
            m_dfAnalysis.CommentsVisible = false;
            m_dfAnalysis.Text = string.Empty;

            // Clear undos/redos
            m_undos.Clear();
            m_redos.Clear();
        }

        public DegreesOfFreedomAnalysis DegreesOfFreedomAnalysis
        {
            get
            {
                return m_dfAnalysis;
            }
        }

        /// <summary>
        /// Gets or sets the font size for equation text in the equations editor. An exception will be 
        /// thrown if an attempt is made to set the value to a number less than or equal to zero.
        /// 
        /// Do not rename this property. Changing it will fire the PropertyChanged event (if non-null) 
        /// and subscribers to this event rely on the property name staying the same.
        /// </summary>
        public double EquationEditorFontSize
        {
            get
            {
                return m_equationEditorFontSize;
            }
            set
            {
                if (value == m_equationEditorFontSize)
                {
                    // No change
                    return;
                }

                if (value <= 0.0)
                {
                    throw new ArgumentException("Equation editor font size must be greater than 0");
                }

                // Set the new value
                m_equationEditorFontSize = value;

                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("EquationEditorFontSize"));
                }
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
            Clear();

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

        public void Redo()
        {
            if (m_redos.Count > 0)
            {
                // Logic:
                // 1. Pop redo collection on top of stack
                // 2. Execute it
                // 3. Take its return value and push it onto the undo stack
                // (done in 1 line below)
                m_undos.Push(m_redos.Pop().Execute(this));

                // Send a notification that this may have changed the undo/redo titles
                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("UndoTitle"));
                    PropertyChanged(this, new PropertyChangedEventArgs("RedoTitle"));
                }
            }
        }

        /// <summary>
        /// Gets the number of redos currently on the redo stack.
        /// </summary>
        public int RedoCount
        {
            get
            {
                return m_redos.Count;
            }
        }

        public string RedoTitle
        {
            get
            {
                if (0 == m_redos.Count)
                {
                    return "Redo";
                }
                return m_redos.Peek().Title;
            }
        }

        /// <summary>
        /// Executes the undo collection that is on the top of the undo stack. If the undo stack 
        /// is empty then no action is taken.
        /// </summary>
        public void Undo()
        {
            if (m_undos.Count > 0)
            {
                // Logic:
                // 1. Pop undo collection on top of stack
                // 2. Execute it
                // 3. Take its return value and push it onto the redo stack
                // (done in 1 line below)
                m_redos.Push(m_undos.Pop().Execute(this));

                // Send a notification that this may have changed the undo/redo titles
                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("UndoTitle"));
                    PropertyChanged(this, new PropertyChangedEventArgs("RedoTitle"));
                }
            }
        }

        /// <summary>
        /// Gets the number of undos currently on the undo stack.
        /// </summary>
        public int UndoCount
        {
            get
            {
                return m_undos.Count;
            }
        }

        public string UndoTitle
        {
            get
            {
                if (0 == m_undos.Count)
                {
                    return "Undo";
                }
                return m_undos.Peek().Title;
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

        #region Events

        public event PropertyChangedEventHandler PropertyChanged = null;

        #endregion
    }
}
