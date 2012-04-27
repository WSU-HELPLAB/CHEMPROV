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
using ChemProV.UI.DrawingCanvas.Commands;
using System.Windows.Media;
using ChemProV.PFD.Streams;

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
            m_contextMenu.Items.Add(menuItem);
            menuItem.Click += delegate(object sender, RoutedEventArgs e)
            {
                m_canvas.Undo();
            };

            // Redo menu item
            menuItem = new MenuItem();
            menuItem.Header = c.RedoTitle;
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
                        (uie as StickyNote).Visibility = Visibility.Visible;
                    }
                }

                // Make sure to remove the popup menu from the canvas
                m_canvas.Children.Remove(m_contextMenu);
                m_contextMenu = null;

                // Flip back to the default state for the canvas (null)
                m_canvas.CurrentState = null;
            };

            // ----- Now we start into stuff that's dependent on the selected item -----
            
            if (m_canvas.SelectedElement is StickyNote)
            {
                menuItem = new MenuItem();
                menuItem.Header = "Blue";
                m_contextMenu.Items.Add(menuItem);
                menuItem.Click += new RoutedEventHandler(ChangeStickyNoteColor);
                menuItem = new MenuItem();
                menuItem.Header = "Pink";
                m_contextMenu.Items.Add(menuItem);
                menuItem.Click += new RoutedEventHandler(ChangeStickyNoteColor);
                menuItem = new MenuItem();
                menuItem.Header = "Green";
                m_contextMenu.Items.Add(menuItem);
                menuItem.Click += new RoutedEventHandler(ChangeStickyNoteColor);
                menuItem = new MenuItem();
                menuItem.Header = "Orange";
                m_contextMenu.Items.Add(menuItem);
                menuItem.Click += new RoutedEventHandler(ChangeStickyNoteColor);
                menuItem = new MenuItem();
                menuItem.Header = "Yellow";
                m_contextMenu.Items.Add(menuItem);
                menuItem.Click += new RoutedEventHandler(ChangeStickyNoteColor);
            }

            //if (m_canvas.SelectedElement is StreamSourceIcon)
            //{
            //    m_canvas.SelectedElement = (m_canvas.SelectedElement as StreamSourceIcon).Stream;
            //}
            //else if (m_canvas.SelectedElement is StreamDestinationIcon)
            //{
            //    m_canvas.SelectedElement = (m_canvas.SelectedElement as StreamDestinationIcon).Stream;
            //}

            // E.O.
            // If the user has right-clicked on a process unit then we want to add subgroup options
            if (m_canvas.SelectedElement is PFD.ProcessUnits.IProcessUnit)
            {
                AddSubgroupMenuOptions(m_contextMenu,
                    m_canvas.SelectedElement as PFD.ProcessUnits.IProcessUnit);
            }

            // E.O.
            // Show comment options if the item implements ICommentCollection
            if (m_canvas.SelectedElement is Core.ICommentCollection)
            {
                // Make the header in the menu for the comment-specific options
                menuItem = new MenuItem();
                menuItem.Header = "Comment Options";
                m_contextMenu.Items.Add(menuItem);
                // We're using this item as a label, so don't let the user click it
                menuItem.IsHitTestVisible = false;
                // Change the colors to signal to the user that it's a label
                menuItem.Background = new SolidColorBrush(Colors.LightGray);
                menuItem.Foreground = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
                menuItem.FontWeight = FontWeights.Bold;

                menuItem = new MenuItem();
                menuItem.Header = "Add new comment...";
                menuItem.Tag = m_canvas.SelectedElement;
                m_contextMenu.Items.Add(menuItem);

                // Use an anonymous delegate to handle the click event
                menuItem.Click += delegate(object sender, RoutedEventArgs e)
                {
                    MenuItem tempMI = sender as MenuItem;
                    Core.ICommentCollection cc = tempMI.Tag as Core.ICommentCollection;

                    // This is kind of hacky, but we add a comment with null properties to
                    // get the interface to pop up so that the user can enter a new comment
                    cc.AddComment(new Core.Comment(null, null));

                    // Make sure to remove the popup menu from the canvas
                    m_canvas.Children.Remove(m_contextMenu);
                    m_contextMenu = null;

                    // Flip back to the default state for the canvas (null)
                    m_canvas.CurrentState = null;
                };
            }
            m_contextMenu.SetValue(Canvas.LeftProperty, location.X);
            m_contextMenu.SetValue(Canvas.TopProperty, location.Y);

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
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ChangeStickyNoteColor(object sender, EventArgs e)
        {
            // Start by adding an undo action that will restore the color we about to change
            m_canvas.AddUndo(new PFD.UndoRedoCollection("Undo stick note color change",
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
                    (uie as StickyNote).Visibility = Visibility.Collapsed;
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
                // Start by getting rid of the popup menu
                m_canvas.Children.Remove(m_contextMenu);
                m_contextMenu = null;
            }
        }

        #endregion IState Members

        /// <summary>
        /// E.O.
        /// </summary>
        private void AddSubgroupMenuOptions(ContextMenu newContextMenu, PFD.ProcessUnits.IProcessUnit pu)
        {
            MenuItem parentMenuItem = new MenuItem();
            parentMenuItem.Header = "Process Unit Subgroup";
            newContextMenu.Items.Add(parentMenuItem);
            // We're using this item as a label, so don't let the user click it
            parentMenuItem.IsHitTestVisible = false;
            // Change the colors to signal to the user that it's a label
            parentMenuItem.Background = new SolidColorBrush(Colors.LightGray);
            parentMenuItem.Foreground = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
            parentMenuItem.FontWeight = FontWeights.Bold;

            // Create a submenu for colors. See the declaration of the s_subgroupColors array for 
            // the list of colors that are being used. Make changes in that array (not in this code) 
            // to change the list of available colors.
            foreach (NamedColor nc in s_subgroupColors)
            {
                MenuItem menuItem = new MenuItem();
                menuItem.Header = nc.Name;
                menuItem.Tag = System.Tuple.Create<PFD.ProcessUnits.IProcessUnit, Color>(
                    pu, nc.Color);

                // Show the menu item with a check next to it if it's the current color
                if (nc.Color.Equals(pu.Subgroup))
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
                    m_canvas.AddUndo(new PFD.UndoRedoCollection("Undo process unit subgroup change",
                        new PFD.Undos.SetProcessSubgroup(t.Item1)));

                    // Set the subgroup
                    t.Item1.Subgroup = t.Item2;

                    // Make sure to remove the popup menu from the canvas
                    m_canvas.Children.Remove(m_contextMenu);
                    m_contextMenu = null;

                    // Flip back to the default state for the canvas (null)
                    m_canvas.CurrentState = null;
                };
            }
        }

        /// <summary>
        /// E.O.
        /// Array of all possible subgroup colors. You can add or remove colors as you like 
        /// and they will appear in the "Process Unit Subgroup" popup menu when the user 
        /// right-clicks on a process unit.
        /// </summary>
        private static readonly NamedColor[] s_subgroupColors = new NamedColor[]{
            new NamedColor("White", Colors.White), // White is the default
            new NamedColor("Red", Colors.Red), new NamedColor("Green", Colors.Green),
            new NamedColor("Blue", Colors.Blue), new NamedColor("Cyan", Colors.Cyan),
            new NamedColor("Magenta", Colors.Magenta), new NamedColor("Yellow", Colors.Yellow)};

        /// <summary>
        /// E.O.
        /// Immutable structure for a color with a name.
        /// </summary>
        private struct NamedColor
        {
            private Color m_clr;
            private string m_name;

            public NamedColor(string name, Color color)
            {
                m_clr = color;
                m_name = name;
            }

            public Color Color
            {
                get
                {
                    return m_clr;
                }
            }

            public string Name
            {
                get
                {
                    return m_name;
                }
            }
        }
    }
}