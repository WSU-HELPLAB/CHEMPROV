using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Data;

namespace ChemProV.PFD.ProcessUnits
{
    public partial class LabeledProcessUnit : GenericProcessUnit, INotifyPropertyChanged, Core.ICommentCollection
    {
        private string processUnitLabel = "foo";
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        /// E.O.
        /// List of comments used for the implementation of Core.ICommentCollection
        /// </summary>
        private List<Core.Comment> m_comments = new List<Core.Comment>();

        private string m_labelOnEditStart = null;

        /// <summary>
        /// Default constructor - used only for design view
        /// Keep the protection on this private!
        /// </summary>
        private LabeledProcessUnit()
            : base("/UI/Icons/pu_generic.png")
        {

        }
        
        public LabeledProcessUnit(string iconSource)
            : base(iconSource)
        {
            InitializeComponent();
            this.DataContext = this;
            ProcessUnitNameText.MouseLeftButtonDown += new MouseButtonEventHandler(ProcessUnitNameText_MouseLeftButtonDown);
            ProcessUnitNameBox.MouseLeftButtonDown += new MouseButtonEventHandler(ProcessUnitNameBox_MouseLeftButtonDown);
            ProcessUnitNameBox.LostFocus += new RoutedEventHandler(ProcessUnitNameBox_LostFocus);
            ProcessUnitNameBox.KeyDown += new KeyEventHandler(ProcessUnitNameBox_KeyDown);

            // E.O.
            // Create the icon image
            BitmapImage bmp = new BitmapImage();
            bmp.UriSource = new Uri(iconSource, UriKind.Relative);
            ProcessUnitImage.SetValue(Image.SourceProperty, bmp);

            // Set the default label
            ProcessUnitLabel = DefaultLabelPrefix + ProcessUnitId;
        }

        /// <summary>
        /// Should be made abstract eventually (will require refactoring that will also make 
        /// this class abstract)
        /// </summary>
        public virtual string DefaultLabelPrefix
        {
            get
            {
                return "foo";
            }
        }

        /// <summary>
        /// Used to track the process unit's label (name)
        /// </summary>
        public String ProcessUnitLabel
        {
            get
            {
                return processUnitLabel;
            }
            set
            {
                // Don't allow null or empty names
                if (!string.IsNullOrEmpty(value))
                {
                    processUnitLabel = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("ProcessUnitLabel"));
                }
            }
        }

        void ProcessUnitNameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessUnitNameBox_LostFocus(this, new RoutedEventArgs());
            }
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }

        void ProcessUnitNameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ProcessUnitNameText.Visibility = System.Windows.Visibility.Visible;
            ProcessUnitNameBox.Visibility = System.Windows.Visibility.Collapsed;

            // Here's where we finalize the name change, so we need an undo if the name changed
            if (!ProcessUnitLabel.Equals(m_labelOnEditStart))
            {
                if (null == m_labelOnEditStart)
                {
                    Core.App.Log(Core.App.LogItemType.Error,
                        "Labeled process unit tried to create undo but had a null string for its original label");
                }
                else
                {
                    string undoText = "Undo renaming process unit from " + m_labelOnEditStart +
                        " to " + ProcessUnitLabel;
                    Core.App.Workspace.DrawingCanvasReference.AddUndo(
                        new UndoRedoCollection(undoText,
                        new Undos.SetProcessUnitLabel(this, m_labelOnEditStart)));
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
            m_labelOnEditStart = ProcessUnitLabel;
        }

        #region GenericProcessUnitOverrides

        /// <summary>
        /// Gets the icon dependency property
        /// </summary>
        public override Image Icon
        {
            get
            {
                return ProcessUnitImage;
            }
        }

        /// <summary>
        /// Use to reference the border around the process unit's icon.  
        /// </summary>
        public override Border IconBorder
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

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("Name", ProcessUnitLabel);

            // E.O.
            // Write any and all comments
            if (m_comments.Count > 0)
            {
                writer.WriteStartElement("Comments");
                for (int i = 0; i < m_comments.Count; i++)
                {
                    writer.WriteStartElement("Comment");
                    writer.WriteAttributeString("UserName", m_comments[i].UserName);
                    writer.WriteString(m_comments[i].Text);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            // Have the parent class write the rest
            base.WriteXml(writer);
        }

        public override IProcessUnit FromXml(System.Xml.Linq.XElement xpu, IProcessUnit targetUnit)
        {
            (targetUnit as LabeledProcessUnit).ProcessUnitLabel = xpu.Attribute("Name").Value;
            targetUnit = base.FromXml(xpu, targetUnit);
            return targetUnit;
        }

        public override Color Subgroup
        {
            get
            {
                return m_subgroup;
            }
            set
            {
                m_subgroup = value;
                ProcessUnitBorder.Background = new SolidColorBrush(value);
            }
        }

        #endregion

        #region ICommentCollection Members
        
        public bool AddComment(Core.Comment comment)
        {
            // Future versions might have some sort of permissions check here, but for 
            // now we just add it
            m_comments.Add(comment);
            
            // If we have comments then we need to show the comment icon, otherwise we hide it
            if (m_comments.Count > 0)
            {
                CommentIcon.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                CommentIcon.Visibility = System.Windows.Visibility.Collapsed;
            }

            // We also have to take care of the visual aspect of the comment. This means we need a 
            // new comment sticky note on the canvas.
            // TODO: Fix
            throw new NotImplementedException();

            // TODO: CREATE UNDO ACTION!
            
            return true;
        }

        public int CommentCount
        {
            get { return m_comments.Count; }
        }

        public Core.Comment GetCommentAt(int index)
        {
            if (index < 0 || index >= m_comments.Count)
            {
                return null;
            }

            return m_comments[index];
        }

        public bool RemoveCommentAt(int index)
        {
            if (index < 0 || index >= m_comments.Count)
            {
                // Return false if the index is invalid
                return false;
            }
            
            // Future versions might have some sort of permissions check here, but for 
            // now we just remove it
            m_comments.RemoveAt(index);
            return true;
        }

        public bool ReplaceCommentAt(int index, Core.Comment newComment)
        {
            if (index < 0 || index >= m_comments.Count)
            {
                // Return false if the index is invalid
                return false;
            }

            // Future versions might have some sort of permissions check here, but for 
            // now we just replace it
            m_comments[index] = newComment;
            return true;
        }

        #endregion
    }
}
