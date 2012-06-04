/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System.Windows;
using System.Windows.Media;
using ChemProV.UI.DrawingCanvas;

namespace ChemProV.PFD.Streams
{
    public class HeatStream : AbstractStream
    {
        private Brush m_streamLineNotSelected;
        
        public HeatStream()
            : this(Core.App.Workspace.DrawingCanvas)
        {
        }

        public HeatStream(DrawingCanvas canvas)
            : base(canvas)
        {
            m_streamLineNotSelected = new SolidColorBrush(Colors.Red);
            this.Stem.Stroke = m_streamLineNotSelected;
        }

        protected override void CreatePropertiesTable()
        {
            m_table = PropertiesWindow.PropertiesWindowFactory.TableFromStreamType(
                StreamType.Heat, m_canvas.CurrentDifficultySetting, false);

            m_table.ParentStream = this;
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
            // Heat streams are only available with MaterialAndEnergyBalance
            return (OptionDifficultySetting.MaterialAndEnergyBalance == difficulty);
        }

        public override bool IsValidSource(ProcessUnits.IProcessUnit unit)
        {
            return false;
        }

        public override bool IsValidDestination(ProcessUnits.IProcessUnit unit)
        {
            // E.O.
            // Heat streams can only have reactors as destinations (and of course the 
            // reactor unit has to be accepting incoming streams).
            // TODO: Check with the chemistry guys to verify this
            return ((unit is PFD.ProcessUnits.Reactor) &&
                unit.IsAcceptingIncomingStreams(this));
        }

        public override Brush StreamLineNotSelected
        {
            get { return m_streamLineNotSelected; }
        }

        /// <summary>
        /// Every stream MUST have this static method or an exception will be thrown upon 
        /// initialization at runtime.
        /// </summary>
        public static string Title
        {
            get
            {
                return "Heat Stream";
            }
        }
    }
}