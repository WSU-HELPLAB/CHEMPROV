using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ChemProV.Library.OsbleService;
using ChemProV.Library.ServiceReference1;
using System.Threading;
using System.Collections.ObjectModel;

//namespace ChemProV.Logic.OSBLE
namespace ChemProV.Library.OSBLE
{
    public class RelevantAssignment
    {
        /// <summary>
        /// Reference to the OSBLE assignment object. This class serves as a way to wrap around 
        /// this object and provide data in a more meaningful and logical way.
        /// </summary>
        private Assignment m_a = null;

        private System.ServiceModel.BasicHttpBinding m_bind;

        private List<AssignmentStream> m_files = new List<AssignmentStream>();

        private int m_getFilesThreadID = -1;

        private bool m_gettingFiles = false;

        private string m_password;

        private string m_userName;

        public RelevantAssignment(Assignment assignment, string userName, string password)
        {
            m_a = assignment;
            m_userName = userName;
            m_password = password;

            m_bind = new System.ServiceModel.BasicHttpBinding();
            m_bind.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.None;
            m_bind.ReceiveTimeout = new TimeSpan(0, 0, 10);
            m_bind.SendTimeout = new TimeSpan(0, 0, 10);
            m_bind.MaxBufferSize = 2147483647;
            m_bind.MaxReceivedMessageSize = 2147483647;
        }

        private void AddFromZip(byte[] zipFileData)
        {
            // Make a memory stream for the byte array
            MemoryStream ms = new MemoryStream(zipFileData);

            // Create the zip file
            ICSharpCode.SharpZipLib.Zip.ZipFile zf;
            try
            {
                zf = new ICSharpCode.SharpZipLib.Zip.ZipFile(ms);
            }
            catch (Exception)
            {
                return;
            }

            // Go through the files within the zip
            foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry ze in zf)
            {
                // Read the whole thing into a memory stream
                AssignmentStream msUncompressed;
                using (Stream tempStream = zf.GetInputStream(ze))
                {
                    msUncompressed = new AssignmentStream(ze.Name, this);
                    tempStream.CopyTo(msUncompressed);
                }

                // Add it to the list
                m_files.Add(msUncompressed);
            }

            ms.Dispose();
        }

        private void AuthenticationCompleted(object sender, ValidateUserCompletedEventArgs e)
        {
            object[] args = e.UserState as object[];
            AuthenticationServiceClient authClient = args[0] as AuthenticationServiceClient;
            
            // If we failed then there's not much we can do
            if (null != e.Error || e.Cancelled)
            {
                m_gettingFiles = false;
                
                // Invoke the completion delegate
                (args[1] as EventHandler)(this, new RelevantAssignmentEventArgs(m_files));
                
                return;
            }

            string authToken = e.Result;

            // We're done with the authentication client
            authClient.CloseAsync();

            OsbleServiceClient osc = new OsbleServiceClient(m_bind,
                new System.ServiceModel.EndpointAddress(OSBLEState.OSBLEServiceLink));
            
            // What we query for depends on the assignment type
            args[0] = osc;
            if (AssignmentTypes.CriticalReview == m_a.Type)
            {
                // We need to use "GetReviewItems"
                osc.GetReviewItemsCompleted += new EventHandler<GetReviewItemsCompletedEventArgs>(GetReviewItemsCompleted);
                osc.GetReviewItemsAsync(m_a.ID, authToken, args);
            }
            else
            {
                // We need to use "GetAssignmentSubmission"
                osc.GetAssignmentSubmissionCompleted += new EventHandler<GetAssignmentSubmissionCompletedEventArgs>(GetAssignmentSubmissionCompleted);
                osc.GetAssignmentSubmissionAsync(m_a.ID, authToken, args);
            }
        }

        public string CourseName
        {
            get
            {
                return m_a.Course.Name;
            }
        }

        public ObservableCollection<Deliverable> Deliverables
        {
            get
            {
                return m_a.Deliverables;
            }
        }

        private void GetAssignmentSubmissionCompleted(object sender, GetAssignmentSubmissionCompletedEventArgs e)
        {
            object[] args = e.UserState as object[];
            OsbleServiceClient client = args[0] as OsbleServiceClient;
            EventHandler onCompletion = args[1] as EventHandler;

            if (null == e.Error && !e.Cancelled)
            {
                AddFromZip(e.Result);
            }

            // Close the OSBLE client
            client.CloseAsync();

            m_gettingFiles = false;

            onCompletion(this, new RelevantAssignmentEventArgs(m_files));
        }

        /// <summary>
        /// Gets the collection of assignment files (as streams).
        /// </summary>
        public void GetFilesAsync(EventHandler onCompletion)
        {
            if (m_gettingFiles)
            {
                return;
            }
            m_gettingFiles = true;

            // Start by clearing the list
            foreach (AssignmentStream stream in m_files)
            {
                stream.Dispose();
            }
            m_files.Clear();

            // Do the refresh on a new thread
            ParameterizedThreadStart pts = new ParameterizedThreadStart(this.RefreshThreadProc);
            Thread t = new Thread(pts);
            t.Start(onCompletion);
        }

        private void GetReviewItemsCompleted(object sender, GetReviewItemsCompletedEventArgs e)
        {
            object[] args = e.UserState as object[];
            OsbleServiceClient client = args[0] as OsbleServiceClient;
            EventHandler onCompletion = args[1] as EventHandler;

            if (null == e.Error && !e.Cancelled)
            {
                AddFromZip(e.Result);
            }

            client.CloseAsync();
            m_gettingFiles = false;
            onCompletion(this, new RelevantAssignmentEventArgs(m_files));
        }

        public int ID
        {
            get
            {
                return m_a.ID;
            }
        }

        public string Name
        {
            get
            {
                return m_a.AssignmentName;
            }
        }

        private void RefreshThreadProc(object parameter)
        {
            // The parameter is the event handler for completion
            //EventHandler onCompletion = parameter as EventHandler;
            
            AuthenticationServiceClient auth = new AuthenticationServiceClient(
                m_bind, new System.ServiceModel.EndpointAddress(OSBLEState.AuthServiceLink));
            auth.ValidateUserCompleted += this.AuthenticationCompleted;
            auth.ValidateUserAsync(m_userName, m_password, new object[] { auth, parameter });
        }

#if DEBUG
        public static string ReadStreamAsString(Stream s)
        {
            if (null == s || 0 == s.Length)
            {
                return string.Empty;
            }

            long posBackup = s.Position;
            s.Position = 0;
            byte[] data = new byte[s.Length];
            s.Read(data, 0, data.Length);
            s.Position = posBackup;

            return System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
        }
#endif

        public class AssignmentStream : MemoryStream
        {
            private string m_name;

            private RelevantAssignment m_parent;
            
            public AssignmentStream(string name, RelevantAssignment parent)
                : base()
            {
                m_name = name;
                m_parent = parent;
            }

            public string Name
            {
                get
                {
                    return m_name;
                }
            }

            public RelevantAssignment Parent
            {
                get
                {
                    return m_parent;
                }
            }
        }

        public class RelevantAssignmentEventArgs : EventArgs
        {
            private IList<AssignmentStream> m_streams;
            
            public RelevantAssignmentEventArgs(IList<AssignmentStream> streams)
            {
                m_streams = streams;
            }

            public IList<AssignmentStream> Files
            {
                get
                {
                    return m_streams;
                }
            }
        }
    }
}
