/*
Copyright 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

// Do not use any UI-specific code in this class (no Silverlight, no WinForms, no WPF, etc.)
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ChemProV.PFD.EquationEditor.Models;

namespace ChemProV.Core
{
    // Do NOT rename properties in this class. Changing various properties fires the PropertyChanged 
    // event (if non-null) and subscribers to this event rely on the property names staying the same.
    
    /// <summary>
    /// Class to store workspace data without dependencies on Silverlight.
    /// All UI controls should hook up events as needed to monitor changes in the workspace that 
    /// they need to know about.
    /// </summary>
    public class Workspace : INotifyPropertyChanged
    {
        /// <summary>
        /// Degrees of freedom analysis object
        /// </summary>
        private DegreesOfFreedomAnalysis m_dfAnalysis = new DegreesOfFreedomAnalysis();

        private OptionDifficultySetting m_difficulty = OptionDifficultySetting.MaterialBalance;

        private double m_equationEditorFontSize = 14.0;

        protected EquationCollection m_equations = new EquationCollection();

        /// <summary>
        /// List of process units within this workspace
        /// </summary>
        protected List<AbstractProcessUnit> m_procUnits = new List<AbstractProcessUnit>();

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
        /// Collection of sticky notes, a.k.a. free-floating comments, for the workspace.
        /// </summary>
        protected ObservableCollection<ChemProV.PFD.StickyNote.StickyNote_UIIndependent> m_stickyNotes =
            new ObservableCollection<ChemProV.PFD.StickyNote.StickyNote_UIIndependent>();

        private List<AbstractStream> m_streams = new List<AbstractStream>();

        /// <summary>
        /// This is the undo stack. When the "Undo()" function is called, the top collection will be 
        /// popped of this stack and executed. The return value from the execution function will be 
        /// pushed onto the redo stack.
        /// </summary>
        private Stack<UndoRedoCollection> m_undos = new Stack<UndoRedoCollection>();

        public Workspace() { }

        public void AddProcessUnit(AbstractProcessUnit unit)
        {
            m_procUnits.Add(unit);

            if (null != ProcessUnitsCollectionChanged)
            {
                ProcessUnitsCollectionChanged(this, EventArgs.Empty);
            }
        }

        public void AddStream(AbstractStream stream)
        {
            if (null == stream)
            {
                throw new ArgumentNullException(
                    "Cannot add a null stream to a workspace");
            }

            m_streams.Add(stream);

            if (null != StreamsCollectionChanged)
            {
                StreamsCollectionChanged(this, EventArgs.Empty);
            }
        }

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
            // Clear process units
            int procUnitCount = m_procUnits.Count;
            m_procUnits.Clear();
            if (procUnitCount > 0)
            {
                // If there were items in the list we just cleared then invoke the notification event
                if (null != ProcessUnitsCollectionChanged)
                {
                    ProcessUnitsCollectionChanged(this, EventArgs.Empty);
                }
            }

            // Clear streams
            int streamCount = m_streams.Count;
            m_streams.Clear();
            if (streamCount > 0)
            {
                // If there were items in the list we just cleared then invoke the notification event
                if (null != StreamsCollectionChanged)
                {
                    StreamsCollectionChanged(this, EventArgs.Empty);
                }
            }

            // Clear the sticky notes. It's an observable collection so it will fire events if necessary
            m_stickyNotes.Clear();
            
            // Clear misc.
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
        /// Gets the current difficulty setting. To set the difficulty, use the "TrySetDifficulty" method.
        /// </summary>
        public OptionDifficultySetting Difficulty
        {
            get
            {
                return m_difficulty;
            }
        }

        /// <summary>
        /// Gets or sets the font size for equation text in the equations editor. An exception will be 
        /// thrown if an attempt is made to set the value to a number less than or equal to zero.
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

        public AbstractStream GetStream(int id)
        {
            foreach (AbstractStream stream in m_streams)
            {
                if (stream.Id == id)
                {
                    return stream;
                }
            }

            return null;
        }

        public void Load(XDocument doc)
        {
            // Start by clearing
            Clear();

            string setting = doc.Element("ProcessFlowDiagram").Attribute("DifficultySetting").Value;
            TrySetDifficulty((OptionDifficultySetting)Enum.Parse(typeof(OptionDifficultySetting), setting, true));

            // Load process units. We have to do this before the streams because the stream loading 
            // does the attaching to the process units.
            XElement processUnits = doc.Descendants("ProcessUnits").ElementAt(0);
            foreach (XElement xmPU in processUnits.Elements())
            {
                m_procUnits.Add(ProcessUnitFactory.Create(xmPU));
            }

            // Load streams (constructors attach process units)
            XElement streamList = doc.Descendants("Streams").ElementAt(0);
            foreach (XElement streamElement in streamList.Elements())
            {
                // Check the type so we know what to create
                string unitType = (string)streamElement.Attribute("StreamType");

                if ("Chemical" == unitType)
                {
                    m_streams.Add(new ChemicalStream(streamElement, m_procUnits));
                }
                else
                {
                    m_streams.Add(new HeatStream(streamElement, m_procUnits));
                }
                
                // Remember that the properties tables are not stored within the 
                // stream element and get loaded later
            }

            // Now that the streams are loaded, we can load the properties windows
            XElement tablesList = doc.Descendants("PropertiesWindows").ElementAt(0);
            foreach (XElement table in tablesList.Elements())
            {
                // Get the table's target
                string parentName = (string)table.Element("ParentStream");

                // Create the properties table
                AbstractStream parentStream = GetStream(Convert.ToInt32(parentName.Split('_')[1]));
                parentStream.PropertiesTable = new StreamPropertiesTable(table, parentStream);
            }

            // Load equations
            XElement equations = doc.Descendants("Equations").ElementAt(0);
            foreach (XElement xmlEquation in equations.Elements())
            {
                EquationModel rowModel = EquationModel.FromXml(xmlEquation);
                m_equations.Add(rowModel);
            }

            // Load the sticky notes
            XElement stickyNoteList = doc.Descendants("StickyNotes").ElementAt(0);
            foreach (XElement note in stickyNoteList.Elements())
            {
                m_stickyNotes.Add(new PFD.StickyNote.StickyNote_UIIndependent(note, null));
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

            // Fire events
            StreamsCollectionChanged(this, EventArgs.Empty);
            ProcessUnitsCollectionChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Parses X and Y values out of a string of the form "X,Y". On failure, the 
        /// values are set to 0.0 and false is returned.
        /// </summary>
        private static bool ParsePoint(string pointString, out double x, out double y)
        {
            // The expected format of the string is "X,Y"
            if (null == pointString || !pointString.Contains(","))
            {
                x = 0.0;
                y = 0.0;
                return false;
            }

            string[] components = pointString.Split(',');
            if (null == components || components.Length < 2)
            {
                x = 0.0;
                y = 0.0;
                return false;
            }

            if (double.TryParse(components[0], out x) && double.TryParse(components[1], out y))
            {
                return true;
            }

            x = 0.0;
            y = 0.0;
            return false;
        }

        /// <summary>
        /// Gets a reference to the collection of process units in this workspace. This collection 
        /// is read-only. Use the methods of the workspace to add and remove process units.
        /// </summary>
        public ReadOnlyCollection<AbstractProcessUnit> ProcessUnits
        {
            get
            {
                return new ReadOnlyCollection<AbstractProcessUnit>(m_procUnits);
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
        /// Removes a process unit from the workspace. Any streams in the workspace that referenced 
        /// that process unit as a source or destination will have their references set to null.
        /// </summary>
        public void RemoveProcessUnit(AbstractProcessUnit unit)
        {
            // First go through all streams and remove references to this unit
            foreach (AbstractStream stream in m_streams)
            {
                if (object.ReferenceEquals(stream.Source, unit))
                {
                    stream.Source = null;
                }
                if (object.ReferenceEquals(stream.Destination, unit))
                {
                    stream.Destination = null;
                }
            }

            // Now remove it from the collection
            m_procUnits.Remove(unit);

            if (null != ProcessUnitsCollectionChanged)
            {
                ProcessUnitsCollectionChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Removes a stream from the workspace. Any process units that have this stream in 
        /// their list of incoming or outgoing streams will have it removed.
        /// Note that no undo/redo actions are created in this function. If undo/redo actions 
        /// are desired, they must be handled elsewhere.
        /// </summary>
        public void RemoveStream(AbstractStream stream)
        {
            foreach (AbstractProcessUnit apu in m_procUnits)
            {
                if (apu.IncomingStreams.Contains(stream))
                {
                    apu.DetachIncomingStream(stream);
                }
                if (apu.OutgoingStreams.Contains(stream))
                {
                    apu.DetachOutgoingStream(stream);
                }
            }
            
            m_streams.Remove(stream);

            // Invoke the collection change event, if non-null
            if (null != StreamsCollectionChanged)
            {
                StreamsCollectionChanged(this, EventArgs.Empty);
            }
        }

        public void Save(System.IO.Stream outputStream, string versionNumber)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "   ";
            
            //create our XML writer
            using (XmlWriter writer = XmlWriter.Create(outputStream, settings))
            {
                // Create the root node
                writer.WriteStartElement("ProcessFlowDiagram");

                //version number
                writer.WriteAttributeString("ChemProV.version", versionNumber);

                // Write the difficulty setting as an attribute
                writer.WriteAttributeString("DifficultySetting", m_difficulty.ToString());

                // DrawingCanvas and all its elements first
                writer.WriteStartElement("DrawingCanvas");
                {
                    // Process units
                    writer.WriteStartElement("ProcessUnits");
                    foreach (AbstractProcessUnit apu in m_procUnits)
                    {
                        writer.WriteStartElement("GenericProcessUnit");
                        apu.WriteXml(writer);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    // Then streams
                    writer.WriteStartElement("Streams");
                    foreach (AbstractStream stream in m_streams)
                    {
                        writer.WriteStartElement("AbstractStream");
                        stream.WriteXml(writer);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    // Now write the stream property tables. Data-structures-wise these are properties 
                    // of the streams, but the file format was designed (before I came along) such that 
                    // the property tables are under their own element and we have to maintain this 
                    // file format.
                    writer.WriteStartElement("PropertiesWindows");
                    foreach (AbstractStream stream in m_streams)
                    {
                        stream.PropertiesTable.WriteXml(writer, stream.UIDString);
                    }
                    writer.WriteEndElement();

                    // Write "free-floating" sticky notes
                    writer.WriteStartElement("StickyNotes");
                    foreach (PFD.StickyNote.StickyNote_UIIndependent sn in m_stickyNotes)
                    {
                        writer.WriteStartElement("StickyNote");
                        sn.WriteXml(writer);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                // Write equations
                writer.WriteStartElement("EquationEditor");
                writer.WriteStartElement("Equations");
                foreach (EquationModel model in m_equations)
                {
                    writer.WriteStartElement("EquationModel");
                    model.WriteXml(writer);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();

                // At one time there was a feedback window, but it's not in use anymore. To maintain 
                // file format compatibility though, we still write the element.
                writer.WriteStartElement("FeedbackWindow");
                writer.WriteEndElement();

                // Write degrees of freedom analysis
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

                // End the root node
                writer.WriteEndElement();
            }
        }

        public ObservableCollection<ChemProV.PFD.StickyNote.StickyNote_UIIndependent> StickyNotes
        {
            get
            {
                return m_stickyNotes;
            }
        }

        /// <summary>
        /// Returns true if a stream with the specified ID already exists in the stream collection, 
        /// false if it does not.
        /// </summary>
        public bool StreamExists(int id)
        {
            foreach (AbstractStream stream in m_streams)
            {
                if (stream.Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a reference to the collection of streams in this workspace. This collection 
        /// is read-only. Use the methods of the workspace to add and remove process streams. 
        /// There is no guarantee about the order of streams in this collection. It is NOT 
        /// guaranteed, for example, that the most recently added item with be at the end 
        /// of the collection.
        /// </summary>
        public ReadOnlyCollection<AbstractStream> Streams
        {
            get
            {
                return new ReadOnlyCollection<AbstractStream>(m_streams);
            }
        }

        public bool TrySetDifficulty(OptionDifficultySetting setting)
        {
            if (m_difficulty == setting)
            {
                // We're already there
                return true;
            }

            // Go through all the streams and process units and see if they are available on 
            // the specified difficulty setting
            foreach (AbstractStream stream in m_streams)
            {
                if (!stream.IsAvailableWithDifficulty(setting))
                {
                    return false;
                }
            }
            foreach (AbstractProcessUnit apu in m_procUnits)
            {
                if (!apu.IsAvailableWithDifficulty(setting))
                {
                    return false;
                }
            }

            // Everything is ok if we come here, so set the new setting, invoke the property 
            // change event and return true.
            m_difficulty = setting;
            PropertyChanged(this, new PropertyChangedEventArgs("Difficulty"));
            return true;
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
                PropertyChanged(this, new PropertyChangedEventArgs("UndoTitle"));
                PropertyChanged(this, new PropertyChangedEventArgs("RedoTitle"));
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

        #region Events

        public event EventHandler ProcessUnitsCollectionChanged = delegate { };

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public event EventHandler StreamsCollectionChanged = delegate { };

        #endregion
    }
}
