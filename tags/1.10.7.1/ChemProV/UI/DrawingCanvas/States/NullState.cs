using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using ChemProV.UI.DrawingCanvas.Commands;
using ChemProV.PFD;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;

namespace ChemProV.UI.DrawingCanvas.States
{
    public class NullState : IState
    {
        private DrawingCanvas canvas;
        private CommandFactory commandFactory;
        public NullState(DrawingCanvas c, CommandFactory cf = null)
        {
            canvas = c;
            if (cf == null)
            {
                cf = new CommandFactory();
            }
            commandFactory = cf;
        }

        #region IState Members

        public void MouseEnter(object sender, MouseEventArgs e)
        {
        }

        public void MouseLeave(object sender, MouseEventArgs e)
        {
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
        }

        /// <summary>
        /// The user selected something and until mouse left up we are dragging it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
                canvas.SelectedElement = sender as IPfdElement;
            
                //if null then we didn't select anything so don't do nothing
                if (canvas.SelectedElement != null)
                {
                    canvas.CurrentState = canvas.MovingState;
                }
        }

        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// This makes the right click menu with out delete since we are in null state nothing is
        /// selected so do not need delete since we would not know what to delete.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (canvas.HoveringOver == null && canvas.HoveringOverStickyNote == null)
            {
                ContextMenu rightClickMenu = new ContextMenu();
                MenuItem menuItem = new MenuItem();

                menuItem.Header = "Undo";
                rightClickMenu.Items.Add(menuItem);
                menuItem.Click += new RoutedEventHandler((canvas.MenuState as MenuState).Undo);

                menuItem = new MenuItem();
                menuItem.Header = "Redo";
                rightClickMenu.Items.Add(menuItem);
                menuItem.Click += new RoutedEventHandler((canvas.MenuState as MenuState).Redo);

                // rightClickMenu.SetValue(System.Windows.Controls.Canvas.LeftProperty, e.GetPosition(canvas).X);
                // rightClickMenu.SetValue(System.Windows.Controls.Canvas.TopProperty, e.GetPosition(canvas).Y);
                //rightClickMenu.SizeChanged += new SizeChangedEventHandler(rightClickMenu_SizeChanged);

                //It does not make sense that the mouse postion has to be relative to canvas's parent but it does
                //if not the menu will appear in the top left of the page
                rightClickMenu.SetValue(Canvas.LeftProperty, e.GetPosition(canvas.Parent as UIElement).X);
                rightClickMenu.SetValue(Canvas.TopProperty, e.GetPosition(canvas.Parent as UIElement).Y);

                //This is above everything else
                rightClickMenu.SetValue(Canvas.ZIndexProperty, 4);
                canvas.Children.Add(rightClickMenu);

                canvas.rightClickMenu = rightClickMenu;

                canvas.CurrentState = canvas.MenuState;

                e.Handled = true;
            }
            else
            {
                canvas.SelectedElement = canvas.HoveringOver;
                ContextMenu rightClickMenu = null;
                MenuItem menuItem = new MenuItem();
                rightClickMenu = new ContextMenu();

                menuItem.Header = "Undo";
                rightClickMenu.Items.Add(menuItem);
                menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((canvas.MenuState as MenuState).Undo));

                menuItem = new MenuItem();
                menuItem.Header = "Redo";
                rightClickMenu.Items.Add(menuItem);
                menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((canvas.MenuState as MenuState).Redo));
                menuItem = new MenuItem();
                menuItem.Header = "Delete";
                rightClickMenu.Items.Add(menuItem);
                menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((canvas.MenuState as MenuState).Delete));

                if (canvas.HoveringOverStickyNote != null)
                {
                    canvas.SelectedElement = canvas.HoveringOverStickyNote;
                    menuItem = new MenuItem();
                    menuItem.Header = "Blue";
                    rightClickMenu.Items.Add(menuItem);
                    menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((canvas.MenuState as MenuState).ChangeColor));
                    menuItem = new MenuItem();
                    menuItem.Header = "Pink";
                    rightClickMenu.Items.Add(menuItem);
                    menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((canvas.MenuState as MenuState).ChangeColor));
                    menuItem = new MenuItem();
                    menuItem.Header = "Green";
                    rightClickMenu.Items.Add(menuItem);
                    menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((canvas.MenuState as MenuState).ChangeColor));
                    menuItem = new MenuItem();
                    menuItem.Header = "Orange";
                    rightClickMenu.Items.Add(menuItem);
                    menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((canvas.MenuState as MenuState).ChangeColor));
                    menuItem = new MenuItem();
                    menuItem.Header = "Yellow";
                    rightClickMenu.Items.Add(menuItem);
                    menuItem.Click += new RoutedEventHandler(new RoutedEventHandler((canvas.MenuState as MenuState).ChangeColor));
                }

                rightClickMenu.SetValue(Canvas.LeftProperty, e.GetPosition(canvas.Parent as UIElement).X);
                rightClickMenu.SetValue(Canvas.TopProperty, e.GetPosition(canvas.Parent as UIElement).Y);
                canvas.Children.Add(rightClickMenu);

                canvas.rightClickMenu = rightClickMenu;

                if (canvas.SelectedElement is StreamSourceIcon)
                    canvas.SelectedElement = (canvas.SelectedElement as StreamSourceIcon).Stream;
                else if (canvas.SelectedElement is StreamDestinationIcon)
                    canvas.SelectedElement = (canvas.SelectedElement as StreamDestinationIcon).Stream;

                canvas.CurrentState = canvas.MenuState;

                e.Handled = true;
            }
            
        }

        public void MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        public void MouseWheel(object sender, MouseEventArgs e)
        {
        }

        #endregion
    }
}
