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
using ChemProV.PFD.StickyNote;
using ChemProV.PFD.Streams.PropertiesTable;

namespace ChemProV.UI.DrawingCanvas.States
{
    public class SelectedState : IState
    {

        private DrawingCanvas canvas;
        private CommandFactory commandFactory;

        public SelectedState(DrawingCanvas c, CommandFactory cf = null)
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
        /// Try to move whatever we clicked on, if sender is not IPfdElement the cast will return null
        /// and so we go to null state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            canvas.SelectedElement = sender as IPfdElement;
            if (canvas.SelectedElement != null)
            {
                canvas.CurrentState = canvas.MovingState;
            }
            else
            {
                canvas.CurrentState = canvas.NullState;
            }
        }

        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// This makes our right click menu with delete since something must be selected since we are 
        /// in the selected state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (canvas.HoveringOverStickyNote != null)
            {
                canvas.SelectedElement = canvas.HoveringOverStickyNote;
            }
            else if (canvas.HoveringOver != null)
            {
                canvas.SelectedElement = canvas.HoveringOver;
            }

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

            if (canvas.SelectedElement is StickyNote)
            {
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


            //rightClickMenu.SizeChanged += new SizeChangedEventHandler(rightClickMenu_SizeChanged);

            rightClickMenu.SetValue(Canvas.LeftProperty, e.GetPosition(canvas.Parent as UIElement).X);
            rightClickMenu.SetValue(Canvas.TopProperty, e.GetPosition(canvas.Parent as UIElement).Y);

            //this is so the menu will appear above everything else
            rightClickMenu.SetValue(Canvas.ZIndexProperty, 4);
            canvas.Children.Add(rightClickMenu);

            if (canvas.SelectedElement is StreamSourceIcon)
                canvas.SelectedElement = (canvas.SelectedElement as StreamSourceIcon).Stream;
            else if (canvas.SelectedElement is StreamDestinationIcon)
                canvas.SelectedElement = (canvas.SelectedElement as StreamDestinationIcon).Stream;

            canvas.CurrentState = canvas.MenuState;

            canvas.rightClickMenu = rightClickMenu;

            e.Handled = true;

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
