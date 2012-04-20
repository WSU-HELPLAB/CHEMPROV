/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

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
using System.Reflection;
using System.Threading;
using ChemProV.PFD.Streams;

namespace ChemProV.UI.DrawingCanvas.States
{
    public class PlacingNewObject : IState
    {
        private DrawingCanvas m_canvas;

        /// <summary>
        /// This is the icon that will appear on the canvas before we left click to 
        /// actually finalize the creation. It is initialized in the constructor.
        /// </summary>
        private Image m_icon = null;

        /// <summary>
        /// Primarily for debugging. Stores the ID of the thread that instantiated the object
        /// </summary>
        private int m_instThreadID;

        private object m_newObject = null;

        private UI.ControlPalette m_palette;

        /// <summary>
        /// As we drag stream source and destination connectors over various process units, we 
        /// may change their border colors. If this value is non-null, then it references a 
        /// process unit whose border color has been changed and must be changed back when we 
        /// complete the placing action or move the mouse out of its area.
        /// </summary>
        private PFD.ProcessUnits.GenericProcessUnit m_puWithAlteredBorderColor = null;

        private object m_typeOrEnum;

        public PlacingNewObject(UI.ControlPalette sender, DrawingCanvas canvas, object objectTypeOrEnum)
        {
            m_palette = sender;
            m_canvas = canvas;
            m_typeOrEnum = objectTypeOrEnum;
            m_instThreadID = Thread.CurrentThread.ManagedThreadId;

            m_canvas.SelectedElement = null;
        }

        private void Init()
        {
            // Create an instance of the object
            if (m_typeOrEnum is PFD.ProcessUnits.ProcessUnitType)
            {
                PFD.ProcessUnits.ProcessUnitType put = (PFD.ProcessUnits.ProcessUnitType)m_typeOrEnum;
                m_newObject = PFD.ProcessUnits.ProcessUnitFactory.ProcessUnitFromUnitType(put);
            }
            else if (m_typeOrEnum is Type)
            {
                Type t = m_typeOrEnum as Type;
                m_newObject = Activator.CreateInstance(t);
            }
            else
            {
                throw new Exception("Unknown type or enumeration: " + m_typeOrEnum.ToString());
            }

            // The object that was just created should be one of these three:
            IStream stream = m_newObject as IStream;
            PFD.ProcessUnits.IProcessUnit pu = m_newObject as PFD.ProcessUnits.IProcessUnit;
            PFD.StickyNote.StickyNote sn = m_newObject as PFD.StickyNote.StickyNote;

            // Initialize an appropriate icon image and add it to the canvas
            m_icon = new Image();
            if (null != stream)
            {
                //Uri uri = new Uri(StreamFactory.IconFromStreamType(StreamFactory.StreamTypeFromStream(stream)), UriKind.Relative);

                // For now we'll use the stream source icon
                Uri uri = new Uri("/UI/Icons/pu_source.png", UriKind.Relative);

                ImageSource img = new System.Windows.Media.Imaging.BitmapImage(uri);
                m_icon.SetValue(Image.SourceProperty, img);
            }
            else if (null != pu)
            {
                // If it's a process unit, we just want to use its actual icon
                // Make sure we make a copy of it
                m_icon = new Image();
                m_icon.Source = pu.Icon.Source;
            }
            else if (null != sn)
            {
                // Use the little sticky note icon
                Uri uri = new Uri("/UI/Icons/palette_stickyNote.png", UriKind.Relative);
                ImageSource img = new System.Windows.Media.Imaging.BitmapImage(uri);
                m_icon.SetValue(Image.SourceProperty, img);
            }
            else
            {
                // Coming here implies that we don't know what the object is
                // Thus, throw an exception
                if (null == m_newObject)
                {
                    throw new Exception("Can't initialize placement of unknown object");
                }
                else
                {
                    throw new Exception("Can't initialize placement of unknown object: " +
                        m_newObject.ToString());
                }
            }

            // Set up the mouse events for the image
            m_icon.MouseEnter += this.MouseEnter;
            m_icon.MouseLeave += this.MouseLeave;
            m_icon.MouseLeftButtonDown += this.MouseLeftButtonDown;
            m_icon.MouseLeftButtonUp += this.MouseLeftButtonUp;
            m_icon.MouseMove += this.MouseMove;
            m_icon.MouseRightButtonDown += this.MouseRightButtonDown;
            m_icon.MouseRightButtonUp += this.MouseRightButtonUp;
            m_icon.MouseWheel += this.MouseWheel;
            m_icon.LostMouseCapture += this.LostMouseCapture;

            // Add the icon image to the canvas. When the time comes to place the real object, 
            // it will have to be removed.
            if (null != m_icon.Parent)
            {
                // If a parent was auto-assigned, we want to remove it so that we can add it as 
                // a child of the canvas
                ((Panel)m_icon.Parent).Children.Remove(m_icon);
            }
            m_icon.SetValue(Canvas.ZIndexProperty, 3);
            m_canvas.Children.Add(m_icon);
            // Give it a default position
            m_icon.SetValue(Canvas.LeftProperty, 0.0);
            m_icon.SetValue(Canvas.TopProperty, 0.0);
        }

        #region IState members

        public void MouseEnter(object sender, MouseEventArgs e)
        {
            // Nothing needed here
        }

        public void MouseLeave(object sender, MouseEventArgs e)
        {
            // Nothing needed here
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            // If m_newObject is null then we need to intialize
            if (null == m_newObject)
            {
                Init();
            }

            // When the mouse is moved, we want to reposition the icon on the drawing canvas
            Point mousePt = e.GetPosition(m_canvas);
            m_icon.SetValue(Canvas.LeftProperty, mousePt.X - m_icon.ActualWidth / 2.0);
            m_icon.SetValue(Canvas.TopProperty, mousePt.Y - m_icon.ActualHeight / 2.0);

            // Clear the border if we'd previously set one
            if (null != m_puWithAlteredBorderColor)
            {
                m_puWithAlteredBorderColor.SetBorderColor(PFD.ProcessUnits.ProcessUnitBorderColor.NoBorder);
                m_puWithAlteredBorderColor = null;
            }

            if (!(m_newObject is IStream))
            {
                return;
            }

            // If the object we're placing is a stream, we want to check if we've hovering 
            // over a process unit. If so, the unit should be highlighted in green if we 
            // can make the connection, or highlighted in red if we cannot.
            IStream s = m_newObject as IStream;
            object element = m_canvas.GetChildAt(e.GetPosition(m_canvas));
            if (null == element)
            {
                // We're not hovering over anything, so there's nothing to do
                return;
            }

            // See if what we're hovering over is a processing unit. If not, we can return
            PFD.ProcessUnits.GenericProcessUnit pu = element as PFD.ProcessUnits.GenericProcessUnit;
            if (null == pu)
            {
                return;
            }

            // See if the unit is a valid source and set the border color appropriately
            pu.SetBorderColor(s.IsValidSource(pu) ?
                PFD.ProcessUnits.ProcessUnitBorderColor.AcceptingStreams :
                PFD.ProcessUnits.ProcessUnitBorderColor.NotAcceptingStreams);
            m_puWithAlteredBorderColor = pu;
        }

        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point location = e.GetPosition(m_canvas);
            UIElement uie = null;
            double w = 32.0, h = 32.0;

            // Regardless of what's about to happen, we will always need to remove the 
            // placement icon, so we can start with that
            m_canvas.Children.Remove(m_icon);

            // There are a few possibilities for a mouse-down event. The first is that 
            // we're placing a trivial item (which as of now is pretty much everything but 
            // streams). Handle this case first
            if (!(m_newObject is IStream))
            {
                // Add the new object to the canvas
                uie = (UIElement)m_newObject;
                m_canvas.Children.Add(uie);
                if (m_newObject is UserControl)
                {
                    w = ((UserControl)m_newObject).DesiredSize.Width;
                    h = ((UserControl)m_newObject).DesiredSize.Height;
                }
                uie.SetValue(Canvas.LeftProperty, location.X - w / 2.0);
                uie.SetValue(Canvas.TopProperty, location.Y - h / 2.0);

                // Create an undo action that will remove the object
                m_canvas.AddUndo(new PFD.UndoRedoCollection(
                    new PFD.Undos.RemoveFromCanvas(uie, m_canvas)));

                // Tell the palette to go back to select mode
                m_palette.SwitchToSelect();

                // Setup mouse events
                uie.MouseEnter += m_canvas.NullState.MouseEnter;
                uie.MouseLeave += m_canvas.NullState.MouseLeave;
                uie.MouseLeftButtonDown += m_canvas.NullState.MouseLeftButtonDown;
                uie.MouseLeftButtonUp += m_canvas.NullState.MouseLeftButtonUp;
                uie.MouseMove += m_canvas.NullState.MouseMove;
                uie.MouseRightButtonDown += m_canvas.NullState.MouseRightButtonDown;
                uie.MouseRightButtonUp += m_canvas.NullState.MouseRightButtonUp;
                uie.MouseWheel += m_canvas.NullState.MouseWheel;

                // That's all
                return;
            }

            // Get a reference to the new stream
            IStream s = m_newObject as IStream;

            // If the user clicked somewhere where we cannot connect to, cancel 
            // the placement action and give the user a message.
            uie = m_canvas.GetChildAt(location);
            if (null != uie && uie is PFD.ProcessUnits.IProcessUnit)
            {
                if (!s.IsValidSource(uie as PFD.ProcessUnits.IProcessUnit))
                {
                    // Tell the palette to go back to select mode. This cancels the 
                    // placement of the object.
                    m_palette.SwitchToSelect();

                    // Show a message to the user
                    Core.App.MessageBox("The process unit that you clicked cannot be used " +
                        "as a source for the type of stream you have selected.");

                    return;
                }
            }

            // Now we have two cases. The first is that we just clicked on a process unit that we 
            // need to connect the stream to and the second is that we didn't. Handle the second 
            // case first.
            if (null == uie || !(uie is PFD.ProcessUnits.IProcessUnit))
            {
                // TODO: Just place the stream and return
            }

            uie = (UIElement)m_newObject;
            m_canvas.Children.Add(uie);
            if (m_newObject is UserControl)
            {
                w = ((UserControl)m_newObject).ActualWidth;
                h = ((UserControl)m_newObject).ActualHeight;
            }
            uie.SetValue(Canvas.LeftProperty, location.X - w / 2.0);
            uie.SetValue(Canvas.TopProperty, location.Y - h / 2.0);

            // Create an undo action that will remove the object
            m_canvas.AddUndo(new PFD.UndoRedoCollection(
                new PFD.Undos.RemoveFromCanvas(uie, m_canvas)));

            // Tell the palette to go back to select mode
            m_palette.SwitchToSelect();
        }

        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Nothing needed here
        }

        public void MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // For now, we do nothing here. I would suggest that a right mouse button 
            // down would cancel the action and flip back to "Select" mode, but this 
            // can be discussed in the future.
        }

        public void MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Nothing needed here
        }

        public void MouseWheel(object sender, MouseEventArgs e)
        {
            // Nothing needed here
        }

        public void LostMouseCapture(object sender, MouseEventArgs e)
        {
            // Not sure about this one
            // TODO: Check on this
        }

        #endregion
    }
}
