/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ChemProV.Core;
using ChemProV.UI;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Undos;
using System.ComponentModel;
using ChemProV.Logic;

namespace ChemProV.UI
{
    public enum StickyNoteColors
    {
        Blue,
        Green,
        Pink,
        Orange,
        Yellow
    }

    public partial class StickyNoteControl : UserControl, PFD.IPfdElement, Core.ICanvasElement
    {
        private DrawingCanvas m_canvas = null;

        /// <summary>
        /// Sticky notes can be used as comments that are tied to specific elements in the process flow 
        /// diagram. When the are in this mode, this will be the line that connects them to their parent 
        /// element in the interface.
        /// If the note is just free-floating and not connected to anything, then this will be null.
        /// </summary>
        private Line m_lineToParent = null;

        /// <summary>
        /// Reference to the parent comment collection if this is a comment-sticky-note, null otherwise.
        /// </summary>
        private object m_commentParent = null;

        private StickyNote m_note;
        
        private StickyNoteColors color;

        private static int i = -1;

        /// <summary>
        /// Default constructor that exists only to make design view work. Must stay private.
        /// </summary>
        private StickyNoteControl()
            : this(null, null)
        {
        }

        private StickyNoteControl(DrawingCanvas canvas, StickyNote memNote)
        {
            InitializeComponent();
            m_canvas = canvas;
            m_note = memNote;

            // Ensure that we always default to yellow
            ColorChange(StickyNoteColors.Yellow);

            // Set properties
            if (null != memNote)
            {
                this.Height = memNote.Height;
                Note.Text = (null == memNote.Text) ? string.Empty : memNote.Text;
                Header.Text = (null == m_note.UserName) ? string.Empty : m_note.UserName;
                this.Width = memNote.Width;
                this.SetValue(Canvas.LeftProperty, memNote.LocationX);
                this.SetValue(Canvas.TopProperty, memNote.LocationY);

                // Monitor changes in the memNote object
                memNote.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(MemNote_PropertyChanged);
            }
        }

        /// <summary>
        /// Callback for changes in the data structure that this control wraps around. We have to update UI 
        /// elements appropriately based on changes to the data.
        /// </summary>
        private void MemNote_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Height":
                    Height = m_note.Height;
                    break;

                case "IsVisible":
                    if (m_note.IsVisible)
                    {
                        this.Visibility = System.Windows.Visibility.Visible;
                        if (null != m_lineToParent)
                        {
                            m_lineToParent.Visibility = System.Windows.Visibility.Visible;
                        }
                    }
                    else
                    {
                        this.Visibility = System.Windows.Visibility.Collapsed;
                        if (null != m_lineToParent)
                        {
                            m_lineToParent.Visibility = System.Windows.Visibility.Collapsed;
                        }
                    }
                    break;
                
                case "LocationX":
                    SetValue(Canvas.LeftProperty, m_note.LocationX);
                    break;

                case "LocationY":
                    SetValue(Canvas.TopProperty, m_note.LocationY);
                    break;
                
                case "Text":
                    Note.Text = m_note.Text;
                    break;
                
                case "UserName":
                    Header.Text = (null == m_note.UserName) ? string.Empty : m_note.UserName;
                    break;

                case "Width":
                    Width = m_note.Width;
                    break;
            }
        }

        public static StickyNoteControl CreateOnCanvas(DrawingCanvas canvas,
            StickyNote memNote, object commentParentControl)
        {
            // I'm using a static constructor because this way I can at least choose 
            // wording that implies that this control will be created AND added to 
            // the drawing canvas.

            // Quick parameter check
            if (null == canvas || null == memNote)
            {
                throw new ArgumentNullException();
            }

            StickyNoteControl snc = new StickyNoteControl(canvas, memNote);
            snc.m_commentParent = commentParentControl;
            canvas.AddNewChild(snc);

            // Give it a high z-index since we want comments above everything else
            snc.SetValue(Canvas.ZIndexProperty, (int)4);

            // Setup extra stuff if we have a parent control
            if (null != commentParentControl)
            {
                if (!(commentParentControl is ChemProV.PFD.Streams.StreamControl) && 
                    !(commentParentControl is ProcessUnitControl))
                {
                    throw new InvalidOperationException(
                        "The parent element for a comment-sticky-note must be a stream or process unit");
                }

                // Create the line and add it to the drawing canvas
                snc.m_lineToParent = new Line();
                canvas.AddNewChild(snc.m_lineToParent);
                snc.m_lineToParent.SetValue(Canvas.ZIndexProperty, -3);
                snc.m_lineToParent.Stroke = new SolidColorBrush(Color.FromArgb(255, 245, 222, 179));
                snc.m_lineToParent.StrokeThickness = 1.0;

                // Make sure that when the parent moves we update the line
                if (commentParentControl is ProcessUnitControl)
                {
                    (commentParentControl as ProcessUnitControl).ProcessUnit.PropertyChanged +=
                        snc.ProcessUnitParentPropertyChanged;
                }
                else
                {
                    (commentParentControl as PFD.Streams.StreamControl).Stream.PropertyChanged +=
                        snc.StreamParentPropertyChanged;
                }

                // Position the line to the parent
                snc.UpdateLineToParent();
            }

            // Hide if need be
            if (!memNote.IsVisible)
            {
                snc.Hide();
            }

            return snc;
        }

        private void ProcessUnitParentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Location"))
            {
                UpdateLineToParent();
            }
        }

        private void StreamParentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Source") ||
                e.PropertyName.Equals("Destination") ||
                e.PropertyName.Equals("SourceLocation") || 
                e.PropertyName.Equals("DestinationLocation"))
            {
                UpdateLineToParent();
            }
        }

        /// <summary>
        /// Only here because IPFDElement requires it
        /// </summary>
        public String Id
        {
            get;
            set;
        }

        /// <summary>
        /// This is not currently used but must have it since IPfdElement has it
        /// TODO: Get rid of it. All location monitoring should happen through the 
        /// workspace objects.
        /// </summary>
        public event EventHandler LocationChanged;

        public void HighlightFeedback(bool highlight)
        {
        }

        public void SetFeedback(string feedbackMessage, int errorNumber)
        {
        }

        public void RemoveFeedback()
        {
        }

        private bool selected;

        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
            }
        }

        /// <summary>
        /// This is not currently used but must have it since IPfdElement has it
        /// </summary>
        public event EventHandler SelectionChanged;

        private void Bottom_Left_Corner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            // Set the canvas state so that we'll enter resizing mode
            m_canvas.CurrentState = new UI.DrawingCanvasStates.ResizingStickyNote(
                m_canvas, m_note);
            // Kick it off by sending the mouse-down event
            m_canvas.CurrentState.MouseLeftButtonDown(sender, e);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Point currentSize = new Point(this.Width, this.Height);

            if (this.Width > 36)
            {
                Header.Width = this.Width - 36;
            }
            else
            {
                this.Width = 36;
            }

            if (this.Height < 23 + 7)
            {
                this.Height = 23 + 7;
            }

            //Note.Height = (double)currentSize.Y - (double)Note.GetValue(System.Windows.Controls.Canvas.TopProperty);
            Note.Height = this.Height - 23.0;
            Note.Width = (double)currentSize.X - (double)Note.GetValue(System.Windows.Controls.Canvas.LeftProperty);
            try
            {
                Thickness thick = new Thickness(currentSize.X - 7, currentSize.Y - 7, 0, 0);

                Bottem_Left_Corner.Margin = thick;
            }
            catch
            {
            }
        }

        private void X_Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Make sure we mark this mouse event as handled
            e.Handled = true;

            // We have a special function to delete
            DeleteWithUndo(m_canvas);
        }

        public static StickyNoteColors StickyNoteColorsFromString(string colorString)
        {
            StickyNoteColors color;
            switch (colorString)
            {
                case "Blue":
                    color = StickyNoteColors.Blue;
                    break;

                case "Pink":
                    color = StickyNoteColors.Pink;
                    break;

                case "Yellow":
                    color = StickyNoteColors.Yellow;
                    break;

                case "Green":
                    color = StickyNoteColors.Green;
                    break;

                case "Orange":
                    color = StickyNoteColors.Orange;
                    break;

                default:
                    color = StickyNoteColors.Yellow;
                    break;
            }
            return color;
        }

        public void ColorChange(StickyNoteColors color)
        {
            SolidColorBrush headerBrush;
            SolidColorBrush bodyBrush;
            switch (color)
            {
                case StickyNoteColors.Blue:
                    headerBrush = new SolidColorBrush(Color.FromArgb(100, 154, 221, 247));
                    bodyBrush = new SolidColorBrush(Color.FromArgb(100, 200, 236, 250));
                    break;
                case StickyNoteColors.Pink:
                    headerBrush = new SolidColorBrush(Color.FromArgb(255, 220, 149, 222));
                    bodyBrush = new SolidColorBrush(Color.FromArgb(255, 241, 195, 241));
                    break;
                case StickyNoteColors.Green:
                    headerBrush = new SolidColorBrush(Color.FromArgb(255, 116, 226, 131));
                    bodyBrush = new SolidColorBrush(Color.FromArgb(255, 160, 224, 169));
                    break;
                case StickyNoteColors.Orange:
                    headerBrush = new SolidColorBrush(Color.FromArgb(255, 243, 134, 57));
                    bodyBrush = new SolidColorBrush(Color.FromArgb(255, 255, 177, 122));
                    break;
                case StickyNoteColors.Yellow:
                    headerBrush = new SolidColorBrush(Color.FromArgb(255, 212, 204, 117));
                    bodyBrush = new SolidColorBrush(Color.FromArgb(255, 255, 252, 163));
                    break;
                default:
                    headerBrush = new SolidColorBrush(Color.FromArgb(255, 212, 204, 117));
                    bodyBrush = new SolidColorBrush(Color.FromArgb(255, 255, 252, 163));
                    break;
            }
            vertialStackPanel.Background = bodyBrush;
            Header_StackPanel.Background = headerBrush;
            X_Label.Background = headerBrush;
            CollapseLabel.Background = headerBrush;
            //Header.Background = headerBrush;

            this.color = color;
        }

        public StickyNoteColors ColorScheme
        {
            get
            {
                return this.color;
            }
        }

        /// <summary>
        /// Gets a reference to the StickyNote object that this control represents
        /// </summary>
        public StickyNote StickyNote
        {
            get
            {
                return m_note;
            }
        }

        #region ICanvasElement Members

        /// <summary>
        /// Gets or sets the location of the sticky note on the canvas. The location for sticky 
        /// notes is in the center of the note horizontally, but 10 pixels down from the top 
        /// edge vertically.
        /// </summary>
        public Point Location
        {
            get
            {
                return new Point(
                    (double)GetValue(Canvas.LeftProperty) + Width / 2.0,
                    (double)GetValue(Canvas.TopProperty) + 10.0);
            }
            set
            {
                if (Location.Equals(value))
                {
                    // No change, so nothing to do
                    return;
                }

                // Stop listening while we change the data
                m_note.PropertyChanged -= this.MemNote_PropertyChanged;
                
                double left = value.X - Width / 2.0;
                double top = value.Y - 10.0;
                SetValue(Canvas.LeftProperty, left);
                SetValue(Canvas.TopProperty, top);
                m_note.LocationX = left;
                m_note.LocationY = top;

                // Start listening again
                m_note.PropertyChanged += this.MemNote_PropertyChanged;

                UpdateLineToParent();
            }
        }

        #endregion

        public void UpdateLineToParent()
        {
            if (null == m_lineToParent || null == m_commentParent)
            {
                return;
            }

            Point location;
            StreamControl stream = m_commentParent as StreamControl;
            if (null == stream)
            {
                location = (m_commentParent as ProcessUnitControl).Location;
            }
            else
            {
                location = stream.StreamLineMidpoint;
            }

            m_lineToParent.X1 = Location.X;
            m_lineToParent.Y1 = Location.Y;
            m_lineToParent.X2 = location.X;
            m_lineToParent.Y2 = location.Y;
        }

        /// <summary>
        /// Sticky notes can be used as comments that are tied to specific elements in the process flow 
        /// diagram. When the are in this mode, this will be the line that connects them to their parent 
        /// element in the interface.
        /// If the note is just free-floating and not connected to anything, then this will be null.
        /// </summary>
        public Line LineToParent
        {
            get
            {
                return m_lineToParent;
            }
        }

        public bool HasCommentCollectionParent
        {
            get
            {
                return null != m_commentParent;
            }
        }

        public static MathCore.Vector ComputeNewCommentNoteLocation(DrawingCanvas canvas, object parentControl,
            double controlWidth = 100.0, double controlHeight = 100.0)
        {
            // First resolve the "center point" of the parent object. Also get a reference to the collection 
            // of comments.
            Point location;
            IList<StickyNote> comments;
            ProcessUnitControl lpu = parentControl as ProcessUnitControl;
            PFD.Streams.StreamControl stream = parentControl as PFD.Streams.StreamControl;
            if (null != lpu)
            {
                location = lpu.Location;
                comments = lpu.ProcessUnit.Comments;
            }
            else if (null != stream)
            {
                location = stream.StreamLineMidpoint;
                comments = stream.Stream.Comments;
            }
            else
            {
                throw new ArgumentException(
                    "Parent control for a comment sticky note must be a stream or process unit " +
                    "control.\n Method: ComputeNewCommentNoteLocation");
            }

            // Get a reference to the workspace. We will look at other sticky notes in this workspace to 
            // try to avoid direct overlap.
            Workspace ws = canvas.GetWorkspace();

            MathCore.Vector loc;
            int attempts = 0;
            while (true)
            {
                // Compute a location
                double radius = 150.0;
                double angle = (double)(comments.Count % 6) * 60.0 / 180.0 * Math.PI;
                loc = new MathCore.Vector(
                    location.X + radius * Math.Cos(angle),
                    location.Y + radius * Math.Sin(angle));

                // Make sure this location wouldn't make the control go off the canvas
                if (loc.X - (controlWidth / 2.0) < 0.0 ||
                    loc.Y - (controlHeight / 2.0) < 0.0)
                {
                    attempts++;
                }
                else if ((null != stream && stream.Stream.ContainsCommentWithLocation(loc.X, loc.Y)) ||
                    (null != lpu && lpu.ProcessUnit.ContainsCommentWithLocation(loc.X, loc.Y)))
                {
                    attempts++;
                }
                else
                {
                    // This means the location is ok and we can return it
                    return loc;
                }

                // Try cascading if radial position failed
                if (attempts > 6)
                {
                    // Reset attempts because we're about to try another method of positioning
                    attempts = 0;

                    double offset = 10.0;
                    while (true)
                    {
                        loc.X = location.X + radius + offset;
                        loc.Y = location.Y + offset;

                        // Make sure this location wouldn't make the control go off the canvas
                        if (loc.X - (controlWidth / 2.0) < 0.0 ||
                            loc.Y - (controlHeight / 2.0) < 0.0)
                        {
                            attempts++;
                        }
                        else if ((null != stream && stream.Stream.ContainsCommentWithLocation(loc.X, loc.Y)) ||
                            (null != lpu && lpu.ProcessUnit.ContainsCommentWithLocation(loc.X, loc.Y)))
                        {
                            attempts++;
                        }
                        else
                        {
                            // This means the location is ok and we can return it
                            return loc;
                        }

                        attempts++;
                        if (attempts > 50)
                        {
                            // Just give up and choose an arbitrary position
                            return new MathCore.Vector(location.X + radius, location.Y);
                        }

                        // Increase the offset for the next attempt
                        offset += 10.0;
                    }
                }
            }
        }

        ///// <summary>
        ///// Creates a new sticky note to be used as a comment attached to a specific PFD element. The comment is added
        ///// to the specified comment-collection-parent and appropriate controls are added to the drawing canvas.
        ///// Undo items are created and returned in a list but are not actually added to the drawing canvas via its 
        ///// "AddUndo" method. Rather, a list is returned so that if the caller is doing multiple things at once it can 
        ///// pack more undo items into a single collection as it sees fit.
        ///// </summary>
        //[Obsolete("Removing this, do not use. Keeping for positioning algorithm reference.")]
        //public static List<IUndoRedoAction> CreateCommentNote(DrawingCanvas canvas, object parent,
        //    XElement optionalToLoadFromXML, out StickyNoteControl createdNote)
        //{
        //    if (!(parent is AbstractStream) && !(parent is GenericProcessUnit))
        //    {
        //        throw new InvalidOperationException(
        //            "The parent element for a comment-sticky-note must be a stream or process unit");
        //    }
            
        //    StickyNoteControl sn;
        //    if (null == optionalToLoadFromXML)
        //    {
        //        sn = new StickyNoteControl(canvas);
        //        sn.Width = 100.0;
        //        sn.Height = 100.0;
        //    }
        //    else
        //    {
        //        sn = new StickyNoteControl(optionalToLoadFromXML, canvas);
        //    }
        //    sn.m_commentParent = parent;
        //    canvas.AddNewChild(sn);

        //    // Give it a high z-index since we want comments above everything else
        //    sn.SetValue(Canvas.ZIndexProperty, (int)4);

        //    // Create the line and add it to the drawing canvas
        //    sn.m_lineToParent = new Line();
        //    canvas.AddNewChild(sn.m_lineToParent);
        //    sn.m_lineToParent.SetValue(Canvas.ZIndexProperty, -3);
        //    sn.m_lineToParent.Stroke = new SolidColorBrush(Color.FromArgb(255, 245, 222, 179));
        //    sn.m_lineToParent.StrokeThickness = 1.0;

        //    // Compute a location if we don't have XML data
        //    if (null == optionalToLoadFromXML)
        //    {
                
        //    }

        //    // Make sure that when the parent moves we update the line
        //    (parent as IPfdElement).LocationChanged +=
        //        delegate(object sender, EventArgs e)
        //        {
        //            sn.UpdateLineToParent();
        //        };

        //    sn.UpdateLineToParent();

        //    // Set the output value
        //    createdNote = sn;

        //    // Add the comment to the collection
        //    parent.AddComment(sn);

        //    // Build and return a list of undos that will remove the elements that we added to the 
        //    // drawing canvas and will remove the comment from the collection
        //    List<IUndoRedoAction> undos = new List<IUndoRedoAction>();
        //    undos.Add(new RemoveFromCanvas(sn, canvas));
        //    undos.Add(new RemoveFromCanvas(sn.LineToParent, canvas));
        //    undos.Add(new RemoveComment(parent, parent.CommentCount - 1));

        //    return undos;
        //}

        /// <summary>
        /// Deletes this sticky note from the workspace and adds an undo to bring it back.
        /// </summary>
        public void DeleteWithUndo(DrawingCanvas canvas)
        {
            // Get a reference to the workspace
            Workspace ws = canvas.GetWorkspace();

            // Start by unsubscribing from events
            if (null != m_commentParent)
            {
                if (m_commentParent is ProcessUnitControl)
                {
                    (m_commentParent as ProcessUnitControl).ProcessUnit.PropertyChanged -=
                        ProcessUnitParentPropertyChanged;
                }
                else
                {
                    (m_commentParent as PFD.Streams.StreamControl).Stream.PropertyChanged -=
                        StreamParentPropertyChanged;
                }
            }
            
            // Get a reference to the relevant comment collection
            IList<StickyNote> comments;
            if (null == m_commentParent)
            {
                comments = ws.StickyNotes;
            }
            else if (m_commentParent is ProcessUnitControl)
            {
                comments = (m_commentParent as ProcessUnitControl).ProcessUnit.Comments;
            }
            else
            {
                comments = (m_commentParent as ChemProV.PFD.Streams.StreamControl).Stream.Comments;
            }

            // Find the index of this comment in the parent collection
            int commentIndex = -1;
            for (int i = 0; i < comments.Count; i++)
            {
                if (object.ReferenceEquals(m_note, comments[i]))
                {
                    commentIndex = i;
                    break;
                }
            }

            // This really should never occur, but if we didn't find the comment in the collection 
            // then this implies that this control shouldn't be on the canvas anyway, so remove it.
            if (-1 == commentIndex)
            {
                canvas.RemoveChild(this);
                canvas.RemoveChild(m_lineToParent);
                return;
            }

            // Add the undo first
            ws.AddUndo(new UndoRedoCollection(
                    "Undo deleting comment", new InsertComment(comments, m_note, commentIndex)));

            // Remove the comment from the collection. Event handlers will update the UI and remove 
            // this control (and the line to the parent control) from the drawing canvas.
            comments.RemoveAt(commentIndex);
        }

        public void Hide()
        {
            // Set the visibility in the data structure to false. Event handlers will respond 
            // and do the actual hiding of the control
            m_note.IsVisible = false;
        }

        public void Show()
        {
            // Set the visibility in the data structure to true. Event handlers will respond 
            // and do the actual showing of the control
            m_note.IsVisible = true;
        }

        private void CollapseLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            
            // Set the visibility to false in the data object. Event handlers will fire in 
            // response and do the actual visibility change for the control (and the line 
            // to the parent object if applicable)
            m_note.IsVisible = false;
        }

        public static StickyNoteColors GetNextUserStickyColor()
        {
            i++;
            switch (i)
            {
                case 0:
                    return StickyNoteColors.Blue;

                case 1:
                    return StickyNoteColors.Green;

                case 2:
                    return StickyNoteColors.Orange;

                case 3:
                    //reset index
                    i = -1;

                    return StickyNoteColors.Pink;

                default:
                    return StickyNoteColors.Yellow;
            }
        }

        private bool IsOffCanvas
        {
            get
            {
                double left = (double)GetValue(Canvas.LeftProperty);
                double top = (double)GetValue(Canvas.TopProperty);
                return (left < 0.0 || top < 0.0);

                // TODO: Checks on the right and bottom edges with respect to the drawing canvas
            }
        }

        /// <summary>
        /// Goes through all controls on the drawing canvas and returns true if any one is 
        /// a sticky note control with the exact specified location.
        /// </summary>
        private static bool ContainsCommentSNAt(DrawingCanvas canvas, Point location)
        {
            for (int i = 0; i < canvas.Children.Count; i++)
            {
                StickyNoteControl sn = canvas.Children[i] as StickyNoteControl;
                if (null != sn)
                {
                    if (sn.Location.Equals(location))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void Note_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_note.Text = Note.Text;
        }
    }
}