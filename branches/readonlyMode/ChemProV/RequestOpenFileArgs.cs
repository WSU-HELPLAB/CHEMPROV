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
