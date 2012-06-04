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

        private static ChemProV.UI.ControlPalette s_palette = null;
        
        private static ChemProV.UI.WorkspaceControl s_workspace = null;

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

        public static OptionDifficultySetting DifficultySetting
        {
            get
            {
                return s_workspace.CurrentDifficultySetting;
            }
        }

        /// <summary>
        /// This must be called upon application initialization
        /// </summary>
        public static void Init(ChemProV.UI.WorkspaceControl workspace, ChemProV.UI.ControlPalette palette)
        {
            s_workspace = workspace;
            s_palette = palette;
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
