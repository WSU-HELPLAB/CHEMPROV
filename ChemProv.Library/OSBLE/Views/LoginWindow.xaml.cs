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
using ChemProV.Library.OSBLE.ViewModels;
using System.Windows.Threading;

namespace ChemProV.Library.OSBLE.Views
{
    public partial class LoginWindow : ChildWindow
    {
        public LoginWindowViewModel ViewModel
        {
            get
            {
                return this.DataContext as LoginWindowViewModel;
            }
            private set
            {
                this.DataContext = value;
            }
        }

        public delegate void LoginAttemptCompletedDelegate(LoginWindow sender, LoginWindowViewModel model);

        /// <summary>
        /// Fires when the login attempt completes. Note that it could have completed with success or failure.
        /// </summary>
        public event LoginAttemptCompletedDelegate LoginAttemptCompleted = null;

        public LoginWindow()
        {
            InitializeComponent();
            ViewModel = new LoginWindowViewModel();
            ViewModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ViewModel_PropertyChanged);
        }

        void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (0 == e.PropertyName.CompareTo("IsProcessingLogin"))
            {
                OKButton.IsEnabled = !ViewModel.IsProcessingLogin;
                if (ViewModel.IsLoggedIn)
                {
                    this.DialogResult = true;

                    // Fire the login completion event if non-null
                    if (null != LoginAttemptCompleted)
                    {
                        LoginAttemptCompleted(this, ViewModel);
                    }
                }
            }
        }

        private void PasswordBox_KeyUp(object sender, KeyEventArgs e)
        {
            //shortcut for logon
            if (e.Key == Key.Enter)
            {
                OKButton.Command.Execute(this);
            }
        }
    }
}

