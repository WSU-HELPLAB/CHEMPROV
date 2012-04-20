/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System.Windows.Controls;
using System.Windows.Shapes;

using ChemProV.PFD.Streams.PropertiesWindow.Chemical;

namespace ChemProV.PFD.Streams
{
    public class ChemicalStream : AbstractStream
    {
        public ChemicalStream()
            : base()
        {
        }

        public override bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty)
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

        public override string Title
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