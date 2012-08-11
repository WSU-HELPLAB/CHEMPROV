/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;

namespace ChemProV.Logic.Undos
{
    public class SetProcessUnitLocation : IUndoRedoAction
    {
        private AbstractProcessUnit m_apu;

        private MathCore.Vector m_location;

        public SetProcessUnitLocation(AbstractProcessUnit processUnit, MathCore.Vector location)
        {
            m_apu = processUnit;
            m_location = location;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            MathCore.Vector current = m_apu.Location;
            m_apu.Location = m_location;
            return new SetProcessUnitLocation(m_apu, current);
        }
    }
}
