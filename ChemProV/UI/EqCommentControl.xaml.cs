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
    public partial class EqCommentControl : UserControl
    {        
        private BasicComment m_basic = null;

        private StickyNote m_sticky = null;
        
        public EqCommentControl()
        {
            InitializeComponent();

            CommentTextBox.Text = string.Empty;
        }

        public EqCommentControl(StickyNote stickyNote)
            : this()
        {
            if (null == stickyNote)
            {
                throw new ArgumentNullException(
                    "StickyNote object cannot be null for an EqCommentControl");
            }
            
            m_sticky = stickyNote;
            
            // We don't offer the option to delete these. That must be done in the 
            // PFD area.
            XLabel.Visibility = System.Windows.Visibility.Collapsed;

            // We also don't allow editing
            CommentTextBox.IsReadOnly = true;

            // Set control text
            UserNameLabel.Content = (null == stickyNote.UserName) ?
                string.Empty : stickyNote.UserName;
            CommentTextBox.Text = stickyNote.Text;

            // Monitor property changes
            stickyNote.PropertyChanged += this.StickyNote_PropertyChanged;
        }

        /// <summary>
        /// Gets or sets the comment object for this control. This can be either a BasicComment object, 
        /// a StickyNote object, or null. This control may subscribe to events for the object, so it 
        /// is best practice to set this to null when the control is no longer being used.
        /// </summary>
        public object CommentObject
        {
            get
            {
                return (null == m_sticky) ? (m_basic as object) : (m_sticky as object);
            }
            set
            {
                if (null != m_sticky)
                {
                    // Remove event handler before changing this value
                    m_sticky.PropertyChanged -= this.StickyNote_PropertyChanged;
                }
                
                m_basic = value as BasicComment;
                m_sticky = value as StickyNote;

                // Update the text in the UI elements
                if (null != m_basic)
                {
                    CommentTextBox.Text = m_basic.CommentText;
                    UserNameLabel.Content = m_basic.CommentUserName;

                    // Allow deletion and editing
                    XLabel.Visibility = System.Windows.Visibility.Visible;
                    CommentTextBox.IsReadOnly = false;
                }
                else if (null != m_sticky)
                {
                    CommentTextBox.Text = m_sticky.Text;
                    UserNameLabel.Content = m_sticky.UserName;

                    // Subsribe to property changes
                    m_sticky.PropertyChanged += this.StickyNote_PropertyChanged;

                    // Don't allow deletion or editing
                    XLabel.Visibility = System.Windows.Visibility.Collapsed;
                    CommentTextBox.IsReadOnly = true;
                }
            }
        }

        private void CommentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (null != m_basic)
            {
                m_basic.CommentText = CommentTextBox.Text;
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
