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

        public LoginWindow()
        {
            InitializeComponent();
            ViewModel = new LoginWindowViewModel();
            ViewModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ViewModel_PropertyChanged);    
        }

        void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.CompareTo("IsProcessingLogin") == 0)
            {
                OKButton.IsEnabled = !ViewModel.IsProcessingLogin;
                if (ViewModel.IsLoggedIn)
                {
                    this.DialogResult = true;
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

