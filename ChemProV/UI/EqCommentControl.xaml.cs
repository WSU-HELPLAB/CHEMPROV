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

namespace ChemProV.UI
{
    public partial class EqCommentControl : UserControl
    {
        private BasicComment m_cmt = null;
        
        public EqCommentControl()
        {
            InitializeComponent();
        }

        public BasicComment CommentObject
        {
            get
            {
                return m_cmt;
            }
            set
            {
                m_cmt = null;
                if (null != value)
                {
                    // Update the text in the UI elements
                    CommentTextBox.Text = value.CommentText;
                    UserNameLabel.Content = value.CommentUserName;
                }
                m_cmt = value;
            }
        }

        private void CommentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (null != m_cmt)
            {
                m_cmt.CommentText = CommentTextBox.Text;
            }
        }
    }
}
