/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

using ChemProV.UI.DrawingCanvas;
using ChemProV.PFD.Streams.PropertiesWindow.Chemical;

namespace ChemProV.PFD.Streams
{
    public class ChemicalStream : AbstractStream
    {
        public ChemicalStream()
            : this(null, new Point())
        {
        }

        public ChemicalStream(DrawingCanvas canvas, Point locationOnCanvas)
            : base(canvas, locationOnCanvas)
        {
        }

        protected override void CreatePropertiesTable()
        {
            // Use the factory to create it
            m_table = ChemicalStreamPropertiesTableFactory.GetChemicalStreamPropertiesTable(
                m_canvas.CurrentDifficultySetting, false, this);
        }

        /// <summary>
        /// Every stream MUST have this static method. I wish I could enforce it at compile-time 
        /// but the only way to do that would be to make it an abstract function in the base 
        /// class, in which case it would be a member function and not a static function. It 
        /// needs to be static so it can be queried without creating an instance.
        /// A runtime error will occur if this method is not present in any class that inherits 
        /// from AbstractStream. This logic exists in the control palette code.
        /// </summary>
        public static bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty)
        {
            // Chemical streams are available with all difficulties
            return true;
        }

        public override bool IsValidSource(ProcessUnits.IProcessUnit unit)
        {
            // E.O.
            // Chemical streams can accept any type of process unit as a source, provided 
            // that the unit is accepting outgoing streams.
            // TODO: Double check this with the chemistry guys
            return unit.IsAcceptingOutgoingStreams(this);
        }

        public override bool IsValidDestination(ProcessUnits.IProcessUnit unit)
        {
            // E.O.
            // As far as I can tell, the chemical streams can accept any type of process 
            // unit as a destination. So the only required check is that the unit is 
            // accepting incoming streams.
            // TODO: Double check this with the chemistry guys
            return unit.IsAcceptingIncomingStreams(this);
        }

        /// <summary>
        /// Every stream MUST have this static method or an exception will be thrown upon 
        /// initialization at runtime.
        /// </summary>
        public static string Title
        {
            get
            {
                return "Chemical Stream";
            }
        }

        public override void UpdateStreamLocation()
        {
            base.UpdateStreamLocation();
            ChemicalStreamPropertiesWindow ctable = Table as ChemicalStreamPropertiesWindow;

            if (ctable != null && ctable.View == PropertiesWindow.View.Collapsed)
            {
                //since the ChemicalSterampropertiesWindow is Collapsed we will move it with the stream.
                Line steamsTableLine = this.TableLine;
                ctable.SetValue(Canvas.LeftProperty, steamsTableLine.X1);
                ctable.SetValue(Canvas.TopProperty, steamsTableLine.Y1);
                this.TableLine.X2 = this.TableLine.X1;
                this.TableLine.Y2 = this.TableLine.Y1;
            }
        }
    }
}