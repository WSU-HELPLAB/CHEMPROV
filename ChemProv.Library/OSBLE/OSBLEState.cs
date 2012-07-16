using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using ChemProV.Library.OsbleService;
using ChemProV.Library.ServiceReference1;

namespace ChemProV.Library.OSBLE
{
    public class OSBLEState
    {
        private string m_authToken = null;

        private IList<Course> m_courses = null;

        /// <summary>
        /// Counter used to keep track of how many async events need to be completed before 
        /// we have all the course data we need.
        /// </summary>
        private int m_coursesRemaining = 0;

        private bool m_isLoggedIn = false;
        
        private string m_password;

        private List<RelevantAssignment> m_relevantAssignments = new List<RelevantAssignment>();
        
        private string m_userName;

        public OSBLEState(string userName, string password)
        {
            m_userName = userName;
            m_password = password;
        }

        private void AuthClient_ValidateUserCompleted(object sender, ValidateUserCompletedEventArgs e)
        {
            if (null != e.Error)
            {
                m_authToken = null;
                m_isLoggedIn = false;

                OnLoginFailure(this, new OSBLEStateEventArgs(e.Error.Message));

                return;
            }

            // This means that they're logged in
            m_isLoggedIn = true;

            // The authentication client has delivered what we need and so we can close it
            AuthenticationServiceClient authClient = e.UserState as AuthenticationServiceClient;
            authClient.CloseAsync();
            
            // Store the authentication token
            m_authToken = e.Result;

            System.ServiceModel.BasicHttpBinding bind = new System.ServiceModel.BasicHttpBinding();
            bind.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.None;
            bind.ReceiveTimeout = new TimeSpan(0, 0, 10);
            bind.SendTimeout = new TimeSpan(0, 0, 10);
            OsbleServiceClient osbleClient = new OsbleServiceClient(bind,
                new System.ServiceModel.EndpointAddress("http://localhost:17532/Services/OsbleService.svc"));
            osbleClient.GetCoursesCompleted += new EventHandler<GetCoursesCompletedEventArgs>(OsbleClient_GetCoursesCompleted);
            osbleClient.GetCoursesAsync(m_authToken, osbleClient);
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

        public bool IsLoggedIn
        {
            get
            {
                return m_isLoggedIn;
            }
        }

        private void OsbleClient_GetCourseAssignmentsCompleted(object sender, GetCourseAssignmentsCompletedEventArgs e)
        {
            (e.UserState as Course).Assignments = e.Result;
            IList<Assignment> assignments = e.Result;

            // Add assignments with ChemProV deliverables to the list of relevant assignments
            foreach (Assignment a in assignments)
            {
                foreach (Deliverable d in a.Deliverables)
                {
                    if (DeliverableType.ChemProV == d.DeliverableType)
                    {
                        m_relevantAssignments.Add(new RelevantAssignment(a, e.UserState as Course, d));
                        break;
                    }
                }
            }

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
                OnError(this, new OSBLEStateEventArgs(
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

        public void RefreshAsync()
        {
            // Clear the list of relevant assignments. We will rebuild it with a series of web 
            // service calls
            m_relevantAssignments.Clear();

            System.ServiceModel.BasicHttpBinding bind = new System.ServiceModel.BasicHttpBinding();
            bind.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.None;
            bind.ReceiveTimeout = new TimeSpan(0, 0, 10);
            bind.SendTimeout = new TimeSpan(0, 0, 10);
            AuthenticationServiceClient authClient = new AuthenticationServiceClient(bind,
                new EndpointAddress(AuthServiceLink));
            authClient.ValidateUserCompleted += new EventHandler<ValidateUserCompletedEventArgs>(AuthClient_ValidateUserCompleted);
            try
            {
                authClient.ValidateUserAsync("bob@smith.com", "123123", authClient);
            }
            catch (Exception)
            {
                OnLoginFailure(this, new OSBLEStateEventArgs(
                    "Could not log in, please try again"));
                return;
            }
        }

        /// <summary>
        /// Gets the list of assignments from the last refresh that have ChemProV deliverables
        /// </summary>
        public IList<RelevantAssignment> RelevantAssignments
        {
            get
            {
                return m_relevantAssignments;
            }
        }

        #region Events

        /// <summary>
        /// Invoked when any error occurs besides a login failure. These errors mean that the user IS 
        /// logged in, but there was some problem that occured after logging in.
        /// </summary>
        public event EventHandler OnError = delegate { };
        
        public event EventHandler OnLoginFailure = delegate { };

        public event EventHandler OnRefreshComplete = delegate { };

        #endregion Events

        public class RelevantAssignment
        {
            private Assignment m_assignment;

            private Course m_course;

            /// <summary>
            /// Reference to the first deliverable in the assignment that has a ChemProV type
            /// </summary>
            private Deliverable m_deliverable;

            /// <param name="deliverable">Reference to the first deliverable in the assignment that has a 
            /// ChemProV type</param>
            public RelevantAssignment(Assignment assignment, Course course, Deliverable deliverable)
            {
                m_assignment = assignment;
                m_course = course;
                m_deliverable = deliverable;
            }

            public Assignment Assignment
            {
                get
                {
                    return m_assignment;
                }
            }

            public Course Course
            {
                get
                {
                    return m_course;
                }
            }

            /// <summary>
            /// Gets a reference to the first deliverable in the assignment that has a ChemProV type
            /// </summary>
            public Deliverable Deliverable
            {
                get
                {
                    return m_deliverable;
                }
            }

            public override string ToString()
            {
                return string.Format("{0} ({1}.cpml)", m_assignment.AssignmentName, m_deliverable.Name);
            }
        }
    }

    public class OSBLEStateEventArgs : EventArgs
    {
        private string m_message;

        public OSBLEStateEventArgs(string message)
        {
            m_message = message;
        }

        public static readonly OSBLEStateEventArgs Empty = new OSBLEStateEventArgs(string.Empty);

        public string Message
        {
            get
            {
                return m_message;
            }
        }
    }
}
