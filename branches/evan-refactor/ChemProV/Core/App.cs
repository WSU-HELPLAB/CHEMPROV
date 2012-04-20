/*
Copyright 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds
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

namespace ChemProV.Core
{
    /// <summary>
    /// Static class to contain global properties and methods that are relevant to the application
    /// </summary>
    public static class App
    {
        private static ChemProV.UI.WorkSpace s_workspace = null;

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
        public static void Init(ChemProV.UI.WorkSpace workspace)
        {
            s_workspace = workspace;
        }

        public static void MessageBox(string message)
        {
            System.Windows.MessageBox.Show(message);
        }

        public static ChemProV.UI.WorkSpace Workspace
        {
            get
            {
                return s_workspace;
            }
        }
    }
}
