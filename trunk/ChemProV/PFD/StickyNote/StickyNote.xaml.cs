/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

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

    public partial class StickyNote : UserControl, IPfdElement, IXmlSerializable
    {
        public event EventHandler Resizing = delegate { };
        public event MouseButtonEventHandler Closing = delegate { };

        private StickyNoteColors color;

        static int i = -1;

        public StickyNote(bool isReadOnly)
        {
            InitializeComponent();
            Note.IsReadOnly = isReadOnly;
            LocalInit();
        }

        public StickyNote()
        {
            InitializeComponent();
            LocalInit();
        }

        private void LocalInit()
        {
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
            Resizing(this, EventArgs.Empty);
            e.Handled = true;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Point currentSize = new Point(this.Width, this.Height);

            if (this.Width > 22)
            {
                Header.Width = this.Width - 22;
            }
            else
            {
                this.Width = 22;
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

        private void vertialStackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            this.Width = 100;
            this.Height = 100;
        }

        private void X_Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Closing(this, e);
            e.Handled = true;
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
            Header.Fill = headerBrush;

            this.color = color;
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
        }

        /// <summary>
        /// Creates a new StickyNote based on the supplied XML element
        /// </summary>
        /// <param name="xmlNote">The xml for a StickyNote</param>
        /// <returns></returns>
        public static StickyNote FromXml(XElement xmlNote)
        {
            StickyNote note = new StickyNote();

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

            //return the processed note
            return note;
        }

        #endregion IXmlSerializable Members
    }
}