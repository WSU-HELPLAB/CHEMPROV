/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using ChemProV.UI.DrawingCanvas;
using ChemProV.PFD.Undos;
using ChemProV.PFD.Streams;
using ChemProV.PFD.ProcessUnits;

namespace ChemProV.PFD.StickyNote
{
    public enum StickyNoteColors
    {
        Blue,
        Green,
        Pink,
        Orange,
        Yellow
    }

    public partial class StickyNote : UserControl, IPfdElement, IXmlSerializable, Core.ICanvasElement, Core.IComment
    {
        [Obsolete("Sticky note handles all it's own closing details and other objects shouldn't capture the event")]
        public event MouseButtonEventHandler Closing = delegate { };

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
        private Core.ICommentCollection m_commentParent = null;
        
        private StickyNoteColors color;

        static int i = -1;

        /// <summary>
        /// Default constructor that exists only to make design view work. Must stay private.
        /// </summary>
        private StickyNote()
            : this(null)
        {
        }
        
        public StickyNote(DrawingCanvas canvas)
        {
            InitializeComponent();
            m_canvas = canvas;

            // Set the next color in the cycle
            i++;
            switch (i)
            {
                case 0:
                    ColorChange(StickyNoteColors.Yellow);
                    break;
                case 1:
                    ColorChange(StickyNoteColors.Blue);
                    break;
                case 2:
                    ColorChange(StickyNoteColors.Green);
                    break;
                case 3:
                    ColorChange(StickyNoteColors.Orange);
                    break;
                case 4:
                    ColorChange(StickyNoteColors.Pink);

                    //reset index
                    i = -1;
                    break;

                default:
                    ColorChange(StickyNoteColors.Yellow);
                    break;
            }
        }

        /// <summary>
        /// This is not currently used but must have it since IPfdElement has it
        /// </summary>
        public event EventHandler LocationChanged;

        public string Id
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

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

        private void Bottem_Left_Corner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            // Set the canvas state so that we'll enter resizing mode
            m_canvas.CurrentState = new UI.DrawingCanvas.States.ResizingStickyNote(
                m_canvas, this);
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

            Note.Height = (double)currentSize.Y - (double)Note.GetValue(System.Windows.Controls.Canvas.TopProperty);
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
            Header.Fill = headerBrush;

            this.color = color;
        }

        public StickyNoteColors ColorScheme
        {
            get
            {
                return this.color;
            }
        }

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// This isn't used as the IProcessUnitFactory is responsible for the creation
        /// of new process units.
        /// </summary>
        /// <param name="reader"></param>
        public void ReadXml(XmlReader reader)
        {
        }

        public void WriteXml(XmlWriter writer)
        {
            //the process units location
            writer.WriteStartElement("Location");
            writer.WriteElementString("X", GetValue(Canvas.LeftProperty).ToString());
            writer.WriteElementString("Y", GetValue(Canvas.TopProperty).ToString());
            writer.WriteEndElement();

            //and the color
            writer.WriteStartElement("Color");
            writer.WriteString(this.color.ToString());
            writer.WriteEndElement();

            //and the stickey note's content
            writer.WriteStartElement("Content");
            writer.WriteString(Note.Text);
            writer.WriteEndElement();

            // E.O.
            // Write the size as well
            writer.WriteElementString("Size", string.Format("{0},{1}", Width, Height));
        }

        /// <summary>
        /// Creates a new StickyNote based on the supplied XML element
        /// </summary>
        /// <param name="xmlNote">The xml for a StickyNote</param>
        /// <returns></returns>
        public static StickyNote FromXml(XElement xmlNote, DrawingCanvas canvas)
        {
            StickyNote note = new StickyNote(canvas);

            //pull out content & color
            note.Note.Text = xmlNote.Element("Content").Value;
            note.ColorChange(StickyNoteColorsFromString(xmlNote.Element("Color").Value));

            //use LINQ to find us the X,Y coords
            var location = from c in xmlNote.Elements("Location")
                           select new
                           {
                               x = (string)c.Element("X"),
                               y = (string)c.Element("Y")
                           };
            note.SetValue(Canvas.LeftProperty, Convert.ToDouble(location.ElementAt(0).x));
            note.SetValue(Canvas.TopProperty, Convert.ToDouble(location.ElementAt(0).y));

            // E.O.
            // Load the size information. If it is not present, default to 100x100
            XElement sizeEl = xmlNote.Element("Size");
            if (null == sizeEl)
            {
                note.Width = note.Height = 100.0;
            }
            else
            {
                Point sizeAsPt;
                if (Core.App.TryParsePoint(sizeEl.Value, out sizeAsPt))
                {
                    note.Width = sizeAsPt.X;
                    note.Height = sizeAsPt.Y;
                }
                else
                {
                    note.Width = note.Height = 100.0;
                }
            }

            //return the processed note
            return note;
        }

        #endregion IXmlSerializable Members

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

                SetValue(Canvas.LeftProperty, value.X - Width / 2.0);
                SetValue(Canvas.TopProperty, value.Y - 10.0);

                UpdateLineToParent();
            }
        }

        #endregion

        private void UpdateLineToParent()
        {
            if (null == m_lineToParent)
            {
                return;
            }

            Point location;
            AbstractStream stream = m_commentParent as AbstractStream;
            if (null == stream)
            {
                location = (m_commentParent as IProcessUnit).MidPoint;
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

        public Core.ICommentCollection CommentCollectionParent
        {
            get
            {
                return m_commentParent;
            }
        }

        /// <summary>
        /// Creates a new sticky note to be used as a comment attached to a specific PFD element. The comment is added
        /// to the specified comment-collection-parent and appropriate controls are added to the drawing canvas.
        /// Undo items are created and returned in a list but are not actually added to the drawing canvas via its 
        /// "AddUndo" method. Rather, a list is returned so that if the caller is doing multiple things at once it can 
        /// pack more undo items into a single collection as it sees fit.
        /// </summary>
        public static List<IUndoRedoAction> CreateCommentNote(DrawingCanvas canvas, Core.ICommentCollection parent,
            XElement optionalToLoadFromXML, out StickyNote createdNote)
        {
            if (!(parent is AbstractStream) && !(parent is IProcessUnit))
            {
                throw new InvalidOperationException(
                    "The parent element for a comment-sticky-note must be a stream or process unit");
            }
            
            StickyNote sn;
            if (null == optionalToLoadFromXML)
            {
                sn = new StickyNote(canvas);
                sn.Width = 100.0;
                sn.Height = 100.0;
            }
            else
            {
                sn = FromXml(optionalToLoadFromXML, canvas);
            }
            sn.m_commentParent = parent;
            canvas.AddNewChild(sn);

            // Create the line and add it to the drawing canvas
            sn.m_lineToParent = new Line();
            canvas.AddNewChild(sn.m_lineToParent);
            sn.m_lineToParent.SetValue(Canvas.ZIndexProperty, -3);
            sn.m_lineToParent.Stroke = new SolidColorBrush(Color.FromArgb(255, 245, 222, 179));
            sn.m_lineToParent.StrokeThickness = 1.0;

            // Compute a location if we don't have XML data
            if (null == optionalToLoadFromXML)
            {
                Point location;
                AbstractStream stream = parent as AbstractStream;
                if (null == stream)
                {
                    location = (parent as IProcessUnit).MidPoint;
                }
                else
                {
                    location = stream.StreamLineMidpoint;
                }

                // Compute a location
                double radius = 150.0;
                int count = (parent as Core.ICommentCollection).CommentCount;
                double angle = (double)(count % 6) * 60.0 / 180.0 * Math.PI;
                sn.Location = new Point(
                    location.X + radius * Math.Cos(angle),
                    location.Y + radius * Math.Sin(angle));
            }

            // Make sure that when the parent moves we update the line
            (parent as IPfdElement).LocationChanged +=
                delegate(object sender, EventArgs e)
                {
                    sn.UpdateLineToParent();
                };

            sn.UpdateLineToParent();

            // Set the output value
            createdNote = sn;

            // Add the comment to the collection
            parent.AddComment(sn);

            // Build and return a list of undos that will remove the elements that we added to the 
            // drawing canvas and will remove the comment from the collection
            List<IUndoRedoAction> undos = new List<IUndoRedoAction>();
            undos.Add(new RemoveFromCanvas(sn, canvas));
            undos.Add(new RemoveFromCanvas(sn.LineToParent, canvas));
            undos.Add(new RemoveComment(parent, parent.CommentCount - 1));

            return undos;
        }

        /// <summary>
        /// Deletes this sticky note from the drawing canvas and adds an undo to bring it back. If this 
        /// is a comment sticky note, it will be removed from its parent's comment collection (and the 
        /// undo will be created to handle that too).
        /// </summary>
        public void DeleteWithUndo(DrawingCanvas canvas)
        {
            if (null == m_commentParent)
            {
                // The case is simple when we're not a comment
                canvas.AddUndo(new UndoRedoCollection("Undo deletion of sticky note",
                    new AddToCanvas(this, canvas)));
                canvas.RemoveChild(this);
                return;
            }

            // Find the index of this comment in the parent collection
            Core.ICommentCollection cc = m_commentParent as Core.ICommentCollection;
            int commentIndex = -1;
            for (int i=0; i<cc.CommentCount; i++)
            {
                if (object.ReferenceEquals(this, cc.GetCommentAt(i)))
                {
                    commentIndex = i;
                }
            }

            // Otherwise we have to do the following:
            // 1. Remove sticky note (and add undo item for this)
            // 2. Removing the line that connects it to the parent (undo item too)
            // 3. Remove it from parent collection (undo item too)
            canvas.AddUndo(new UndoRedoCollection("Undo deleting comment",
                new AddToCanvas(this, canvas),
                new AddToCanvas(m_lineToParent, canvas),
                new InsertComment(cc, this, commentIndex)));

            canvas.RemoveChild(this);
            canvas.RemoveChild(m_lineToParent);
            cc.RemoveCommentAt(commentIndex);
        }

        #region IComment Members

        public string CommentText
        {
            get { return Note.Text; }
            set
            {
                Note.Text = value;
            }
        }

        /// <summary>
        /// (might use this in future versions)
        /// </summary>
        public string CommentUserName
        {
            get { return string.Empty; }
        }

        #endregion

        public void Hide()
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
            if (null != m_lineToParent)
            {
                m_lineToParent.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public void Show()
        {
            this.Visibility = System.Windows.Visibility.Visible;
            if (null != m_lineToParent)
            {
                m_lineToParent.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void CollapseLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Hide();
        }
    }
}