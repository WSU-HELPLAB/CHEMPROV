﻿/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

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
using ChemProV.Core;

namespace ChemProV.PFD.Undos
{
    public class RemoveComment :IUndoRedoAction
    {
        private int m_index;
        
        private ICommentCollection m_owner;

        public RemoveComment(ICommentCollection owner, int index)
        {
            m_owner = owner;
            m_index = index;
        }

        public IUndoRedoAction Execute(UndoRedoExecutionParameters parameters)
        {
            IComment current = m_owner.GetCommentAt(m_index);
            m_owner.RemoveCommentAt(m_index);
            return new InsertComment(m_owner, current, m_index);
        }
    }
}