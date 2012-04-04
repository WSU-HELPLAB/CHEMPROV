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
using ChemProv.Library.OsbleServices;
using System.ComponentModel;

namespace ChemProv.Library.OSBLE.ViewModels
{
    public class LoginWindowViewModel : ViewModelBase
    {
        private string _password = "";
        private bool _rememberMe = false;
        private Visibility _errorVisibility = Visibility.Collapsed;
        private OsbleServices.AuthenticationServiceClient authClient;

        #region properties
        public ICommand LoginCommand { get; set; }
        public UserProfile Profile { get; set; }
        public string UserName
        {
            get
            {
                return Profile.UserName;
            }
            set
            {
                Profile.UserName = value;
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
            authClient.ValidateUserCompleted += new EventHandler<ValidateUserCompletedEventArgs>(ValidateUserCompleted);
            LoginCommand = new DelegateCommand(Login, CanLogIn);
        }

        void ValidateUserCompleted(object sender, ValidateUserCompletedEventArgs e)
        {
            string result = e.Result;
        }

        private void Login(object param)
        {
            authClient.ValidateUserAsync(UserName, Password);
        }

        private bool CanLogIn(object param)
        {
            return true;
        }
    }
}
