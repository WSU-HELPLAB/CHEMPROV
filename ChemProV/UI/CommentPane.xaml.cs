using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChemProV.Core;
using ChemProV.Logic.Equations;
using ChemProV.Logic;

namespace ChemProV.UI
{
    /// <summary>
    /// Control to show comments for a workspace. Currently only supports equation comments and 
    /// comments for the degrees of freedom analysis.
    /// </summary>
    public partial class CommentPane : UserControl
    {
        /// <summary>
        /// List of EquationModel objects whose comment collections we are monitoring
        /// </summary>
        private List<EquationModel> m_equationListeners = new List<EquationModel>();
        
        /// <summary>
        /// List of process units whose comment collections we are monitoring
        /// </summary>
        private List<AbstractProcessUnit> m_puListeners = new List<AbstractProcessUnit>();
        
        /// <summary>
        /// The list of sticky notes whose PropertyChanged events we are subscribed to. This 
        /// will contain all free floating sticky notes in the workspace, all stream comments, 
        /// and all process unit comments. Therefore, this collection gets updated whenever:
        ///  1. The collection of free-floating sticky notes in the workspace changes
        ///  2. The collection of streams in the workspace changes
        ///  3. The collection of process units in the workspace changes
        ///  4. The collection of comments of any stream in the workspace changes
        ///  5. The collection of comments of any process unit in the workspace changes
        /// </summary>
        private List<Logic.StickyNote> m_snListeners = new List<Logic.StickyNote>();

        /// <summary>
        /// List of streams whose comment collections we are monitoring
        /// </summary>
        private List<AbstractStream> m_streamListeners = new List<AbstractStream>();
        
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

        private void AddDFCommentButtonClick(object sender, RoutedEventArgs e)
        {
            DegreesOfFreedomAnalysis dfa = m_workspace.DegreesOfFreedomAnalysis;

            // Create an undo that will remove the comment that we're about to add
            m_workspace.AddUndo(new UndoRedoCollection("Undo adding degrees of freedom analysis comment",
                new Logic.Undos.RemoveBasicComment(dfa.Comments, dfa.Comments.Count)));
            
            // Add a new comment to the collection. Event handlers will update the UI.
            dfa.Comments.Add(new BasicComment());
        }

        private void AddEqCommentButtonClick(object sender, RoutedEventArgs e)
        {
            EquationModel model = (sender as Button).Tag as EquationModel;
            model.Comments.Add(new BasicComment(string.Empty, null));
            
            // Create and undo that will delete the comment we just added
            m_workspace.AddUndo(new UndoRedoCollection(
                "Undo addition of new equation comment",
                new Logic.Undos.RemoveBasicComment(model.Comments, model.Comments.Count - 1)));
            
            UpdateComments();
        }

        private void Comments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateComments();
        }

        /// <summary>
        /// Gets the number of hidden comments (StickyNote objects) within a collection
        /// </summary>
        private int CountHiddenComments(IEnumerable<StickyNote> collection)
        {
            int count = 0;
            foreach (StickyNote sn in collection)
            {
                if (!sn.IsVisible)
                {
                    count++;
                }
            }

            return count;
        }

        private void DFPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // We only care about comment visibility
            if (e.PropertyName.Equals("CommentsVisible"))
            {
                UpdateComments();
            }
        }

        private void Equations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Start by unsubscribing from all the equation events in our local list. This is exactly why 
            // we keep this list. If equations are deleted from the workspace then we must make sure we 
            // no longer monitor events for them.
            foreach (EquationModel em in m_equationListeners)
            {
                em.Comments.CollectionChanged -= new NotifyCollectionChangedEventHandler(Comments_CollectionChanged);
            }

            // Clear and rebuild the list, subscribing to comment collection changes on each equation
            m_equationListeners.Clear();
            foreach (EquationModel em in m_workspace.Equations)
            {
                em.Comments.CollectionChanged += new NotifyCollectionChangedEventHandler(Comments_CollectionChanged);
                m_equationListeners.Add(em);
            }
            
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

        private void PFDCommentsOptionChecked(object sender, RoutedEventArgs e)
        {
            UpdatePFDComments();
        }

        private void ProcessUnitCommentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RebuildStickyNoteList();
        }

        private void RebuildStickyNoteList()
        {
            // Start by unsubscribing from everything we are subscribed to
            foreach (Logic.StickyNote sn in m_snListeners)
            {
                sn.PropertyChanged -= this.StickyNotePropertyChanged;
            }

            // Subscribe to all free-floating sticky notes and keep track of them in our list
            foreach (Logic.StickyNote sn in m_workspace.StickyNotes)
            {
                m_snListeners.Add(sn);
                sn.PropertyChanged += this.StickyNotePropertyChanged;
            }

            // Subscribe to all sticky note comments for all streams
            foreach (AbstractStream stream in m_workspace.Streams)
            {
                foreach (StickyNote sn in stream.Comments)
                {
                    m_snListeners.Add(sn);
                    sn.PropertyChanged += this.StickyNotePropertyChanged;
                }
            }
            
            // Subscribe to all sticky note comments for all process units
            foreach (AbstractProcessUnit unit in m_workspace.ProcessUnits)
            {
                foreach (StickyNote sn in unit.Comments)
                {
                    m_snListeners.Add(sn);
                    sn.PropertyChanged += this.StickyNotePropertyChanged;
                }
            }

            UpdatePFDComments();
        }

        public void SetWorkspace(Workspace workspace)
        {
            m_workspace = workspace;

            // Attach listeners
            workspace.DegreesOfFreedomAnalysis.PropertyChanged += this.DFPropertyChanged;
            workspace.DegreesOfFreedomAnalysis.Comments.CollectionChanged += new NotifyCollectionChangedEventHandler(Comments_CollectionChanged);
            workspace.Equations.CollectionChanged += new NotifyCollectionChangedEventHandler(Equations_CollectionChanged);
            workspace.Equations.EquationModelPropertyChanged += new EquationCollection.EquationModelPropertyChangedDelegate(Equations_EquationModelPropertyChanged);
            workspace.StickyNotes.CollectionChanged += new NotifyCollectionChangedEventHandler(WorkspaceStickyNotesCollectionChanged);

            // Subscribe to sticky notes
            foreach (Logic.StickyNote sn in workspace.StickyNotes)
            {
                m_snListeners.Add(sn);
                sn.PropertyChanged += new PropertyChangedEventHandler(StickyNotePropertyChanged);
            }
            
            // When stream and process unit collections change we also need to update
            workspace.StreamsCollectionChanged += this.WorkspaceStreamsCollectionChanged;
            workspace.ProcessUnitsCollectionChanged += this.WorkspaceProcessUnitsCollectionChanged;
            
            // Do an update
            UpdateComments();
            UpdatePFDComments();
        }

        /// <summary>
        /// Invoked when a sticky note in the PFD has a property changed
        /// </summary>
        private void StickyNotePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // The comment control monitors text changes so we only care about visibility here
            if (e.PropertyName.Equals("IsVisible"))
            {
                UpdatePFDComments();
            }
        }

        private void StreamsCommentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RebuildStickyNoteList();
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
                    PaneCommentControl cc = new PaneCommentControl();
                    cc.SetCommentObject(bc);
                    cc.Margin = new Thickness(3.0);
                    cc.XLabel.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
                    {
                        // Create and undo that adds the comment back
                        BasicComment toRemove = cc.CommentObject as BasicComment;
                        m_workspace.AddUndo(new UndoRedoCollection(
                            "Undo deletion of equation comment",
                            new Logic.Undos.InsertBasicComment(toRemove, model.Comments, model.Comments.IndexOf(toRemove))));
                        
                        model.Comments.Remove(cc.CommentObject as BasicComment);
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
                btn.Click += new RoutedEventHandler(AddEqCommentButtonClick);
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
                
                DegreesOfFreedomAnalysis dfa = m_workspace.DegreesOfFreedomAnalysis;
                foreach (BasicComment bc in dfa.Comments)
                {
                    PaneCommentControl pcc = new PaneCommentControl();
                    pcc.SetCommentObject(bc);
                    pcc.Margin = new Thickness(3.0);
                    pcc.XLabel.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
                    {
                        BasicComment toRemove = pcc.CommentObject as BasicComment;
                        int index = dfa.Comments.IndexOf(toRemove);
                        
                        // Add an undo that will re-insert the comment that we're about to delete
                        m_workspace.AddUndo(new UndoRedoCollection(
                            "Undo deletion of comment for degrees of freedom analysis",
                            new Logic.Undos.InsertBasicComment(toRemove, dfa.Comments, index)));

                        // Now delete the comment
                        m_workspace.DegreesOfFreedomAnalysis.Comments.Remove(pcc.CommentObject as BasicComment);
                    };
                    sp.Children.Add(pcc);
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

        private void UpdatePFDComments()
        {
            // Apparently this sometimes gets fired before everything is initialized
            if (null == PFDCommentsStack)
            {
                return;
            }

            // Special case: If we don't want any PFD comments then clear and return
            if (rbPFDCommentsNone.IsChecked.Value)
            {
                PFDCommentsStack.Children.Clear();
                return;
            }

            // What we want to do here is avoid needless creation of controls, as this is 
            // computationally expensive. Therefore we want to reuse any existing controls 
            // that are already in PFDCommentsStack. The total number of controls we will 
            // need is: 
            //   (# free-floating sticky notes that need to be shown) + 
            //   (# comments in all streams that need to be shown) + 
            //   (# comments in all process units that need to be shown)
            int count = 0;
            if (rbPFDCommentsAll.IsChecked.Value)
            {
                // Count up all sticky notes (visible or not)
                count = m_workspace.StickyNotes.Count;
                foreach (AbstractStream stream in m_workspace.Streams)
                {
                    count += stream.Comments.Count;
                }
                foreach (AbstractProcessUnit unit in m_workspace.ProcessUnits)
                {
                    count += unit.Comments.Count;
                }
            }
            else
            {
                // Count up all hidden sticky notes
                count = CountHiddenComments(m_workspace.StickyNotes);
                foreach (AbstractStream stream in m_workspace.Streams)
                {
                    count += CountHiddenComments(stream.Comments);
                }
                foreach (AbstractProcessUnit unit in m_workspace.ProcessUnits)
                {
                    count += CountHiddenComments(unit.Comments);
                }
            }

            // We now know how many controls we need, so we need to add or remove controls to get 
            // to this exact number
            while (PFDCommentsStack.Children.Count > count)
            {
                // Remove the last child. Set its comment object to null first to make sure that 
                // it unsubsribes from events
                int index = PFDCommentsStack.Children.Count - 1;
                (PFDCommentsStack.Children[index] as PaneCommentControl).SetCommentObject(null);
                PFDCommentsStack.Children.RemoveAt(index);
            }
            while (PFDCommentsStack.Children.Count < count)
            {
                // Add a new comment control
                PFDCommentsStack.Children.Add(new PaneCommentControl()
                    {
                        Margin = new Thickness(3.0)
                    });
            }

            // Start with free-floating comments
            count = 0;
            foreach (Logic.StickyNote sn in m_workspace.StickyNotes)
            {
                if (rbPFDCommentsAll.IsChecked.Value ||
                    (!sn.IsVisible && rbPFDCommentsHidden.IsChecked.Value))
                {
                    // Set the comment object for the existing control
                    (PFDCommentsStack.Children[count] as PaneCommentControl).SetCommentObject(sn, null);
                    count++;
                }
            }

            // Now do comments for streams
            foreach (AbstractStream stream in m_workspace.Streams)
            {
                foreach (Logic.StickyNote sn in stream.Comments)
                {
                    if (rbPFDCommentsAll.IsChecked.Value ||
                        (!sn.IsVisible && rbPFDCommentsHidden.IsChecked.Value))
                    {
                        // Set the comment object for the existing control
                        (PFDCommentsStack.Children[count] as PaneCommentControl).SetCommentObject(sn, stream);
                        count++;
                    }
                }
            }

            // Do comments for process units
            foreach (AbstractProcessUnit unit in m_workspace.ProcessUnits)
            {
                foreach (Logic.StickyNote sn in unit.Comments)
                {
                    if (rbPFDCommentsAll.IsChecked.Value ||
                        (!sn.IsVisible && rbPFDCommentsHidden.IsChecked.Value))
                    {
                        // Set the comment object for the existing control
                        (PFDCommentsStack.Children[count] as PaneCommentControl).SetCommentObject(sn, unit);
                        count++;
                    }
                }
            }
        }

        private void WorkspaceProcessUnitsCollectionChanged(object sender, EventArgs e)
        {
            // First unsubscribe from everything that we are currently subscribed to
            foreach (AbstractProcessUnit unit in m_puListeners)
            {
                unit.Comments.CollectionChanged -= ProcessUnitCommentsCollectionChanged;
            }
            m_puListeners.Clear();

            // Now subscribe to the comment collection in every process unit
            foreach (AbstractProcessUnit unit in m_workspace.ProcessUnits)
            {
                unit.Comments.CollectionChanged += this.StreamsCommentsCollectionChanged;
                m_puListeners.Add(unit);
            }

            // Do an update
            RebuildStickyNoteList();
        }

        private void WorkspaceStickyNotesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RebuildStickyNoteList();
        }

        private void WorkspaceStreamsCollectionChanged(object sender, EventArgs e)
        {
            // First unsubscribe from everything that we are currently subscribed to
            foreach (AbstractStream stream in m_streamListeners)
            {
                stream.Comments.CollectionChanged -= this.StreamsCommentsCollectionChanged;
            }
            m_streamListeners.Clear();

            // Now subscribe to the comment collection in every stream
            foreach (AbstractStream stream in m_workspace.Streams)
            {
                stream.Comments.CollectionChanged += this.StreamsCommentsCollectionChanged;
                m_streamListeners.Add(stream);
            }

            // Do an update
            RebuildStickyNoteList();
        }
    }
}
