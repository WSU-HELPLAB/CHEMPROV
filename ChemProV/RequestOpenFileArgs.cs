using System;
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