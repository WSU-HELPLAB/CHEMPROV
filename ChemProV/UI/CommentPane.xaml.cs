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
using ChemProV.PFD.EquationEditor;

namespace ChemProV.UI
{
    public partial class CommentPane : UserControl
    {
        public CommentPane()
        {
            InitializeComponent();
        }

        private void AddCommentButtonClick(object sender, RoutedEventArgs e)
        {
            EquationControl ec = (sender as Button).Tag as EquationControl;
            ec.Model.Comments.Add(new BasicComment(string.Empty, null));
            UpdateComments(Core.App.Workspace.EquationEditor, null);
        }

        public void UpdateComments(EquationEditor editor, EquationControl focusRow)
        {
            // Clear first
            CommentsStack.Children.Clear();

            for (int i = 0; i < editor.EquationRowCount; i++)
            {
                EquationControl ec = editor.GetRowControl(i);
                if (ec.CommentsVisible && ec.Model.Comments.Count > 0)
                {
                    Border brdr = new Border();
                    brdr.Margin = new Thickness(3.0, 3.0, 3.0, 0.0);
                    brdr.CornerRadius = new CornerRadius(3.0);
                    brdr.BorderThickness = new Thickness(2.0);
                    brdr.BorderBrush = new SolidColorBrush(NamedColors.CommentKeys[i].Color);
                    StackPanel sp = new StackPanel();
                    sp.Orientation = Orientation.Vertical;
                    brdr.Child = sp;

                    // Add each comment (as a label for testing purposes)
                    foreach (BasicComment bc in ec.Model.Comments)
                    {
                        EqCommentControl cc = new EqCommentControl();
                        cc.CommentTextBox.Text = bc.CommentText;
                        cc.Margin = new Thickness(3.0);
                        cc.CommentTextBox.TextChanged += delegate(object sender, TextChangedEventArgs e)
                        {
                            bc.CommentText = cc.CommentTextBox.Text;
                        };
                        sp.Children.Add(cc);
                    }

                    // Add a button to allow addition of more comments
                    Button btn = new Button();
                    btn.Margin = new Thickness(3.0);
                    Image btnIcon = Core.App.CreateImageFromSource("plus_16x16.png");
                    btnIcon.Width = btnIcon.Height = 16;
                    btn.Content = btnIcon;
                    btn.Tag = ec;
                    btn.Click += new RoutedEventHandler(AddCommentButtonClick);
                    sp.Children.Add(btn);

                    CommentsStack.Children.Add(brdr);
                }
            }
        }
    }
}
