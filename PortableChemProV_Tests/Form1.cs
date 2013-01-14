using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace PortableChemProV_Tests
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnBrowseChild_CM_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = btnBrowseChild_CM.Tag as OpenFileDialog;
            if (null == ofd)
            {
                // Create it
                ofd = new OpenFileDialog();
                btnBrowseChild_CM.Tag = ofd;
                ofd.Filter = "CPML Files|*.cpml";
            }

            // Show it
            if (ofd.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            // Put the file name in the text box
            tbChildSource_CM.Text = ofd.FileName;
        }

        private void btnBrowseParent_CM_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = btnBrowseParent_CM.Tag as OpenFileDialog;
            if (null == ofd)
            {
                // Create it
                ofd = new OpenFileDialog();
                btnBrowseParent_CM.Tag = ofd;
                ofd.Filter = "CPML Files|*.cpml";
            }

            // Show it
            if (ofd.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            // Put the file name in the text box
            tbParentSource_CM.Text = ofd.FileName;
        }

        private void btnBrowseOutput_CM_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = btnBrowseParent_CM.Tag as SaveFileDialog;
            if (null == sfd)
            {
                // Create the dialog
                sfd = new SaveFileDialog();
                btnBrowseParent_CM.Tag = sfd;
                sfd.Filter = "CPML Files|*.cpml";
                sfd.DefaultExt = "cpml";
                sfd.AddExtension = true;
            }

            // Show it
            if (sfd.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            // Put the file name in the text box
            tbOutput_CM.Text = sfd.FileName;
        }

        private void btnGo_CM_Click(object sender, EventArgs e)
        {
            // Open the two input files
            FileStream fsParent, fsChild;
            try
            {
                fsParent = new FileStream(tbParentSource_CM.Text, FileMode.Open, FileAccess.Read);
                fsChild = new FileStream(tbChildSource_CM.Text, FileMode.Open, FileAccess.Read);
            }
            catch (Exception)
            {
                MessageBox.Show(this, "Could not open input file(s)");
                return;
            }

            // Read them into memory. We do this in case the output file is going to overwrite one of the 
            // two, which we want to allow, but can't if we have the file open for reading.
            MemoryStream msParent = FileStreamToMemoryStream(fsParent);
            MemoryStream msChild = FileStreamToMemoryStream(fsChild);

            // Close the input files
            fsParent.Dispose();
            fsChild.Dispose();
            fsParent = fsChild = null;

            // Open the output file
            FileStream fsOutput = new FileStream(tbOutput_CM.Text, FileMode.Create, FileAccess.ReadWrite);

            // Have the library do the merge
            ChemProV.Core.CommentMerger.Merge(
                msParent, tbParentUserName_CM.Text, // Parent
                msChild, tbChildUserName_CM.Text, // Child
                fsOutput);

            // Close the output file
            fsOutput.Dispose();

            MessageBox.Show(this, "Merge complete");
        }

        private static MemoryStream FileStreamToMemoryStream(FileStream fs)
        {
            byte[] buf = new byte[fs.Length];
            int count = fs.Read(buf, 0, buf.Length);
            return new MemoryStream(buf);
        }
    }
}
