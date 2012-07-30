using ICSharpCode.SharpZipLib.Zip;
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

namespace ChemProV.Logic.OSBLE
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

        private OsbleServiceClient m_osbleClient = null;

        private string m_password;

        /// <summary>
        /// ID of the logged in user. This is retrieved after authentication.
        /// </summary>
        private int m_userID = -1;

        private string m_userName;

        public RelevantAssignment(Assignment assignment, string userName, string password)
        {
            m_a = assignment;
            m_userName = userName;
            m_password = password;

            m_bind = new System.ServiceModel.BasicHttpBinding();
            m_bind.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.None;
            m_bind.ReceiveTimeout = new TimeSpan(0, 0, 15);
            m_bind.SendTimeout = new TimeSpan(0, 0, 15);
            m_bind.MaxBufferSize = 2147483647;
            m_bind.MaxReceivedMessageSize = 2147483647;
        }

        private bool AddFromZip(byte[] zipFileData)
        {
            return AddFromZip(zipFileData, string.Empty);
        }

        private bool AddFromZip(byte[] zipFileData, string namePrefix)
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
                return false;
            }

            // Go through the files within the zip
            foreach (ZipEntry ze in zf)
            {
                string name = namePrefix;
                if (!string.IsNullOrEmpty(namePrefix))
                {
                    name = (name + " - ");
                }
                name += System.IO.Path.GetFileName(ZipEntry.CleanName(ze.Name));
                
                // Read the whole thing into a memory stream
                AssignmentStream msUncompressed;
                using (Stream tempStream = zf.GetInputStream(ze))
                {
                    msUncompressed = new AssignmentStream(name, this);
                    tempStream.CopyTo(msUncompressed);
                }

                // There might be zips within the zip
                if (!AddFromZip(msUncompressed.ToArray(), ZipEntry.CleanName(ze.Name)))
                {
                    // This implies failure to read the uncompressed stream as another zip file. So the 
                    // stream we have should be added to the list.
                    m_files.Add(msUncompressed);
                }
            }

            ms.Dispose();
            return true;
        }

        private void AuthClient_GetActiveUserCompleted(object sender, GetActiveUserCompletedEventArgs e)
        {
            // The authentication client has delivered what we need and so we can close it
            AuthenticationServiceClient authClient = e.UserState as AuthenticationServiceClient;
            authClient.CloseAsync();

            if (null == e.Error)
            {
                m_userID = e.Result.ID;
            }
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

            // The last thing we do with the authentication client is get the active user
            authClient.GetActiveUserCompleted += new EventHandler<GetActiveUserCompletedEventArgs>(AuthClient_GetActiveUserCompleted);
            authClient.GetActiveUserAsync(authToken, authClient);

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

        private void AuthForSaveCompleted(object sender, ValidateUserCompletedEventArgs e)
        {
            object[] args = e.UserState as object[];
            AuthenticationServiceClient authClient = args[0] as AuthenticationServiceClient;
            EventHandler onSaveComplete = args[1] as EventHandler;
            byte[] zippedData = args[2] as byte[];

            // If we failed then there's not much we can do
            if (null != e.Error || e.Cancelled)
            {
                // Invoke the completion delegate and return
                onSaveComplete(this, new SaveEventArgs(false));
                return;
            }

            string authToken = e.Result;

            // Build the OSBLE service client
            m_osbleClient = new OsbleServiceClient(m_bind,
                new System.ServiceModel.EndpointAddress(OSBLEState.OSBLEServiceLink));
            m_osbleClient.OpenCompleted += delegate(object sender2, System.ComponentModel.AsyncCompletedEventArgs e2)
            {

                // How we save depends on the assignment type
                if (AssignmentTypes.CriticalReview == m_a.Type)
                {
                    // For critical reviews we use SubmitReviewAsync. According to the OSBLE team, when you call this 
                    // you give it a zip file that contains a single file that is the review document. Recall that when 
                    // we retrieve the files for a review assignment it's a zip of zips, and each file to review is 
                    // within its own "child" zip under the main "parent" zip. This is NOT something we have to deal 
                    // with here. We just submit the review file and we're done.
                    m_osbleClient.SubmitReviewCompleted += new EventHandler<SubmitReviewCompletedEventArgs>(OSBLESubmitReviewCompleted);
                    m_osbleClient.SubmitReviewAsync(m_userID, m_a.ID, zippedData, authToken, new object[] { m_osbleClient, onSaveComplete });
                }
                else
                {
                    // It's assumed that the assignment type is "Basic" if we come here
                    // For basic assignments we just use the SubmitAssignmentAsync method
                    m_osbleClient.SubmitAssignmentCompleted += this.OSBLESubmitAssignmentCompleted;
                    m_osbleClient.SubmitAssignmentAsync(m_a.ID, zippedData, authToken, new object[] { m_osbleClient, onSaveComplete });
                }
            };
            m_osbleClient.OpenAsync();

            // We're done with the authentication client
            authClient.CloseAsync();
        }

        public string CourseName
        {
            get
            {
                return m_a.Course.Name;
            }
        }

        /// <summary>
        /// Creates a zip file that contains a single file within it. The data for the incoming file 
        /// as well as the data for the created zip files are byte arrays.
        /// </summary>
        public static byte[] CreateZipFile(byte[] fileData, string fileName)
        {
            // Create a memory stream to save the zip file to
            MemoryStream ms = new MemoryStream();

            // Create the zip output stream with the maximum level of compression
            ZipOutputStream zos = new ZipOutputStream(ms);
            zos.SetLevel(9);

            // Create the entry for this file
            ZipEntry newEntry = new ZipEntry(ZipEntry.CleanName(fileName));
            newEntry.DateTime = DateTime.Now;
            zos.PutNextEntry(newEntry);
            zos.Write(fileData, 0, fileData.Length);
            zos.CloseEntry();

            // Flush and finish the zip file. This must be done to complete the zip file creation 
            // in the memory stream.
            zos.Flush();
            zos.Finish();

            // Get a copy of the compressed data
            byte[] compressed = new byte[ms.Length];
            Array.Copy(ms.ToArray(), compressed, (int)ms.Length);

            // Clean up
            zos.Close();

            return compressed;
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

#if DEBUG
        /// <summary>
        /// Debugging method to get a list of all the file names within a zip file
        /// </summary>
        private static IList<string> GetZipFileNames(byte[] zipFileData)
        {
            List<string> names = new List<string>();
            
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
                return names;
            }

            // Go through the files within the zip
            foreach (ZipEntry ze in zf)
            {
                names.Add(ze.Name);
            }

            ms.Dispose();
            return names;
        }
#endif

        public int ID
        {
            get
            {
                return m_a.ID;
            }
        }

        public bool IsReview
        {
            get
            {
                return (AssignmentTypes.CriticalReview == m_a.Type);
            }
        }

        public string Name
        {
            get
            {
                return m_a.AssignmentName;
            }
        }

        private void OSBLESubmitAssignmentCompleted(object sender, SubmitAssignmentCompletedEventArgs e)
        {
            object[] args = e.UserState as object[];
            OsbleServiceClient osc = args[0] as OsbleServiceClient;
            EventHandler onSaveComplete = args[1] as EventHandler;

            // We're done with the OSBLE client and can close it
            osc.CloseAsync();
            m_osbleClient = null;

            onSaveComplete(this, new SaveEventArgs(null == e.Error && e.Result));
        }

        private void OSBLESubmitReviewCompleted(object sender, SubmitReviewCompletedEventArgs e)
        {
            object[] args = e.UserState as object[];
            OsbleServiceClient osc = args[0] as OsbleServiceClient;
            EventHandler onSaveComplete = args[1] as EventHandler;

            // We're done with the OSBLE client and can close it
            osc.CloseAsync();

            onSaveComplete(this, new SaveEventArgs(null == e.Error && e.Result));
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

        public void SaveAsync(AssignmentStream stream, Workspace workspace, EventHandler onSaveComplete)
        {
            // Create another thread to do this
            ParameterizedThreadStart pts = new ParameterizedThreadStart(this.SaveAsyncThreadProc);
            Thread saveThread = new Thread(pts);
            saveThread.Start(new object[] { stream, workspace, onSaveComplete });
        }

        /// <summary>
        /// Thread procedure for saving an assignment. Compresses the workspace save data and then starts 
        /// the save process by re-authenticating.
        /// </summary>
        private void SaveAsyncThreadProc(object o)
        {
            AssignmentStream stream = (o as object[])[0] as AssignmentStream;
            Workspace workspace = (o as object[])[1] as Workspace;
            EventHandler onSaveComplete = (o as object[])[2] as EventHandler;

            // Build the zip file here. Start by saving the workspace to a memory stream.
            MemoryStream ms = new MemoryStream();
            workspace.Save(ms);

            // Next compress the file data
            string fileName;
            if (null != stream)
            {
                fileName = stream.Name;
            }
            else
            {
                fileName = OSBLEState.GetDeliverableFileName(this);
            }
            byte[] zipData = CreateZipFile(ms.ToArray(), fileName);

            // Dispose the memory stream, as we are done with it
            ms.Dispose();
            ms = null;

            // If it's null then we have a problem
            if (null == zipData)
            {
                onSaveComplete(this, new SaveEventArgs(false));
                return;
            }

#if DEBUG
            IList<string> fileNamesCheck = GetZipFileNames(zipData);
            if (1 != fileNamesCheck.Count)
            {
                throw new Exception("Workspace save data could not be correctly compressed");
            }
#endif

            // Authenticate so that we can save to OSBLE
            AuthenticationServiceClient auth = new AuthenticationServiceClient(
                m_bind, new System.ServiceModel.EndpointAddress(OSBLEState.AuthServiceLink));
            auth.ValidateUserCompleted += new EventHandler<ValidateUserCompletedEventArgs>(AuthForSaveCompleted);
            auth.ValidateUserAsync(m_userName, m_password, new object[] { auth, onSaveComplete, zipData });
        }

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

        public class SaveEventArgs : EventArgs
        {
            private bool m_success;

            public SaveEventArgs(bool success)
            {
                m_success = success;
            }

            public bool Success
            {
                get
                {
                    return m_success;
                }
            }
        }
    }
}
