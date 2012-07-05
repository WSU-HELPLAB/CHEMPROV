using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
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

        private const string c_osbleLoginFileName = "ChemProV_OSBLE_Login.dat";

        public LoginWindow()
        {
            InitializeComponent();
            ViewModel = new LoginWindowViewModel();
            ViewModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ViewModel_PropertyChanged);

            // See if the user name and password were stored locally
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(c_osbleLoginFileName))
                {
                    using (Stream s = store.OpenFile(c_osbleLoginFileName, FileMode.Open, FileAccess.Read))
                    {

                        // First byte is the size of the encrypted user name
                        int ds1 = s.ReadByte();
                        if (ds1 <= 0)
                        {
                            return;
                        }

                        // Next byte is the size of the encrypted password data
                        int ds2 = s.ReadByte();
                        if (-1 == ds2)
                        {
                            return;
                        }

                        // The stream length should be at least the sum of the data sizes plus the 2 bytes for 
                        // the size values
                        if (s.Length < 2 + ds1 + ds2)
                        {
                            return;
                        }

                        // Read the data for the encrypted user name
                        byte[] a = new byte[ds1];
                        s.Read(a, 0, ds1);
                        ViewModel.UserName = Dec(a);

                        // Read the data for the encrypted password
                        a = new byte[ds2];
                        s.Read(a, 0, a.Length);
                        ViewModel.Password = Dec(a);

                        // Saved file indicates that they had "Remember me" checked last time, so we'll check it 
                        // again
                        ViewModel.RememberMe = true;

                    }
                }
            }
        }

        /// <summary>
        /// Decrypts an array of bytes into a string. Public so that it can have unit tests.
        /// </summary>
        public static string Dec(byte[] enc)
        {            
            byte[] a1 = new byte[enc.Length / 2];
            for (int i = 0; i < enc.Length; i += 2)
            {
                // It's a weak encryption scheme, but we just use every two bytes for one byte 
                // in the decrypted version. Every other bit starting at 0 is just random junk, 
                // but every other bit starting at bit 1 is the actual byte data.
                ushort u = (ushort)(enc[i] | (enc[i + 1] << 8));
                a1[i / 2] = 0;
                for (int j = 1; j < 16; j += 2)
                {
                    a1[i / 2] |= (byte)((u & (1 << j)) >> ((j + 1) / 2));
                }
            }

            return Encoding.UTF8.GetString(a1, 0, a1.Length);
        }

        /// <summary>
        /// Encrypts a string into an array of bytes. This method is public so that it can have 
        /// unit tests.
        /// </summary>
        public static byte[] Enc(string s)
        {
            Random r = new Random();
            byte[] stringBytes = Encoding.UTF8.GetBytes(s);
            byte[] output = new byte[stringBytes.Length * 2];
            for (int i = 0; i < stringBytes.Length; i++)
            {
                // Start off with a completely random value
                ushort us = (ushort)r.Next(ushort.MaxValue);
                
                for (int j = 0; j < 8; j++)
                {
                    // Clear the bit first
                    us &= (ushort)~(1 << (j * 2 + 1));

                    // Set it if it's a 1 in the source
                    if (0 != (stringBytes[i] & (1 << j)))
                    {
                        us |= (ushort)(1 << (j * 2 + 1));
                    }
                }

                // Set values in output array
                output[i * 2] = (byte)us;
                output[i * 2 + 1] = (byte)(us >> 8);
            }

            return output;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Ignore this click if they left out the user name or password
            if (string.IsNullOrEmpty(UserNameTextBox.Text) ||
                string.IsNullOrEmpty(PasswordBox.Password))
            {
                return;
            }
            
            // Save the user name and password if check box is checked
            if (RememberCredentialsCheckBox.IsChecked.HasValue &&
                RememberCredentialsCheckBox.IsChecked.Value)
            {
                byte[] encUserName = Enc(UserNameTextBox.Text);
                byte[] encPwd = Enc(PasswordBox.Password);

                // Write the encrypted data to isolated storage
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream s = new IsolatedStorageFileStream(c_osbleLoginFileName, FileMode.OpenOrCreate, store))
                    {
                        s.Position = 0;
                        s.SetLength(0);

                        byte len = (byte)encUserName.Length;
                        s.WriteByte(len);
                        len = (byte)encPwd.Length;
                        s.WriteByte(len);
                        s.Write(encUserName, 0, encUserName.Length);
                        s.Write(encPwd, 0, encPwd.Length);
                    }
                }
            }
            else
            {
                // Otherwise ensure that the file to save user name and password is deleted
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    try
                    {
                        store.DeleteFile(c_osbleLoginFileName);
                    }
                    catch (Exception) { }
                }
            }

            this.DialogResult = true;

            //ViewModel.LoginCommand.Execute(null);

            OsbleAuthServices.AuthenticationServiceClient c = new OsbleAuthServices.AuthenticationServiceClient(
                ServiceBindings.AuthenticationServiceBinding,
                new System.ServiceModel.EndpointAddress("https://osble.org/Services/AuthenticationService.svc"));
                //ServiceBindings.RemoteAuthenticationEndpoint);
            c.ValidateUserCompleted += new EventHandler<OsbleAuthServices.ValidateUserCompletedEventArgs>(c_ValidateUserCompleted);
            c.ValidateUserAsync(UserNameTextBox.Text, PasswordBox.Password);

            #region DEBUG

            //WebClient wc = new WebClient();
            //wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);
            //wc.DownloadStringAsync(new Uri("https://osble.org/Services/AuthenticationService.svc"));
            //wc.DownloadStringAsync(new Uri("http://www.evanolds.com/"));
            //wc.DownloadStringAsync(new Uri("https://thefinder.tax.ohio.gov/StreamlineSalesTaxWeb/WebService/About.aspx"));
            //wc.DownloadStringAsync(new Uri("http://sciencesoft.at/services/latex?wsdl"));

            #endregion
        }

        void c_ValidateUserCompleted(object sender, OsbleAuthServices.ValidateUserCompletedEventArgs e)
        {
            string result = e.Result;
            bool breakhere = true;
        }

        private void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string result = e.Result;
            bool breakHere = true;
        }

        private void PasswordBox_KeyUp(object sender, KeyEventArgs e)
        {
            //shortcut for logon
            if (e.Key == Key.Enter)
            {
                OKButton.Command.Execute(this);
            }
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
    }
}

