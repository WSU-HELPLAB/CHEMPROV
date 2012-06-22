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
using ChemProV.Library.OsbleAuthServices;
using System.ComponentModel;
using System.Windows.Threading;

namespace ChemProV.Library.OSBLE.ViewModels
{
    public class LoginWindowViewModel : ViewModelBase
    {
        private string _password = "";
        private bool _rememberMe = false;
        private Visibility _errorVisibility = Visibility.Collapsed;
        private AuthenticationServiceClient authClient;
        private bool _isProcessingLogin = false;
        private string _errorText = "Error text here.";
        private DispatcherTimer _timer;

        #region properties
        public string AuthenticationToken { get; set; }
        public ICommand LoginCommand { get; set; }
        public UserProfile Profile { get; set; }
        public bool IsLoggedIn { get; set; }
        public string ErrorText
        {
            get
            {
                return _errorText;
            }
            set
            {
                _errorText = value;
                OnPropertyChanged("ErrorText");
            }
        }

        public bool IsProcessingLogin
        {
            get
            {
                return _isProcessingLogin;
            }
            set
            {
                _isProcessingLogin = value;
                OnPropertyChanged("IsProcessingLogin");
            }
        }

        public string UserName
        {
            get
            {
                return Profile.UserNamek__BackingField;
                //return Profile.UserName;
            }
            set
            {
                //Profile.UserName = value;
                Profile.UserNamek__BackingField = value;
                OnPropertyChanged("UserName");
            }
        }
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }
        public bool RememberMe
        {
            get
            {
                return _rememberMe;
            }
            set
            {
                _rememberMe = value;
                OnPropertyChanged("RememberMe");
            }
        }
        public Visibility ErrorVisibility
        {
            get
            {
                return _errorVisibility;
            }
            set
            {
                _errorVisibility = value;
                OnPropertyChanged("ErrorVisibility");
            }
        }
        #endregion

        public LoginWindowViewModel()
        {
            Profile = new UserProfile();
            authClient = new AuthenticationServiceClient(ServiceBindings.AuthenticationServiceBinding, ServiceBindings.AuthenticationServiceEndpoint);
            LoginCommand = new DelegateCommand(Login, CanLogIn);

            //set up our "connection timeout" timer.  Currently set to to a 10 second timeout.  May
            //need to adjust.
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 10);
            _timer.Tick += new EventHandler(_timer_Tick);

            authClient.ValidateUserCompleted += new EventHandler<ValidateUserCompletedEventArgs>(ValidateUserCompleted);
            authClient.GetActiveUserCompleted += new EventHandler<GetActiveUserCompletedEventArgs>(GetActiveUserCompleted);
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            ServerConnectionError();
        }

        private void ServerConnectionError()
        {
            _timer.Stop();
            IsProcessingLogin = false;
            ErrorText = "Could not connect to server.";
            ErrorVisibility = Visibility.Visible;
        }

        void GetActiveUserCompleted(object sender, GetActiveUserCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                Profile = e.Result;
                IsLoggedIn = true;
            }
            IsProcessingLogin = false;
        }

        void ValidateUserCompleted(object sender, ValidateUserCompletedEventArgs e)
        {
            _timer.Stop();
            if (e.Error == null)
            {
                string result = e.Result;
                if (result.Length != 0)
                {
                    AuthenticationToken = result;

                    //get the full user object
                    ErrorVisibility = Visibility.Collapsed;
                    authClient.GetActiveUserAsync(result);
                }
                else
                {
                    ErrorText = "Invalid user name or password.";
                    ErrorVisibility = Visibility.Visible;
                    IsProcessingLogin = false;
                }
            }
            else
            {
                ServerConnectionError();
            }
        }

        private void Login(object param)
        {
            IsProcessingLogin = true;
            ErrorVisibility = Visibility.Collapsed;
            _timer.Start();
            authClient.ValidateUserAsync(UserName, Password);
        }

        private bool CanLogIn(object param)
        {
            return true;
        }
    }
}
