using System.Windows.Media.Imaging;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ChemProV.Core;
using ChemProV.Logic;

namespace ChemProV.UI
{
    /// <summary>
    /// This control will wrap around either a BasicComment or StickyNote object
    /// </summary>
    public partial class PaneCommentControl : UserControl
    {        
        private BasicComment m_basic = null;

        private StickyNote m_sticky = null;
        
        public PaneCommentControl()
        {
            InitializeComponent();

            CommentTextBox.Text = string.Empty;
        }

        /// <summary>
        /// Gets the comment object for this control. This can be either a BasicComment object, 
        /// a StickyNote object, or null.
        /// </summary>
        public object CommentObject
        {
            get
            {
                return (null == m_sticky) ? (m_basic as object) : (m_sticky as object);
            }
        }

        private void CommentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (null != m_basic)
            {
                m_basic.CommentText = CommentTextBox.Text;
            }
            else if (null != m_sticky)
            {
                m_sticky.Text = CommentTextBox.Text;
            }
        }

        /// <summary>
        /// Sets the comment object for this control to the specified BasicComment object. This control 
        /// may subscribe to events for the object, so it is best practice to set this to null when the 
        /// control is no longer being used.
        /// </summary>
        public void SetCommentObject(BasicComment comment)
        {
            if (null != m_sticky)
            {
                // Remove event handler before changing this value
                m_sticky.PropertyChanged -= this.StickyNote_PropertyChanged;
            }

            m_basic = comment;
            m_sticky = null;

            // It is valid to call this method with a null comment, so only update the UI if we 
            // have a non-null comment.
            if (null != m_basic)
            {
                // Update the text in the UI elements
                CommentTextBox.Text = m_basic.CommentText;
                UserNameLabel.Content = m_basic.CommentUserName;

                // Allow deletion and editing
                XLabel.Visibility = System.Windows.Visibility.Visible;
                CommentTextBox.IsReadOnly = false;

                // Make sure the icon is hidden
                IconImage.Visibility = System.Windows.Visibility.Collapsed;
                TitleBarGrid.ColumnDefinitions[0].Width = new GridLength(0.0);
            }
        }

        public void SetCommentObject(StickyNote comment, object parent)
        {
            if (null != m_sticky)
            {
                // Remove event handler before changing this value
                m_sticky.PropertyChanged -= this.StickyNote_PropertyChanged;
            }

            m_basic = null;
            m_sticky = comment;

            // Update the UI elements if the comment is not null
            if (null != m_sticky)
            {
                CommentTextBox.Text = m_sticky.Text;
                UserNameLabel.Content = m_sticky.UserName;

                // Subsribe to property changes
                m_sticky.PropertyChanged += this.StickyNote_PropertyChanged;

                // Allow editing but not deletion
                CommentTextBox.IsReadOnly = false;
                XLabel.Visibility = System.Windows.Visibility.Collapsed;

                // Show or hide the icon based on the parent
                if (parent is AbstractStream)
                {
                    // Get the right icon for this type of stream
                    string iconSource = PFD.Streams.StreamControl.GetIconSource(parent.GetType());
                    BitmapImage bmp = new BitmapImage();
                    bmp.UriSource = new Uri(iconSource, UriKind.Relative);
                    IconImage.SetValue(Image.SourceProperty, bmp);

                    // Make sure the icon is visible
                    IconImage.Visibility = System.Windows.Visibility.Visible;
                    TitleBarGrid.ColumnDefinitions[0].Width = new GridLength(20.0);
                }
                else if (parent is AbstractProcessUnit)
                {
                    // Get the right icon for this type of process unit
                    string iconSource = ProcessUnitControl.GetIconSource(parent.GetType());
                    BitmapImage bmp = new BitmapImage();
                    bmp.UriSource = new Uri(iconSource, UriKind.Relative);
                    IconImage.SetValue(Image.SourceProperty, bmp);

                    // Make sure the icon is visible
                    IconImage.Visibility = System.Windows.Visibility.Visible;
                    TitleBarGrid.ColumnDefinitions[0].Width = new GridLength(20.0);
                }
                else
                {
                    // Make sure the icon is hidden
                    IconImage.Visibility = System.Windows.Visibility.Collapsed;
                    TitleBarGrid.ColumnDefinitions[0].Width = new GridLength(0.0);
                }
            }
        }

        private void StickyNote_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Text"))
            {
                CommentTextBox.Text = m_sticky.Text;
            }
            else if (e.PropertyName.Equals("UserName"))
            {
                UserNameLabel.Content = m_sticky.Text;
            }
        }
    }
}
