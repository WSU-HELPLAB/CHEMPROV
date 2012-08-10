/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using ChemProV.PFD.Streams;
using System.Xml.Linq;
using ChemProV.Logic;

namespace ChemProV.UI
{
    public enum ProcessUnitBorderColor
    {
        Selected,
        NotAcceptingStreams,
        AcceptingStreams,
        NoBorder
    }

    /// <summary>
    /// Represents the control used to represent all process units. The inheritance hierarchy of process 
    /// units is completely beneath the UI layer in the logic layer. This control just wraps around an 
    /// AbstractProcessUnit object.
    /// </summary>
    public partial class ProcessUnitControl : UserControl, PFD.IPfdElement, Core.ICanvasElement, UI.ICommentControlManager, Core.IRemoveSelfFromCanvas
    {
        #region Instance Variables

        /// <summary>
        /// Private instance var used to keep track of whether or not we've been selected
        /// </summary>
        private bool isSelected = false;

        protected DrawingCanvas m_canvas;

        /// <summary>
        /// Flag that gets temporarily set to true when we modify m_pu. Modifications to m_pu 
        /// will trigger property change events and we want to ignore these in certain 
        /// circumstances.
        /// </summary>
        private bool m_ignoreUnitPropertyChanges = false;

        private string m_labelOnEditStart = null;

        /// <summary>
        /// Reference to the process unit data structure. This control is essentially just a UI 
        /// layer to represent this data structure.
        /// Do not rename. Many comments refer to "m_pu" and those comments won't make sense if 
        /// this is renamed.
        /// </summary>
        protected AbstractProcessUnit m_pu;

        /// <summary>
        /// The list of sticky note controls that have been created to represent comments for this 
        /// process unit. Remember that the actual comments for the process unit are stored in the 
        /// process unit data structure (m_pu) and this list updates as the list of comments in that 
        /// object change.
        /// We store this because the process unit is responsible for creating and removing its sticky 
        /// note controls on the drawing canvas.
        /// </summary>
        private List<StickyNoteControl> m_stickyNotes = new List<StickyNoteControl>();

        private static Brush s_selectedBorderBrush = new SolidColorBrush(Colors.Yellow);
        private Brush GreenBorderBrush = new SolidColorBrush(Colors.Green);
        private Brush RedBorderBrush = new SolidColorBrush(Colors.Red);
        private Brush TransparentBorderBrush = new SolidColorBrush(Colors.Transparent);

        public event EventHandler LocationChanged = delegate { };
        public event EventHandler SelectionChanged = delegate { };

        #endregion Instance Variables

        /// <summary>
        /// Default constructor. Should not be used (hence the private) but I think the designer view 
        /// requires this to work.
        /// </summary>
        private ProcessUnitControl() : this(null, null) { }

        /// <summary>
        /// Use CreateOnCanvas
        /// </summary>
        private ProcessUnitControl(DrawingCanvas canvas, AbstractProcessUnit processUnit)
        {
            InitializeComponent();

            // Store a reference to the canvas and process unit
            m_pu = processUnit;
            m_canvas = canvas;

            // Create the icon image
            BitmapImage bmp = new BitmapImage();
            bmp.UriSource = new Uri(GetIconSource(processUnit.GetType()), UriKind.Relative);
            ProcessUnitImage.SetValue(Image.SourceProperty, bmp);

            // Set the background color based on the subprocess
            Color clr;
            if (Core.App.TryParseColor(m_pu.Subprocess, out clr))
            {
                ProcessUnitBorder.Background = new SolidColorBrush(clr);
            }

            ProcessUnitNameText.MouseLeftButtonDown += new MouseButtonEventHandler(ProcessUnitNameText_MouseLeftButtonDown);
            ProcessUnitNameBox.MouseLeftButtonDown += new MouseButtonEventHandler(ProcessUnitNameBox_MouseLeftButtonDown);
            ProcessUnitNameBox.LostFocus += new RoutedEventHandler(ProcessUnitNameBox_LostFocus);
            ProcessUnitNameBox.KeyDown += new KeyEventHandler(ProcessUnitNameBox_KeyDown);

            // Create the icon image
            ProcessUnitImage.SetValue(Image.SourceProperty, ProcessUnitImage.Source);

            // Set the default label
            ProcessUnitNameText.Text = ProcessUnitNameBox.Text = m_pu.Label;

            // Setup the event listener for the comment collection. It is the responsibility of this 
            // control to create and manage the sticky note controls for its comments
            processUnit.Comments.CollectionChanged += CommentCollectionChanged;

            // Setup the event listener for property changes. When things like the location property 
            // change, we'll need to update our position on the canvas.
            processUnit.PropertyChanged += this.ProcessUnit_PropertyChanged;

            // Just a note that we don't need to watch when the incoming or outgoing streams change 
            // because that doesn't affect how we present the process unit in the UI

            // Invoke the comment change event to do the intial creation of comment controls
            CommentCollectionChanged(m_pu.Comments,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void ProcessUnit_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Ignore this event if the flag is set
            if (m_ignoreUnitPropertyChanges)
            {
                return;
            }
            
            switch (e.PropertyName)
            {
                case "Location":
                    this.SetValue(Canvas.LeftProperty, m_pu.Location.X - this.Width / 2.0);
                    this.SetValue(Canvas.TopProperty, m_pu.Location.Y - this.Height / 2.0);

                    // Moving any process unit could cause it to be in a position on the canvas that 
                    // intersects a stream line that it didn't intersect while in its previous position. 
                    // So we'll tell the canvas to update all streams so they can build stream lines that 
                    // move around the process units if need be.
                    // I don't know if this will be bad for performance for really large workspaces, but 
                    // the stream updates are designed to be efficient, so hopefully it won't be a 
                    // problem.
                    m_canvas.UpdateAllStreamLocations();

                    break;

                case "Label":
                    ProcessUnitNameText.Text = ProcessUnitNameBox.Text = m_pu.Label;
                    break;

                case "Subprocess":
                    Color clr;
                    if (Core.App.TryParseColor(m_pu.Subprocess, out clr))
                    {
                        // We need to update the control's background color
                        ProcessUnitBorder.Background = new SolidColorBrush(clr);
                    }
                    break;
            }
        }

        public static ProcessUnitControl CreateOnCanvas(DrawingCanvas canvas,
            AbstractProcessUnit processUnit)
        {
            ProcessUnitControl pu = new ProcessUnitControl(canvas, processUnit);
            canvas.AddNewChild(pu);

            // Set the initial location
            pu.SetValue(Canvas.LeftProperty, processUnit.Location.X - pu.Width / 2.0);
            pu.SetValue(Canvas.TopProperty, processUnit.Location.Y - pu.Height / 2.0);

            // Make sure we have the right z-order
            pu.SetValue(Canvas.ZIndexProperty, 1);

            return pu;
        }

        #region GenericProcessUnit Members

        /// <summary>
        /// Invoked when the comment collection in m_pu changes. This method updates the set of 
        /// sticky note controls on the drawing canvas that are used to represent comments for 
        /// the process unit.
        /// </summary>
        private void CommentCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Start by deleting any sticky note controls that represent comments that are no longer 
            // in the comment collection. While we do this, build a list of all comments that we 
            // have a sticky note control for (used in next step).
            List<Logic.StickyNote> existing = new List<Logic.StickyNote>();
            for (int i = 0; i < m_stickyNotes.Count; i++)
            {
                if (!m_pu.Comments.Contains(m_stickyNotes[i].StickyNote))
                {
                    // Tell the sticky note to remove itself from the drawing canvas
                    m_stickyNotes[i].RemoveSelfFromCanvas(m_canvas);

                    // Remove it from our collection as well and then back up the index
                    m_stickyNotes.RemoveAt(i);
                    i--;
                }
                else
                {
                    existing.Add(m_stickyNotes[i].StickyNote);
                }
            }

            // Now add new sticky note controls for comments that don't have them
            for (int i = 0; i < m_pu.Comments.Count; i++)
            {
                if (!existing.Contains(m_pu.Comments[i]))
                {
                    StickyNoteControl snc = StickyNoteControl.CreateOnCanvas(
                        m_canvas, m_pu.Comments[i], this);
                    m_stickyNotes.Add(snc);
                    snc.UpdateLineToParent();
                }
            }

            // If we have 1 or more comments then we show the comment icon over the process unit, 
            // otherwise we hide it
            if (m_pu.Comments.Count >= 1)
            {
                CommentIcon.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                CommentIcon.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void ProcessUnitNameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessUnitNameBox_LostFocus(this, new RoutedEventArgs());
            }
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
            if (e.Key == Key.Delete || Key.Back == e.Key)
            {
                e.Handled = true;
            }
        }

        private void ProcessUnitNameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // We should only be receiving this when the text box is visible
            if (Visibility.Visible != ProcessUnitNameBox.Visibility)
            {
                return;
            }
            
            ProcessUnitNameText.Visibility = System.Windows.Visibility.Visible;
            ProcessUnitNameBox.Visibility = System.Windows.Visibility.Collapsed;

            // Here's where we finalize the name change, so we need an undo if the name changed
            if (!m_pu.Label.Equals(m_labelOnEditStart))
            {
                if (null == m_labelOnEditStart)
                {
                    Core.App.Log(Core.App.LogItemType.Error,
                        "Labeled process unit tried to create undo but had a null string for its original label");
                }
                else
                {
                    // Set the new text
                    m_ignoreUnitPropertyChanges = true;
                    m_pu.Label = ProcessUnitNameBox.Text;
                    ProcessUnitNameText.Text = ProcessUnitNameBox.Text;
                    m_ignoreUnitPropertyChanges = false;
                    
                    // Add the undo
                    string undoText = "Undo renaming process unit from " + m_labelOnEditStart +
                        " to " + m_pu.Label;
                    m_canvas.GetWorkspace().AddUndo(new UndoRedoCollection(undoText,
                        new PFD.Undos.SetProcessUnitLabel(m_pu, m_labelOnEditStart)));
                }
            }

        }

        void ProcessUnitNameBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        void ProcessUnitNameText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ProcessUnitNameText.Visibility = System.Windows.Visibility.Collapsed;
            ProcessUnitNameBox.Visibility = System.Windows.Visibility.Visible;
            ProcessUnitNameBox.Focus();
            e.Handled = true;

            // This is where we begin the name edit, so we need to store the current name
            m_labelOnEditStart = m_pu.Label;
        }

        public static string GetIconSource(Type processUnitType)
        {
            // Determine the icon from the process unit type    
            if (typeof(HeatExchangerNoUtility) == processUnitType)
            {
                return "/UI/Icons/pu_heat_exchanger_no_utility.png";
            }
            else if (typeof(HeatExchangerWithUtility) == processUnitType)
            {
                return "/UI/Icons/pu_heat_exchanger.png";
            }
            else if (typeof(Mixer) == processUnitType)
            {
                return "/UI/Icons/pu_mixer.png";
            }
            else if (typeof(Reactor) == processUnitType)
            {
                return "/UI/Icons/pu_reactor.png";
            }
            else if (typeof(Logic.Separator) == processUnitType)
            {
                return "/UI/Icons/pu_separator.png";
            }

            // We could have an 'else' here and throw an exception...
            // But we'll just return the generic icon for now
            return "/UI/Icons/pu_generic.png";            
        }

        /// <summary>
        /// Gets the icon dependency property
        /// </summary>
        public virtual Image Icon
        {
            get
            {
                return ProcessUnitImage;
            }
        }

        /// <summary>
        /// Use to reference the border around the process unit's icon.  
        /// </summary>
        public virtual Border IconBorder
        {
            get
            {
                return ProcessUnitBorder;
            }
            set
            {
                ProcessUnitBorder = value;
            }
        }

        /// <summary>
        /// Gets the unique identifier string for the process unit that this control represents
        /// </summary>
        public String Id
        {
            get
            {
                return m_pu.UIDString;
            }
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the processing unit should be available 
        /// with the specified difficulty setting.
        /// TODO: Remove and use logic in AbstractProcessUnit.
        /// </summary>
        public virtual bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty)
        {
            return m_pu.IsAvailableWithDifficulty(difficulty);
        }

        /// <summary>
        /// Gets a reference to the process unit that this control encapsulates
        /// </summary>
        public AbstractProcessUnit ProcessUnit
        {
            get
            {
                return m_pu;
            }
        }

        public int ProcessUnitId
        {
            get
            {
                return m_pu.Id;
            }
        }

        /// <summary>
        /// A short description of the process unit.  Not more than a few words in length.
        /// </summary>
        public string Description
        {
            get
            {
                return m_pu.Description;
            }
        }

        /// <summary>
        /// Total number of incoming streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public virtual int MaxIncomingStreams
        {
            get
            {
                return m_pu.MaxIncomingStreams;
            }            
        }

        /// <summary>
        /// Total number of outgoing streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public virtual int MaxOutgoingStreams
        {
            get
            {
                return m_pu.MaxOutgoingStreams;
            }
        }

        /// <summary>
        /// Total number of incoming heat streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public virtual int MaxIncomingHeatStreams
        {
            get
            {
                return m_pu.MaxIncomingHeatStreams;
            }
        }

        /// <summary>
        /// Total number of outgoing heat streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        public virtual int MaxOutgoingHeatStreams
        {
            get
            {
                return m_pu.MaxOutgoingHeatStreams;
            }
        }

        ///// <summary>
        ///// List of incoming streams
        ///// </summary>
        //public IList<ChemProV.PFD.Streams.AbstractStream> IncomingStreams
        //{
        //    get
        //    {
        //        return incomingStreams;
        //    }
        //}

        ///// <summary>
        ///// List of outgoing streams
        ///// </summary>
        //public IList<ChemProV.PFD.Streams.AbstractStream> OutgoingStreams
        //{
        //    get
        //    {
        //        return outgoingStreams;
        //    }
        //}

        /// <summary>
        /// Gets or sets the selection flag for the stream
        /// </summary>
        public Boolean Selected
        {
            get
            {
                return isSelected;
            }
            set
            {
                bool oldValue = isSelected;
                isSelected = value;

                //either turn the highlight on or off
                if (isSelected)
                {
                    SetBorderColor(ProcessUnitBorderColor.Selected);
                    SelectionChanged(this, new EventArgs());
                }
                else
                {
                    SetBorderColor(ProcessUnitBorderColor.NoBorder);
                }
            }
        }

        /// <summary>
        /// Sets the border around the process unit
        /// </summary>
        /// <param name="brush"></param>
        public void SetBorderColor(ProcessUnitBorderColor borderColor)
        {
            Brush brush;
            if (borderColor == ProcessUnitBorderColor.Selected)
            {
                brush = s_selectedBorderBrush;
            }
            else if (borderColor == ProcessUnitBorderColor.NotAcceptingStreams)
            {
                brush = RedBorderBrush;
            }
            else if (borderColor == ProcessUnitBorderColor.AcceptingStreams)
            {
                brush = GreenBorderBrush;
            }
            else
            {
                brush = TransparentBorderBrush;
            }
            IconBorder.BorderThickness = new Thickness(2);
            IconBorder.BorderBrush = brush;
        }

        /// <summary>
        /// GenericProcessUnit cannot have feedback associated with it so return null.
        /// Must impliment these functions as it is a part of IpfdElement
        /// </summary>
        /// <param name="highlight"></param>
        public void HighlightFeedback(bool highlight)
        {
            return;
        }

        public void SetFeedback(string feedbackMessage, int errorNumber)
        {
            return;
        }

        public void RemoveFeedback()
        {
            return;
        }

        /// <summary>
        /// Gets or sets the subprocess color
        /// </summary>
        public virtual Color Subprocess
        {
            get
            {
                Color clr;
                if (!Core.App.TryParseColor(m_pu.Subprocess, out clr))
                {
                    clr = Colors.White;
                }
                return clr;
            }
            set
            {
                string stringValue = value.ToString();
                
                if (stringValue.Equals(m_pu.Subprocess))
                {
                    // No change
                    return;
                }

                // Update the data structure
                m_ignoreUnitPropertyChanges = true;
                m_pu.Subprocess = stringValue;
                m_ignoreUnitPropertyChanges = false;

                // Parse out the color
                Color clr;
                if (!Core.App.TryParseColor(m_pu.Subprocess, out clr))
                {
                    clr = Colors.White;
                }

                // We need to update the control's background color
                this.Background = new SolidColorBrush(clr);
            }
        }

        #endregion GenericProcessUnit Members

        #region ICanvasElement Members

        public virtual Point Location
        {
            get
            {
                return new Point(m_pu.Location.X, m_pu.Location.Y);
            }
            set
            {
                if (!Location.Equals(value))
                {
                    this.SetValue(Canvas.LeftProperty, value.X - this.Width / 2.0);
                    this.SetValue(Canvas.TopProperty, value.Y - this.Height / 2.0);

                    // Unless we're told not to, update the process unit data structure
                    if (!m_ignoreUnitPropertyChanges)
                    {
                        m_ignoreUnitPropertyChanges = true;
                        m_pu.Location = new MathCore.Vector(value.X, value.Y);
                        m_ignoreUnitPropertyChanges = false;
                    }

                    if (null != LocationChanged)
                    {
                        LocationChanged(this, new EventArgs());
                    }
                }
            }
        }

        #endregion

        public void HideAllComments()
        {
            foreach (StickyNote sn in m_pu.Comments)
            {
                sn.IsVisible = false;
            }
        }

        public void ShowAllComments()
        {
            foreach (StickyNote sn in m_pu.Comments)
            {
                sn.IsVisible = true;
            }
        }

        /// <summary>
        /// Removes this control from the canvas. This includes removal of controls that are 
        /// "attached" to this control, such as the comment sticky note controls.
        /// This is considered "disposing" the control and it should not be used after this call.
        /// </summary>
        public void RemoveSelfFromCanvas(UI.DrawingCanvas owner)
        {
            // Unsubscribe from events
            m_pu.Comments.CollectionChanged -= this.CommentCollectionChanged;
            m_pu.PropertyChanged -= this.ProcessUnit_PropertyChanged;
            
            owner.RemoveChild(this);
            foreach (ChemProV.UI.StickyNoteControl snc in m_stickyNotes)
            {
                snc.RemoveSelfFromCanvas(owner);
            }

            // Set the process unit reference to null
            m_pu = null;
        }

        private void ProcessUnitNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_ignoreUnitPropertyChanges = true;
            m_pu.Label = ProcessUnitNameBox.Text;
            ProcessUnitNameText.Text = ProcessUnitNameBox.Text;
            m_ignoreUnitPropertyChanges = false;
        }
    }
}