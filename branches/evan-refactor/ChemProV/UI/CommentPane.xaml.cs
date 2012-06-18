using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChemProV.Core;
using ChemProV.PFD.EquationEditor.Models;

namespace ChemProV.UI
{
    /// <summary>
    /// Control to show comments for a workspace. Currently only supports equation comments and 
    /// comments for the degrees of freedom analysis.
    /// </summary>
    public partial class CommentPane : UserControl
    {
        private Workspace m_workspace = null;

        /// <summary>
        /// Brush for the border around degrees of freedom analysis comments
        /// </summary>
        private static Brush s_dfBorderBrush = new SolidColorBrush(Color.FromArgb(255, 240, 42, 176));
        
        /// <summary>
        /// Brush for the border around equation comments
        /// </summary>
        private static Brush s_eqBorderBrush = new SolidColorBrush(Color.FromArgb(255,42,176,240));
        
        public CommentPane()
        {
            InitializeComponent();

            // Clear anything that was put in as hints at design time
            CommentsStack.Children.Clear();
        }

        private void AddCommentButtonClick(object sender, RoutedEventArgs e)
        {
            EquationModel model = (sender as Button).Tag as EquationModel;
            model.Comments.Add(new BasicComment(string.Empty, null));
            UpdateComments();
        }

        private void AddDFCommentButtonClick(object sender, RoutedEventArgs e)
        {
            m_workspace.DegreesOfFreedomAnalysis.Comments.Add(new BasicComment());
            UpdateComments();
        }

        private void Comments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateComments();
        }

        private void Equations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateComments();
        }

        private void Equations_EquationModelPropertyChanged(EquationModel sender, string propertyName)
        {
            // We only care about comment visibility
            if (propertyName.Equals("CommentsVisible"))
            {
                UpdateComments();
            }
        }

        private void DFPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // We only care about comment visibility
            if (e.PropertyName.Equals("CommentsVisible"))
            {
                UpdateComments();
            }
        }

        public void SetWorkspace(ChemProV.Core.Workspace workspace)
        {
            m_workspace = workspace;

            // Attach listeners
            workspace.DegreesOfFreedomAnalysis.PropertyChanged += this.DFPropertyChanged;
            workspace.DegreesOfFreedomAnalysis.Comments.CollectionChanged += new NotifyCollectionChangedEventHandler(Comments_CollectionChanged);
            workspace.Equations.CollectionChanged += new NotifyCollectionChangedEventHandler(Equations_CollectionChanged);
            workspace.Equations.EquationModelPropertyChanged += new EquationCollection.EquationModelPropertyChangedDelegate(Equations_EquationModelPropertyChanged);
            
            // Do an update
            UpdateComments();
        }

        private void UpdateComments()
        {
            // Clear first
            CommentsStack.Children.Clear();

            // Start with equation comments
            for (int i = 0; i < m_workspace.Equations.Count; i++)
            {
                EquationModel model = m_workspace.Equations[i];

                if (!model.CommentsVisible)
                {
                    continue;
                }

                Border brdr = new Border();
                brdr.Margin = new Thickness(3.0, 3.0, 3.0, 0.0);
                brdr.CornerRadius = new CornerRadius(3.0);
                brdr.BorderThickness = new Thickness(2.0);
                brdr.BorderBrush = s_eqBorderBrush;
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Vertical;
                brdr.Child = sp;

                // Add an equation number label at the top
                Label numLabel = new Label();
                numLabel.Content = "Equation " + (i + 1).ToString();
                numLabel.Foreground = s_eqBorderBrush;
                numLabel.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                sp.Children.Add(numLabel);

                // Add each comment
                foreach (BasicComment bc in model.Comments)
                {
                    EqCommentControl cc = new EqCommentControl();
                    cc.CommentObject = bc;
                    cc.Margin = new Thickness(3.0);
                    cc.XLabel.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
                    {
                        model.Comments.Remove(cc.CommentObject);
                        sp.Children.Remove(cc);
                    };
                    sp.Children.Add(cc);
                }

                // Add a button to allow addition of more comments
                Button btn = new Button();
                btn.Margin = new Thickness(3.0);
                Image btnIcon = Core.App.CreateImageFromSource("plus_16x16.png");
                btnIcon.Width = btnIcon.Height = 16;
                btn.Content = btnIcon;
                btn.Tag = model;
                btn.Click += new RoutedEventHandler(AddCommentButtonClick);
                sp.Children.Add(btn);

                CommentsStack.Children.Add(brdr);
            }

            // Next do comments for the degrees of freedom analysis
            if (m_workspace.DegreesOfFreedomAnalysis.CommentsVisible)
            {
                Border brdr = new Border();
                brdr.Margin = new Thickness(3.0, 3.0, 3.0, 0.0);
                brdr.CornerRadius = new CornerRadius(3.0);
                brdr.BorderThickness = new Thickness(2.0);
                brdr.BorderBrush = s_dfBorderBrush;
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Vertical;
                brdr.Child = sp;

                // Add a label at the top
                Label numLabel = new Label();
                numLabel.Content = "DF Analysis";
                numLabel.Foreground = s_dfBorderBrush;
                numLabel.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                sp.Children.Add(numLabel);
                
                foreach (BasicComment bc in m_workspace.DegreesOfFreedomAnalysis.Comments)
                {
                    EqCommentControl cc = new EqCommentControl();
                    cc.CommentObject = bc;
                    cc.Margin = new Thickness(3.0);
                    cc.XLabel.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
                    {
                        m_workspace.DegreesOfFreedomAnalysis.Comments.Remove(cc.CommentObject);

                        // TODO: Figure out if we want this or if event listeners are going to take care of it
                        sp.Children.Remove(cc);
                    };
                    sp.Children.Add(cc);
                }

                // Add a button to allow addition of more comments
                Button btn = new Button();
                btn.Margin = new Thickness(3.0);
                Image btnIcon = Core.App.CreateImageFromSource("plus_16x16.png");
                btnIcon.Width = btnIcon.Height = 16;
                btn.Content = btnIcon;
                btn.Click += new RoutedEventHandler(AddDFCommentButtonClick);
                sp.Children.Add(btn);

                CommentsStack.Children.Add(brdr);
            }
        }
    }
}
