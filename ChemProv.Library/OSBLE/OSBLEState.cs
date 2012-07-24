﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using ChemProV.Library.OsbleService;
using ChemProV.Library.ServiceReference1;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace ChemProV.Library.OSBLE
{
    public class OSBLEState
    {
        private string m_authToken = null;

        private System.ServiceModel.BasicHttpBinding m_bind;

        private IList<Course> m_courses = null;

        /// <summary>
        /// Counter used to keep track of how many async events need to be completed before 
        /// we have all the course data we need.
        /// </summary>
        private int m_coursesRemaining = 0;

        private Assignment m_currentAssignment = null;

        private bool m_isLoggedIn = false;

        private OsbleServiceClient m_osbleClient = null;
        
        private string m_password;
        
        private string m_userName;

        public OSBLEState(string userName, string password)
        {
            m_userName = userName;
            m_password = password;

            m_bind = new System.ServiceModel.BasicHttpBinding();
            m_bind.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.None;
            m_bind.ReceiveTimeout = new TimeSpan(0, 0, 10);
            m_bind.SendTimeout = new TimeSpan(0, 0, 10);
            m_bind.MaxBufferSize = 2147483647;
            m_bind.MaxReceivedMessageSize = 2147483647;
        }

        private void AuthClient_ValidateUserCompleted(object sender, ValidateUserCompletedEventArgs e)
        {
            if (null != e.Error)
            {
                m_authToken = null;
                m_isLoggedIn = false;

                OnLoginFailure(this, new OSBLEStateEventArgs(false,
                    "Could not login. Please recheck your user name and password."));
                    
                    //new OSBLEStateEventArgs(false, e.Error.Message));

                return;
            }

            // This means that they're logged in
            m_isLoggedIn = true;

            // The authentication client has delivered what we need and so we can close it
            AuthenticationServiceClient authClient = e.UserState as AuthenticationServiceClient;
            authClient.CloseAsync();
            
            // Store the authentication token
            m_authToken = e.Result;

            // Create the OSBLE client
            System.ServiceModel.BasicHttpBinding bind = new System.ServiceModel.BasicHttpBinding();
            bind.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.None;
            bind.ReceiveTimeout = new TimeSpan(0, 0, 10);
            bind.SendTimeout = new TimeSpan(0, 0, 10);
            bind.MaxBufferSize = 2147483647;
            bind.MaxReceivedMessageSize = 2147483647;
            bind.TextEncoding = System.Text.Encoding.Unicode;
            m_osbleClient = new OsbleServiceClient(bind,
                new System.ServiceModel.EndpointAddress("http://localhost:17532/Services/OsbleService.svc"));
            
            // Make sure we setup all callbacks here
            m_osbleClient.GetCoursesCompleted += new EventHandler<GetCoursesCompletedEventArgs>(OsbleClient_GetCoursesCompleted);
            m_osbleClient.GetAssignmentSubmissionCompleted += this.OsbleClient_GetAssignmentSubmissionCompleted;
            
            // Get the list of courses
            m_osbleClient.GetCoursesAsync(m_authToken, m_osbleClient);
        }

        public const string AuthServiceLink = "http://localhost:17532/Services/AuthenticationService.svc";
        //public const string AuthServiceLink = "https://osble.org/Services/AuthenticationService.svc";

        public IList<Course> Courses
        {
            get
            {
                return m_courses;
            }
        }

        public Assignment CurrentAssignment
        {
            get
            {
                return m_currentAssignment;
            }
        }

        public void GetAssignmentStreamAsync(Assignment assignment)
        {
            // Re-authenticate
            // The design choice is to re-authenticate every time we need to download an assignment file. This 
            // is primarily because previous tests that cached the authentication token were actually failing. 
            // This could be a bug in the OSBLE API, but rather than wait for that to be fixed I'm just sticking 
            // with the way that works.
            // This design also prevents issues from the authentication token expiring after login and then a 
            // long idle period. So it's not just to work around a bug, it makes sense from a usability 
            // perspective as well because this way we never have to tell the user that their session has expired 
            // and they need to log in again.
            AuthenticationServiceClient authClient = new AuthenticationServiceClient(m_bind,
                new EndpointAddress(AuthServiceLink));
            authClient.ValidateUserCompleted += this.ValidateForGetStreamCompleted;
            authClient.ValidateUserAsync(m_userName, m_password, new object[] { authClient, assignment });
        }

        private string GetDeliverableFileName(Assignment assignment)
        {
            foreach (Deliverable d in assignment.Deliverables)
            {
                if (DeliverableType.ChemProV == d.DeliverableType)
                {
                    return d.Name + ".cpml";
                }
            }

            return assignment.AssignmentName + ".cpml";
        }

        public bool IsLoggedIn
        {
            get
            {
                return m_isLoggedIn;
            }
        }

        private void OsbleClient_GetAssignmentSubmissionCompleted(object sender,
            GetAssignmentSubmissionCompletedEventArgs e)
        {
            if (e.Cancelled || null != e.Error)
            {
                m_currentAssignment = null;
                
                // TODO: See if we need to re-authenticate (?)
                OnDownloadComplete(this, new OSBLEStateEventArgs(false,
                    "Could not download the specified assignment file"));
                
                return;
            }

            // If there hasn't been any submission for this assignment yet then it may be 0 bytes in size
            if (0 == e.Result.Length)
            {
                OnDownloadComplete(this, new OSBLEStateEventArgs(true,
                    "Assignment has yet to be submitted. You may save your work to OSBLE to submit " +
                    "the first version for this assignment.", null));
                return;
            }

            // Make a memory stream for the byte array
            System.IO.MemoryStream ms = new System.IO.MemoryStream(e.Result);
            
            // Create the zip file
            ICSharpCode.SharpZipLib.Zip.ZipFile zf = new ICSharpCode.SharpZipLib.Zip.ZipFile(ms);

            // If there are 0 files in the zip, then let the user know that nothing has been submitted
            if (0 == zf.Count)
            {
                OnDownloadComplete(this, new OSBLEStateEventArgs(true,
                    "Assignment has yet to be submitted. You may save your work to OSBLE to submit " +
                    "the first version for this assignment.", null));
                return;
            }

            // Go through the files within the zip looking for the .cpml
            foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry ze in zf)
            {
                if (ze.Name.ToLower().EndsWith(".cpml"))
                {
                    // We need a stream that is seekable and the Zip streams don't guarantee this. Thus 
                    // we'll read the whole thing into a memory stream.
                    MemoryStream msUncompressed;
                    using (Stream tempStream = zf.GetInputStream(ze))
                    {
                        msUncompressed = new MemoryStream();
                        tempStream.CopyTo(msUncompressed);
                    }

                    OnDownloadComplete(this, new OSBLEStateEventArgs(true, null, msUncompressed));
                    return;
                }
            }
                
            // This means we didn't find a .cpml in the zip
            m_currentAssignment = null;
            OnDownloadComplete(this, new OSBLEStateEventArgs(false,
                "Could not download the specified assignment file"));
        }

        private void OsbleClient_GetCourseAssignmentsCompleted(object sender, GetCourseAssignmentsCompletedEventArgs e)
        {
            // Set the assignments for the Course object
            (e.UserState as Course).Assignments = e.Result;

            // Set the course reference for each assignment since this is NOT done automatically
            foreach (Assignment a in e.Result)
            {
                a.Course = e.UserState as Course;
            }

            // Decrement the number of remaining courses
            if (0 == Interlocked.Decrement(ref m_coursesRemaining))
            {
                OnRefreshComplete(this, OSBLEStateEventArgs.Empty);
            }
        }

        private void OsbleClient_GetCoursesCompleted(object sender, GetCoursesCompletedEventArgs e)
        {
            OsbleServiceClient osbleClient = e.UserState as OsbleServiceClient;
            m_courses = e.Result;

            // If we don't have any courses then let the user know
            if (0 == m_courses.Count)
            {
                OnError(this, new OSBLEStateEventArgs(false,
                    "No courses were found for this user. You may need to contact your professor " +
                    "or systems administrator to remedy this."));
                return;
            }

            // Loading the course list does not load their child lists of assignments.
            // We need to do that now.
            osbleClient.GetCourseAssignmentsCompleted += OsbleClient_GetCourseAssignmentsCompleted;
            m_coursesRemaining = 0;
            foreach (Course course in m_courses)
            {
                Interlocked.Increment(ref m_coursesRemaining);
                osbleClient.GetCourseAssignmentsAsync(course.ID, m_authToken, course);
            }
        }

        private void OsbleClient_SubmitAssignmentCompleted(object sender, SubmitAssignmentCompletedEventArgs e)
        {
            OnSaveComplete(this, new OSBLEStateEventArgs(e.Result, null));
        }

        public void RefreshAsync()
        {
            // Create the authentication client. We need this to get an authentication token which serves 
            // as our "key" for getting lists of courses, assignments, etc.
            AuthenticationServiceClient authClient = new AuthenticationServiceClient(m_bind,
                new EndpointAddress(AuthServiceLink));
            authClient.ValidateUserCompleted += new EventHandler<ValidateUserCompletedEventArgs>(AuthClient_ValidateUserCompleted);
            try
            {
                authClient.ValidateUserAsync(m_userName, m_password, authClient);
            }
            catch (Exception)
            {
                OnLoginFailure(this, new OSBLEStateEventArgs(false, 
                    "Could not log in, please try again"));
                return;
            }
        }

        public void SaveAssignmentAsync(Assignment a, byte[] assignmentData)
        {
            // When we save to an assignment, it becomes the "current" assignment
            m_currentAssignment = a;

            // Call the method to save to the current
            SaveCurrentAssignmentAsync(assignmentData);
        }

        public void SaveCurrentAssignmentAsync(byte[] assignmentData)
        {
            AuthenticationServiceClient authClient = new AuthenticationServiceClient(m_bind,
                new EndpointAddress(AuthServiceLink));
            authClient.ValidateUserCompleted += this.ValidateForSaveCompleted;
            authClient.ValidateUserAsync(m_userName, m_password, new object[] { authClient, assignmentData });
        }

        private void ValidateForGetStreamCompleted(object sender, ValidateUserCompletedEventArgs e)
        {
            // Error check
            if (e.Cancelled)
            {
                return;
            }
            if (null != e.Error)
            {
                OnError(this, new OSBLEStateEventArgs(false, e.Error.Message));
                return;
            }
            
            // The authentication token is stored in the result
            m_authToken = e.Result;

            // Build the OSBLE client
            System.ServiceModel.BasicHttpBinding bind = new System.ServiceModel.BasicHttpBinding();
            bind.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.None;
            bind.ReceiveTimeout = new TimeSpan(0, 0, 10);
            bind.SendTimeout = new TimeSpan(0, 0, 10);
            bind.MaxBufferSize = 2147483647;
            bind.MaxReceivedMessageSize = 2147483647;
            m_osbleClient = new OsbleServiceClient(bind,
                new System.ServiceModel.EndpointAddress("http://localhost:17532/Services/OsbleService.svc"));
            m_osbleClient.GetAssignmentSubmissionCompleted += this.OsbleClient_GetAssignmentSubmissionCompleted;

            // We built an object array for the user state to keep track of the authentication client 
            // (which we have to close at the end) and the assignment.
            object[] objs = e.UserState as object[];

            // The authentication client is the first object in the array
            AuthenticationServiceClient auth = objs[0] as AuthenticationServiceClient;

            // The assignment is the second object in the array
            // Store a reference to this assignment as the "current" assignment
            m_currentAssignment = objs[1] as Assignment;

            // We will reuse the array for our next async call, this time with the OSBLE client as the 
            // first object
            objs[0] = m_osbleClient;

            // Get the assignment submission. In the completion function it will actually be decompressed.
            m_osbleClient.GetAssignmentSubmissionAsync(m_currentAssignment.ID, m_authToken, objs);

            // "Always close the client" says the documentation
            auth.CloseAsync();
        }

        private void ValidateForSaveCompleted(object sender, ValidateUserCompletedEventArgs e)
        {
            // Error check
            if (e.Cancelled)
            {
                return;
            }
            if (null != e.Error)
            {
                OnError(this, new OSBLEStateEventArgs(false, e.Error.Message));
                return;
            }

            // The authentication token is stored in the result
            m_authToken = e.Result;

            // Build the OSBLE client
            m_osbleClient = new OsbleServiceClient(m_bind,
                new System.ServiceModel.EndpointAddress("http://localhost:17532/Services/OsbleService.svc"));
            m_osbleClient.SubmitAssignmentCompleted += new EventHandler<SubmitAssignmentCompletedEventArgs>(OsbleClient_SubmitAssignmentCompleted);

            // We built an object array for the user state to keep track of the authentication client 
            // (which we have to close at the end) and the save data
            object[] objs = e.UserState as object[];

            // The authentication client is the first object in the array
            AuthenticationServiceClient auth = objs[0] as AuthenticationServiceClient;

            // The assignment data is the second object in the array. Note that this is the uncompressed data 
            // and we need to put it in a zip before submitting.
            byte[] assignmentData = objs[1] as byte[];

            // Create a memory stream to save the zip file to
            MemoryStream ms = new MemoryStream();

            // Create the zip output stream with the maximum level of compression
            ZipOutputStream zos = new ZipOutputStream(ms);
            zos.SetLevel(9);

            // Create the entry for this file
            ZipEntry newEntry = new ZipEntry(ZipEntry.CleanName(GetDeliverableFileName(m_currentAssignment)));
            newEntry.DateTime = DateTime.Now;
            zos.PutNextEntry(newEntry);
            zos.Write(assignmentData, 0, assignmentData.Length);
            zos.CloseEntry();

            // Flush the zip file
            zos.Flush();

            // Get a reference to the compressed data
            byte[] compressed = ms.ToArray();

            // Close the zip file
            zos.Close();

            // We will reuse the array for our next async call, this time with the OSBLE client as the 
            // first object
            objs[0] = m_osbleClient;

            // Submit
            m_osbleClient.SubmitAssignmentAsync(m_currentAssignment.ID, ms.ToArray(), m_authToken);

            // Free memory
            zos.Dispose();
            ms.Dispose();

            // "Always close the client" says the documentation
            auth.CloseAsync();
        }

        #region Events

        public event EventHandler OnDownloadComplete = delegate { };

        /// <summary>
        /// Invoked when any error occurs besides a login failure. These errors mean that the user IS 
        /// logged in, but there was some problem that occured after logging in.
        /// </summary>
        public event EventHandler OnError = delegate { };
        
        public event EventHandler OnLoginFailure = delegate { };

        public event EventHandler OnRefreshComplete = delegate { };

        public event EventHandler OnSaveComplete = delegate { };

        #endregion Events
    }

    public class OSBLEStateEventArgs : EventArgs
    {
        private bool m_success = false;
        
        private string m_message;

        private Stream m_stream = null;

        public OSBLEStateEventArgs(bool success, string message)
        {
            m_message = message;
            m_success = success;
        }

        public OSBLEStateEventArgs(bool success, string message, Stream stream)
        {
            m_message = message;
            m_success = success;
            m_stream = stream;
        }

        public static readonly OSBLEStateEventArgs Empty = new OSBLEStateEventArgs(true, string.Empty);

        public string Message
        {
            get
            {
                return m_message;
            }
        }

        public Stream Stream
        {
            get
            {
                return m_stream;
            }
        }

        /// <summary>
        /// Indicates whether or not the action completed successfully
        /// </summary>
        public bool Success
        {
            get
            {
                return m_success;
            }
        }
    }
}