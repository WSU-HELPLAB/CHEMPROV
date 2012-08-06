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
using System.Windows.Threading;
using ChemProV.Logic.OSBLE;

#if DEBUG
using ChemProV.OSBLEAuthServiceLocalRef;
using ChemProV.OSBLEClientServiceLocalRef;
#else
using ChemProV.OSBLEAuthServiceRef;
using ChemProV.OSBLEClientServiceRef;
#endif

namespace ChemProV.UI.OSBLE
{
    public partial class LoginWin : ChildWindow
    {
        private OSBLEState m_state = null;

        private bool m_terminate = false;

        private const string c_osbleLoginFileName = "ChemProV_OSBLE_Login.dat";

        public LoginWin()
        {
            InitializeComponent();

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
                        UserNameTextBox.Text = Dec(a);

                        // Read the data for the encrypted password
                        a = new byte[ds2];
                        s.Read(a, 0, a.Length);
                        PasswordBox.Password = Dec(a);

                        // Saved file indicates that they had "Remember me" checked last time, so we'll check it 
                        // again
                        RememberCredentialsCheckBox.IsChecked = true;
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

        private void LoginWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;

            // Set the flag to indicate termination. This way, when async callbacks complete, we 
            // can just ignore them.
            m_terminate = true;
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

            ErrorTextBlock.Visibility = System.Windows.Visibility.Collapsed;
            OKButton.Visibility = System.Windows.Visibility.Collapsed;
            LoadingProgressBar.Visibility = System.Windows.Visibility.Visible;

            m_state = new OSBLEState(UserNameTextBox.Text, PasswordBox.Password);
            m_state.OnLoginFailure += new EventHandler(OnLoginFailure);
            m_state.OnError += new EventHandler(OnError);
            m_state.OnRefreshComplete += new EventHandler(StateRefreshComplete);
            m_state.RefreshAsync();

            #region DEBUG

            //WebClient wc = new WebClient();
            //wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);
            //wc.DownloadStringAsync(new Uri("https://osble.org/Services/AuthenticationService.svc"));
            //wc.DownloadStringAsync(new Uri("http://www.evanolds.com/"));

            #endregion
        }

        private void OnError(object sender, EventArgs e)
        {
            if (!m_terminate)
            {
                Dispatcher.BeginInvoke(new EventHandler(ShowMessage),
                   (e as OSBLEStateEventArgs).Message, EventArgs.Empty);
            }
        }

        private void OnLoginFailure(object sender, EventArgs e)
        {
            if (!m_terminate)
            {
                // TODO: Check if I need to do this in a thread safe way
                LoadingProgressBar.Visibility = System.Windows.Visibility.Collapsed;
                OKButton.Visibility = System.Windows.Visibility.Visible;
                ErrorTextBlock.Text = (e as OSBLEStateEventArgs).Message;
                ErrorTextBlock.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void PasswordBox_KeyUp(object sender, KeyEventArgs e)
        {
            // Shortcut for logon
            if (e.Key == Key.Enter)
            {
                OKButton_Click(null, null);
            }
        }

        private void ShowMessage(object sender, EventArgs e)
        {
            MessageBox.Show(sender as string);
            this.DialogResult = false;
        }

        public OSBLEState State
        {
            get
            {
                return m_state;
            }
        }

        private void StateRefreshComplete(object sender, EventArgs e)
        {
            if (!m_terminate)
            {
                LoginAttemptCompleted(this, EventArgs.Empty);
                this.DialogResult = true;
            }
        }

        private void UserNameTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            // Shortcut for logon
            if (e.Key == Key.Enter)
            {
                OKButton_Click(null, null);
            }
        }

        /// <summary>
        /// Invoked when the login window finishes the login attempt, either with a successful 
        /// login or an error.
        /// </summary>
        public event EventHandler LoginAttemptCompleted = delegate { };
    }
}

