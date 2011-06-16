using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;

namespace ChemProV
{
    public partial class ChemProVHolder : UserControl
    {
        public ChemProVHolder()
        {
            InitializeComponent();

            AddNewMainPage();

        }

        void RequestOpenFile(object sender, RequestOpenFileArgs e)
        {
            RemoveOldMainPage(sender as MainPage);
            AddNewMainPage(e.fileInfo);
        }

        void newBlankChemProV(object sender, EventArgs e)
        {
            RemoveOldMainPage(sender as MainPage);
            AddNewMainPage();
        }

        private void RemoveOldMainPage(MainPage mp)
        {
            mp.RequestNewBlankMainPage -= new EventHandler(newBlankChemProV);
            mp.RequestOpenFile -= new RequestOpenFileEventHandler(RequestOpenFile);
            this.LayoutRoot.Children.Remove(mp);
            GC.Collect();
        }

        private void AddNewMainPage(FileInfo fileInfo = null)
        {
            MainPage mp = new MainPage(fileInfo);

            mp.RequestNewBlankMainPage += new EventHandler(newBlankChemProV);
            mp.RequestOpenFile += new RequestOpenFileEventHandler(RequestOpenFile);

            this.LayoutRoot.Children.Add(mp);
        }

    }
}
