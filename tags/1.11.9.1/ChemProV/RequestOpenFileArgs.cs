/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/
﻿using System;
using System.IO;

namespace ChemProV
{
    public delegate void RequestOpenFileEventHandler(object sender, RequestOpenFileArgs e);

    public class RequestOpenFileArgs : EventArgs
    {
        public readonly FileInfo fileInfo;

        public RequestOpenFileArgs(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
        }

        public RequestOpenFileArgs()
        {
            this.fileInfo = null;
        }
    }
}