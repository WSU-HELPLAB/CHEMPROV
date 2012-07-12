/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace ChemProV.Logic
{
    /// <summary>
    /// This class exists to provide a UI-independent sticky note entity. This is important because 
    /// things like ASP.NET pages, WinForms apps, etc. cannot load Silverlight controls or reference 
    /// Silverlight class libraries. So if we have all the logic for PFD elements within Silverlight 
    /// controls then we can't use it in non-Silverlight apps.
    /// This class can load/save/store all relevant data for a sticky note without any Silverlight-
    /// specific dependencies.
    /// </summary>
    public class StickyNote : INotifyPropertyChanged
    {
        /// <summary>
        /// Currently used for runtime convenience only, not saved to /loaded from files
        /// </summary>
        private bool m_isVisible = true;
        
        private double m_locX = 0.0;

        private double m_locY = 0.0;

        private string m_parentId = null;

        private double m_sizeH = 100.0;

        private double m_sizeW = 100.0;

        private string m_text = string.Empty;

        private string m_userName = null;

        public StickyNote() { }

        /// <summary>
        /// Initializes from a &lt;StickyNote&gt; or &lt;Comment&gt; XElement with an optional ideintifier 
        /// for a parent object.
        /// </summary>
        public StickyNote(XElement stickyNoteElement, string optionalParentId)
        {
            // Make sure we have the correct type of XElement
            string elNameLwr = stickyNoteElement.Name.LocalName.ToLower();
            if (!elNameLwr.Equals("stickynote") && !elNameLwr.Equals("comment"))
            {
                throw new ArgumentException("XElement for a sticky note must be either a " +
                    "<Comment> or <StickyNote> element. Element was named: " + elNameLwr);
            }
            
            m_parentId = optionalParentId;
            m_text = stickyNoteElement.Element("Content").Value;

            //use LINQ to find us the X,Y coords
            var location = from c in stickyNoteElement.Elements("Location")
                           select new
                           {
                               x = (string)c.Element("X"),
                               y = (string)c.Element("Y")
                           };
            m_locX = Convert.ToDouble(location.ElementAt(0).x);
            m_locY = Convert.ToDouble(location.ElementAt(0).y);

            // E.O.
            // Load the size information. If it is not present, default to 100x100
            XElement sizeEl = stickyNoteElement.Element("Size");
            if (null == sizeEl)
            {
                m_sizeH = m_sizeW = 100.0;
            }
            else
            {
                if (!TryParseXY(sizeEl.Value, out m_sizeW, out m_sizeH))
                {
                    m_sizeH = m_sizeW = 100.0;
                }
            }

            // Look for a user name
            XElement userEl = stickyNoteElement.Element("UserName");
            if (null != userEl)
            {
                m_userName = userEl.Value;
            }
        }

        public double Height
        {
            get
            {
                return m_sizeH;
            }
            set
            {
                if (m_sizeH == value)
                {
                    // No change
                    return;
                }

                m_sizeH = value;
                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Height"));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the sticky note should be visible 
        /// in the UI. This value is not saved to files and implicitly isn't loaded from them 
        /// either.
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return m_isVisible;
            }
            set
            {
                if (m_isVisible == value)
                {
                    // No change
                    return;
                }

                m_isVisible = value;
                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsVisible"));
                }
            }
        }

        public double LocationX
        {
            get
            {
                return m_locX;
            }
            set
            {
                if (m_locX == value)
                {
                    // No change
                    return;
                }

                m_locX = value;
                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("LocationX"));
                }
            }
        }

        public double LocationY
        {
            get
            {
                return m_locY;
            }
            set
            {
                if (m_locY == value)
                {
                    // No change
                    return;
                }

                m_locY = value;
                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("LocationY"));
                }
            }
        }

        public string ParentId
        {
            get
            {
                return m_parentId;
            }
        }

        public string Text
        {
            get
            {
                return m_text;
            }
            set
            {
                if (m_text == value)
                {
                    // No change
                    return;
                }

                m_text = string.IsNullOrEmpty(value) ? string.Empty : value;
                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Text"));
                }
            }
        }

        /// <summary>
        /// Attempts to parse a string of the form "X,Y". On failure, the values are set 
        /// to 0.0 and false is returned.
        /// </summary>
        private static bool TryParseXY(string pointString, out double x, out double y)
        {
            // The expected format of the string is "X,Y"
            if (null == pointString || !pointString.Contains(","))
            {
                x = y = 0.0;
                return false;
            }

            string[] components = pointString.Split(',');
            if (null == components || components.Length < 2)
            {
                x = y = 0.0;
                return false;
            }

            if (double.TryParse(components[0], out x) && double.TryParse(components[1], out y))
            {
                return true;
            }

            x = y = 0.0;
            return false;
        }

        public string UserName
        {
            get
            {
                return m_userName;
            }
            set
            {
                if (m_userName == value)
                {
                    // No change
                    return;
                }
                
                m_userName = value;
                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("UserName"));
                }
            }
        }

        public double Width
        {
            get
            {
                return m_sizeW;
            }
            set
            {
                if (m_sizeW == value)
                {
                    // No change
                    return;
                }

                m_sizeW = value;
                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Width"));
                }
            }
        }

        /// <summary>
        /// Writes this sticky note's XML as a child under the specified parent
        /// </summary>
        public void WriteElement(XElement parent)
        {
            XElement tree = new XElement("Comment",
                new XElement("Location", 
                    new XElement("X", m_locX),
                    new XElement("Y", m_locY)),
                new XElement("Content", m_text),
                new XElement("Size", string.Format("{0},{1}", Width, Height)));

            // Write the user name if we have one
            if (!string.IsNullOrEmpty(m_userName))
            {
                tree.Add(new XElement("UserName", m_userName));
            }

            parent.Add(tree);
        }


        public void WriteXml(XmlWriter writer)
        {
            // Location
            writer.WriteStartElement("Location");
            writer.WriteElementString("X", m_locX.ToString());
            writer.WriteElementString("Y", m_locY.ToString());
            writer.WriteEndElement();

            // Write the content
            writer.WriteStartElement("Content");
            writer.WriteString(m_text);
            writer.WriteEndElement();

            // Write the size as well
            writer.WriteElementString("Size", string.Format("{0},{1}", Width, Height));

            // Write the user name if we have one
            if (!string.IsNullOrEmpty(m_userName))
            {
                writer.WriteElementString("UserName", m_userName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = null;
    }
}
