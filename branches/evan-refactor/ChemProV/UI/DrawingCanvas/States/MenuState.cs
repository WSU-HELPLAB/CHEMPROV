/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChemProV.PFD.StickyNote;
using System.Windows.Media;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Undos;

namespace ChemProV.UI.DrawingCanvas.States
{
    /// <summary>
    /// The menu state is set as the drawing canvas's current state whenever a right-click occurs. This 
    /// state is responsible for creating and showing an appropriate popup menu and setting up click 
    /// events for the items in the menu.
    /// </summary>
    public class MenuState : IState
    {
        private DrawingCanvas m_canvas;

        private ContextMenu m_contextMenu;

        public MenuState(DrawingCanvas c, Point location)
        {
            m_canvas = c;

            // Create the context menu
            m_contextMenu = new ContextMenu();

            // Undo menu item
            MenuItem menuItem = new MenuItem();
            menuItem.Header = c.UndoTitle;
            menuItem.IsEnabled = (c.UndoCount > 0);
            m_contextMenu.Items.Add(menuItem);
            menuItem.Click += delegate(object sender, RoutedEventArgs e)
            {
                m_canvas.Undo();
            };

            // Redo menu item
            menuItem = new MenuItem();
            menuItem.Header = c.RedoTitle;
            menuItem.IsEnabled = (c.RedoCount > 0);
            m_contextMenu.Items.Add(menuItem);
            menuItem.Click += delegate(object sender, RoutedEventArgs e)
            {
                m_canvas.Redo();
            };

            // Delete (selected object) menu item
            menuItem = new MenuItem();
            menuItem.Header = "Delete";
            menuItem.IsEnabled = !m_canvas.IsReadOnly;
            m_contextMenu.Items.Add(menuItem);
            menuItem.Click += new RoutedEventHandler(this.Delete);
            
            // Hide all sticky notes menu item
            menuItem = new MenuItem();
            menuItem.Header = "Hide All Sticky Notes";
            m_contextMenu.Items.Add(menuItem);
            menuItem.Click += new RoutedEventHandler(this.HideStickyNotes);
            
            // Show all sticky notes menu item
            menuItem = new MenuItem();
            menuItem.Header = "Show All Sticky Notes";
            m_contextMenu.Items.Add(menuItem);
            menuItem.Click += delegate(object sender, RoutedEventArgs e)
            {
                foreach (UIElement uie in m_canvas.Children)
                {
                    // Change visibility if it's a sticky note
                    if (uie is StickyNote)
                    {
                        (uie as StickyNote).Show();
                    }
                }

                // Make sure to remove the popup menu from the canvas
                m_canvas.Children.Remove(m_contextMenu);
                m_contextMenu = null;

                // Flip back to the default state for the canvas (null)
                m_canvas.CurrentState = null;
            };

            // ----- Now we start into stuff that's dependent on the selected item -----

            // Show comment options if the item implements ICommentCollection
            if (m_canvas.SelectedElement is Core.ICommentCollection)
            {
                AddCommentCollectionMenuOptions(m_contextMenu);
            }
            m_contextMenu.SetValue(Canvas.LeftProperty, location.X);
            m_contextMenu.SetValue(Canvas.TopProperty, location.Y);

            // If the user has right-clicked on a process unit then we want to add subprocess options
            if (m_canvas.SelectedElement is PFD.ProcessUnits.IProcessUnit)
            {
                AddSubprocessMenuOptions(m_contextMenu,
                    m_canvas.SelectedElement as PFD.ProcessUnits.IProcessUnit);
            }

            // Set the Z-index to put the menu on top of everything else
            m_contextMenu.SetValue(Canvas.ZIndexProperty, 4);
            m_canvas.Children.Add(m_contextMenu);
        }

        #region IState Members

        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // This implies that the user clicked somewhere within the canvas that wasn't over 
            // the popup menu. So we need to remove the popup menu from the canvas and flip back 
            // to the default state for the canvas (null;
            m_canvas.Children.Remove(m_contextMenu);
            m_contextMenu = null;
            m_canvas.CurrentState = null;
        }

        #region Unused Mouse Events

        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        public void MouseWheel(object sender, MouseEventArgs e)
        {
        }

        public void MouseEnter(object sender, MouseEventArgs e)
        {
        }

        public void MouseLeave(object sender, MouseEventArgs e)
        {
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
        }

        #endregion Unused Mouse Events

        /// <summary>
        /// This is used to changed color of the sticky notes
        /// TODO: Delete since it is not used anymore (keeping temporarily for reference)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ChangeStickyNoteColor(object sender, EventArgs e)
        {
            // Start by adding an undo action that will restore the color we about to change
            m_canvas.AddUndo(new UndoRedoCollection("Undo stick note color change",
                new PFD.Undos.RestoreStickyNoteColor(m_canvas.SelectedElement as StickyNote)));
            
            // Change the color
            string header = ((sender as MenuItem).Header as string);
            StickyNoteColors color = StickyNote.StickyNoteColorsFromString(header);
            (m_canvas.SelectedElement as StickyNote).ColorChange(color);

            // Make sure to remove the popup menu from the canvas
            m_canvas.Children.Remove(m_contextMenu);
            m_contextMenu = null;

            // Flip back to the default state for the canvas (null)
            m_canvas.CurrentState = null;
        }

        public void HideStickyNotes(object sender, EventArgs e)
        {
            foreach (UIElement uie in m_canvas.Children)
            {
                if (uie is StickyNote)
                {
                    (uie as StickyNote).Hide();
                }
            }

            // Make sure to remove the popup menu from the canvas
            m_canvas.Children.Remove(m_contextMenu);
            m_contextMenu = null;

            // Flip back to the default state for the canvas (null)
            m_canvas.CurrentState = null;
        }

        /// <summary>
        /// This is called when the user selects the Delete from the right click menu
        /// </summary>
        /// <param name="sender">not used</param>
        /// <param name="e">not used</param>
        public void Delete(object sender, EventArgs e)
        {
            // Start by getting rid of the popup menu
            m_canvas.Children.Remove(m_contextMenu);
            m_contextMenu = null;

            // Use the DrawingCanvasCommands static class
            Core.DrawingCanvasCommands.DeleteSelectedElement(m_canvas);

            // Go back to the null state
            m_canvas.CurrentState = null;
        }

        public void LostMouseCapture(object sender, MouseEventArgs e)
        {
        }

        public void StateEnding()
        {
            if (null != m_contextMenu)
            {
                // Get rid of the popup menu if it's still on the drawing canvas
                m_canvas.Children.Remove(m_contextMenu);
                m_contextMenu = null;
            }
        }

        #endregion IState Members

        private void AddCommentCollectionMenuOptions(ContextMenu newContextMenu)
        {
            // Make the header in the menu for the comment-specific options
            MenuItem menuItem = new MenuItem();
            menuItem.Header = "Comment Options";
            newContextMenu.Items.Add(menuItem);
            // We're using this item as a label, so don't let the user click it
            menuItem.IsHitTestVisible = false;
            // Change the colors to signal to the user that it's a label
            menuItem.Background = new SolidColorBrush(Colors.LightGray);
            menuItem.Foreground = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
            menuItem.FontWeight = FontWeights.Bold;

            menuItem = new MenuItem();
            menuItem.Header = "Add new comment...";
            menuItem.Tag = m_canvas.SelectedElement;
            newContextMenu.Items.Add(menuItem);

            // Use an anonymous delegate to handle the click event
            menuItem.Click += delegate(object sender, RoutedEventArgs e)
            {
                MenuItem tempMI = sender as MenuItem;
                Core.ICommentCollection cc = tempMI.Tag as Core.ICommentCollection;

                // Build the new comment sticky note on the canvas and add undo
                StickyNote sn;
                m_canvas.AddUndo(new UndoRedoCollection("Undo creation of comment",
                    StickyNote.CreateCommentNote(m_canvas, cc, null, out sn).ToArray()));

                // Make sure to remove the popup menu from the canvas
                m_canvas.Children.Remove(m_contextMenu);
                m_contextMenu = null;

                // Flip back to the default state for the canvas (null)
                m_canvas.CurrentState = null;
            };

            string objName = "selected object";
            if (m_canvas.SelectedElement is PFD.ProcessUnits.LabeledProcessUnit)
            {
                objName = (m_canvas.SelectedElement as PFD.ProcessUnits.LabeledProcessUnit).ProcessUnitLabel;
            }
            else if (m_canvas.SelectedElement is AbstractStream)
            {
                objName = "selected stream";
            }

            // Add a new menu item to hide all comments
            menuItem = new MenuItem();
            menuItem.Header = "Hide all comments for " + objName;
            menuItem.Tag = m_canvas.SelectedElement;
            newContextMenu.Items.Add(menuItem);
            menuItem.Click += delegate(object sender, RoutedEventArgs e)
            {
                MenuItem tempMI = sender as MenuItem;
                Core.ICommentCollection cc = tempMI.Tag as Core.ICommentCollection;

                for (int i = 0; i < cc.CommentCount; i++)
                {
                    StickyNote sn = cc.GetCommentAt(i) as StickyNote;
                    sn.Hide();
                }

                // Make sure to remove the popup menu from the canvas
                m_canvas.Children.Remove(m_contextMenu);
                m_contextMenu = null;

                // Flip back to the default state for the canvas (null)
                m_canvas.CurrentState = null;
            };

            // Add one to show all comments too
            menuItem = new MenuItem();
            menuItem.Header = "Show all comments for " + objName;
            menuItem.Tag = m_canvas.SelectedElement;
            newContextMenu.Items.Add(menuItem);
            menuItem.Click += delegate(object sender, RoutedEventArgs e)
            {
                MenuItem tempMI = sender as MenuItem;
                Core.ICommentCollection cc = tempMI.Tag as Core.ICommentCollection;

                for (int i = 0; i < cc.CommentCount; i++)
                {
                    StickyNote sn = cc.GetCommentAt(i) as StickyNote;
                    sn.Show();
                }

                // Make sure to remove the popup menu from the canvas
                m_canvas.Children.Remove(m_contextMenu);
                m_contextMenu = null;

                // Flip back to the default state for the canvas (null)
                m_canvas.CurrentState = null;
            };
        }

        /// <summary>
        /// Adds menu options for the subprocess color changing
        /// </summary>
        private void AddSubprocessMenuOptions(ContextMenu newContextMenu, PFD.ProcessUnits.IProcessUnit pu)
        {
            MenuItem parentMenuItem = new MenuItem();
            parentMenuItem.Header = "Subprocess";
            newContextMenu.Items.Add(parentMenuItem);
            // We're using this item as a label, so don't let the user click it
            parentMenuItem.IsHitTestVisible = false;
            // Change the colors to signal to the user that it's a label
            parentMenuItem.Background = new SolidColorBrush(Colors.LightGray);
            parentMenuItem.Foreground = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
            parentMenuItem.FontWeight = FontWeights.Bold;

            // Create a submenu for colors. See the declaration of the Core.NamedColors.All array for 
            // the list of colors that are being used. Make changes in that array (not in this code) 
            // to change the list of available colors.
            foreach (Core.NamedColor nc in Core.NamedColors.All)
            {
                MenuItem menuItem = new MenuItem();
                menuItem.Header = nc.Name;
                menuItem.Tag = System.Tuple.Create<PFD.ProcessUnits.IProcessUnit, Color>(
                    pu, nc.Color);

                // Show the menu item with a check next to it if it's the current color
                if (nc.Color.Equals(pu.Subprocess))
                {
                    menuItem.Icon = Core.App.CreateImageFromSource("check_16x16.png");
                }

                // Add it to the menu
                newContextMenu.Items.Add(menuItem);

                // Use an anonymous delegate to handle the click event
                menuItem.Click += delegate(object sender, RoutedEventArgs e)
                {
                    // Get the objects we need
                    MenuItem tempMI = sender as MenuItem;
                    System.Tuple<PFD.ProcessUnits.IProcessUnit, Color> t = tempMI.Tag as
                        System.Tuple<PFD.ProcessUnits.IProcessUnit, Color>;
                    
                    // Create undo item before setting the new subgroup
                    m_canvas.AddUndo(new UndoRedoCollection("Undo subprocess change",
                        new SetSubprocess(t.Item1)));

                    // Set the subgroup
                    t.Item1.Subprocess = t.Item2;

                    m_canvas.PFDModified();

                    // Make sure to remove the popup menu from the canvas
                    m_canvas.Children.Remove(m_contextMenu);
                    m_contextMenu = null;

                    // Flip back to the default state for the canvas (null)
                    m_canvas.CurrentState = null;
                };
            }
        }
    }
}