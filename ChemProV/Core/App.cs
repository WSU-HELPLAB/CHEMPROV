/*
Copyright 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace ChemProV.Core
{
    /// <summary>
    /// Static class to contain global properties and methods that are relevant to the application
    /// </summary>
    public static class App
    {
        private static List<LogItem> s_log = new List<LogItem>();

        private static MainPage s_mainPage = null;

        private static ChemProV.UI.ControlPalette s_palette = null;

        /// <summary>
        /// Stores a reference to an active popup. Will be null if there is no active popup.
        /// </summary>
        private static Popup s_popup = null;
        
        private static ChemProV.UI.WorkspaceControl s_workspace = null;

        /// <summary>
        /// Closes the open popup menu, if one exists
        /// </summary>
        public static void ClosePopup()
        {
            if (null != s_popup)
            {
                s_popup.IsOpen = false;
                s_popup = null;
            }
        }

        public static ChemProV.UI.ControlPalette ControlPalette
        {
            get
            {
                return s_palette;
            }
        }

        /// <summary>
        /// Creates an image from the specified project source
        /// Remember that the image's "BuildAction" must be set to "Content"
        /// </summary>
        public static Image CreateImageFromSource(string source)
        {
            if (!source.StartsWith("/UI"))
            {
                source = "/UI/Icons/" + source;
            }
            
            Image img = new Image();
            BitmapImage bmp = new BitmapImage();
            bmp.UriSource = new Uri(source, UriKind.Relative);
            img.SetValue(Image.SourceProperty, bmp);

            return img;
        }

        /// <summary>
        /// This must be called upon application initialization
        /// </summary>
        public static void Init(MainPage mainPage, ChemProV.UI.ControlPalette palette)
        {
            s_mainPage = mainPage;
            s_workspace = mainPage.WorkSpace;
            s_palette = palette;
        }

        /// <summary>
        /// Sets up a right-click menu for the text box that has cut/copy/paste options
        /// </summary>
        /// <param name="textBox">Text box to initialize the menu for. The right-mouse-down 
        /// event for this text box will then cause the popup menu to be shown.</param>
        public static void InitRightClickMenu(TextBox textBox)
        {
            textBox.MouseRightButtonDown += new MouseButtonEventHandler(TextBox_MouseRightButtonDown);
        }

        /// <summary>
        /// Launches the specified popup menu. Note that if the previous popup menu shown by this function 
        /// is still open then it is closed before showing the new one.
        /// </summary>
        public static void LaunchPopup(System.Windows.Controls.Primitives.Popup popup, MouseEventArgs e)
        {
            // If a previous popup was open, close it first
            if (null != s_popup)
            {
                s_popup.IsOpen = false;
                s_popup = null;
            }
            
            // Position the popup
            Point pt = e.GetPosition(s_mainPage);
            popup.HorizontalOffset = pt.X;
            popup.VerticalOffset = pt.Y;

            // Store a reference to it so that it can be hidden when the user clicks elsewhere
            s_popup = popup;

            // Show the menu (required before getting its size)
            popup.IsOpen = true;
            popup.UpdateLayout();

            // Get sizes
            Size popSize = (popup.Child as Control).DesiredSize;
            Size s = s_mainPage.LayoutRoot.DesiredSize;

            // Make sure we don't fly off an edge
            if (popup.HorizontalOffset + popSize.Width > s.Width)
            {
                popup.HorizontalOffset = s.Width - popSize.Width;
            }
            if (popup.HorizontalOffset < 0.0)
            {
                popup.HorizontalOffset = 0.0;
            }
            if (popup.VerticalOffset + popSize.Height > s.Height)
            {
                popup.VerticalOffset = s.Height - popSize.Height;
            }
            if (popup.VerticalOffset < 0.0)
            {
                popup.VerticalOffset = 0.0;
            }
        }

        /// <summary>
        /// Writes a message to the application's log. Currently this is just an in-memory log 
        /// used mainly for debugging, but it might be altered to write to a file in the future.
        /// </summary>
        public static void Log(LogItemType type, string message)
        {
            s_log.Add(new LogItem(type, message));
        }

        public static void MessageBox(string message)
        {
            System.Windows.MessageBox.Show(message);
        }

        public static ChemProV.Logic.OSBLE.OSBLEState OSBLEState
        {
            get;
            set;
        }

        /// <summary>
        /// Handles the right-mouse-button-down event for a text box in the properties window. Creates 
        /// and displays a popup menu with cut/copy/paste options.
        /// </summary>
        private static void TextBox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (null == tb)
            {
                return;
            }

            // Mark the mouse event as handled
            e.Handled = true;

            // Build context menu items
            MenuItem cutItem = new MenuItem()
            {
                Header = "Cut",
                IsEnabled = !string.IsNullOrEmpty(tb.SelectedText)
            };
            cutItem.Click += delegate(object senderMenuItem, RoutedEventArgs menuItemClickEventArgs)
            {
                try
                {
                    Clipboard.SetText(tb.SelectedText);
                }
                catch (System.Security.SecurityException)
                {
                    // Will be thrown if the user denies the app access to the clipboard. There's not 
                    // much we can do in this case. We just need to make sure the popup menu is hidden
                    Core.App.ClosePopup();
                    return;
                }
                tb.SelectedText = string.Empty;
                Core.App.ClosePopup();
            };
            MenuItem copyItem = new MenuItem()
            {
                Header = "Copy",
                IsEnabled = !string.IsNullOrEmpty(tb.SelectedText)
            };
            copyItem.Click += delegate(object senderMenuItem, RoutedEventArgs menuItemClickEventArgs)
            {
                try
                {
                    Clipboard.SetText(tb.SelectedText);
                }
                catch (System.Security.SecurityException)
                {
                    // Will be thrown if the user denies the app access to the clipboard. There's not 
                    // much we can do in this case. We just need to make sure the popup menu is hidden
                    Core.App.ClosePopup();
                    return;
                }
                Core.App.ClosePopup();
            };
            MenuItem pasteItem = new MenuItem()
            {
                Header = "Paste",
                IsEnabled = Clipboard.ContainsText()
            };
            pasteItem.Click += delegate(object senderMenuItem, RoutedEventArgs menuItemClickEventArgs)
            {
                try
                {
                    tb.SelectedText = Clipboard.GetText();
                }
                catch (System.Security.SecurityException)
                {
                    // Will be thrown if the user denies the app access to the clipboard. There's not 
                    // much we can do in this case. We just need to make sure the popup menu is hidden
                    Core.App.ClosePopup();
                    return;
                }
                Core.App.ClosePopup();
            };

            // Build the context menu from these items
            ContextMenu cm = new ContextMenu();
            cm.Items.Add(cutItem);
            cm.Items.Add(copyItem);
            cm.Items.Add(pasteItem);

            // Create the popup menu
            Popup popup = new Popup();
            popup.Child = cm;
            // Show it
            LaunchPopup(popup, e);
        }

        /// <summary>
        /// Attempts to parse a string of the form "#AARRGGBB" and build a color 
        /// value from it. On failure, the color is set to white and false is 
        /// returned.
        /// </summary>
        public static bool TryParseColor(string argb, out Color color)
        {
            // We're expecting strings of this form for the color:
            //  #AARRGGBB
            
            if (!argb.StartsWith("#") || 9 != argb.Length)
            {
                color = Colors.White;
                return false;
            }

            byte a = Convert.ToByte(argb.Substring(1, 2), 16);
            byte r = Convert.ToByte(argb.Substring(3, 2), 16);
            byte g = Convert.ToByte(argb.Substring(5, 2), 16);
            byte b = Convert.ToByte(argb.Substring(7, 2), 16);
            color = Color.FromArgb(a, r, g, b);
            return true;
        }

        /// <summary>
        /// Attempts to parse a string of the form "X,Y" and build a point 
        /// value from it. On failure, the point is set to 0,0 and false 
        /// is returned.
        /// </summary>
        public static bool TryParsePoint(string pointString, out Point point)
        {
            // The expected format of the string is "X,Y"
            if (null == pointString || !pointString.Contains(","))
            {
                point = new Point(0.0, 0.0);
                return false;
            }

            string[] components = pointString.Split(',');
            if (null == components || components.Length < 2)
            {
                point = new Point(0.0, 0.0);
                return false;
            }

            double x, y;
            if (double.TryParse(components[0], out x) && double.TryParse(components[1], out y))
            {
                point = new Point(x, y);
                return true;
            }

            point = new Point(0.0, 0.0);
            return false;
        }

        public static ChemProV.UI.WorkspaceControl Workspace
        {
            get
            {
                return s_workspace;
            }
        }

        public enum LogItemType
        {
            Info,
            Warning,
            Error
        }

        private class LogItem
        {
            private LogItemType m_type;

            private string m_message;

            public LogItem(LogItemType type, string message)
            {
                m_type = type;
                m_message = message;
            }

            public string Message
            {
                get
                {
                    return m_message;
                }
            }

            public LogItemType Type
            {
                get
                {
                    return m_type;
                }
            }
        }
    }
}
