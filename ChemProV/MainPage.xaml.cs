/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using ChemProV.Logic;
using ChemProV.UI;
using ChemProV.UI.OSBLE;
using ImageTools;
using ImageTools.IO.Png;

namespace ChemProV
{
    public partial class MainPage : UserControl
    {
        private string versionNumber = "";
        private const string c_saveFileFilter = "ChemProV PFD XML (*.cpml)|*.cpml|Portable Network Graphics (*.png)|*.png";
        private const string c_loadFileFilter = "ChemProV PFD XML (*.cpml)|*.cpml";
        private const string c_autoSaveFileName = "autoSave.cpml";
        private const string c_configFile = "cpv.config";
        private TimeSpan autoSaveTimeSpan = new TimeSpan(0, 1, 0);

        private DispatcherTimer saveTimer = new DispatcherTimer();

        /// <summary>
        /// Flag to indicate that events from m_workspace should be ignored. This is set to true when 
        /// we are modifying the workspace and thus want to ignore the events that are fired during 
        /// this modification.
        /// </summary>
        private bool m_ignoreWorkspaceChanges = false;

        private SaveFileDialog m_saveDialog = null;

        /// <summary>
        /// Represents the logical workspace. Refactoring is still happening but the long term goal 
        /// is to have all data stored in this object and all UI elements would attach listeners 
        /// and do their modifications through this object.
        /// </summary>
        private Workspace m_workspace = new Workspace();

        private bool OptionDifficultySettingChanged(OptionDifficultySetting value)
        {
            //save the change in the config file
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(c_configFile, FileMode.Create, isf))
                {
                    using (StreamWriter sr = new StreamWriter(isfs))
                    {
                        sr.WriteLine(value.ToString());
                    }
                }
            }

            // Show or hide UI elements appropriately
            if (m_workspace.Difficulty == OptionDifficultySetting.MaterialBalance)
            {
                Compounds_DF_TabControl.SelectedItem = DFAnalysisTab;
                CompoundTableTab.Visibility = Visibility.Collapsed;
            }
            else
            {
                CompoundTableTab.Visibility = Visibility.Visible;
            }

            // Tell the control palette that the difficulty changed
            PrimaryPalette.RefreshPalette(value);

            return true;
        }

        public MainPage()
        {
            // Required to initialize variables
            InitializeComponent();

            // Set the workspace for the equation editor and other controls
            m_workspace.Equations.Add(new ChemProV.Logic.Equations.EquationModel());
            WorkSpace.EquationEditor.SetWorkspace(m_workspace);
            WorkSpace.CommentsPane.SetWorkspace(m_workspace);
            m_workspace.DegreesOfFreedomAnalysis.PropertyChanged += 
                new PropertyChangedEventHandler(DegreesOfFreedomAnalysis_PropertyChanged);
            m_workspace.DegreesOfFreedomAnalysis.Comments.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(DFCommentsCollectionChanged);
            WorkSpace.SetWorkspace(m_workspace);
            CompoundTable.SetWorkspace(m_workspace);

            // Tell the palette control to update
            PrimaryPalette.RefreshPalette(m_workspace.Difficulty);

            // Monitor when the difficulty changes so we can update the config file
            m_workspace.PropertyChanged += delegate(object o, PropertyChangedEventArgs e)
            {
                if (e.PropertyName.Equals("Difficulty"))
                {
                    OptionDifficultySettingChanged(m_workspace.Difficulty);
                }
            };

            // Monitor when the comments pane visibility changes so we can update tooltips
            WorkSpace.PropertyChanged += delegate(object o, PropertyChangedEventArgs e)
            {
                if (e.PropertyName.Equals("CommentsPaneVisible"))
                {
                    ToolTipService.SetToolTip(CommentPaneButton, WorkSpace.CommentsPaneVisible ?
                        "Hide comments pane" : "Show comments pane");
                }
            };

            if (Application.Current.IsRunningOutOfBrowser)
            {
                Install_Button.Click -= new RoutedEventHandler(InstallButton_Click);
                ButtonToolbar.Children.Remove(Install_Button);
            }

            this.Loaded += new RoutedEventHandler(LoadConfigFile);

            this.MouseLeftButtonDown += new MouseButtonEventHandler(MainPage_MouseButton);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(MainPage_MouseButton);
            this.MouseRightButtonDown += new MouseButtonEventHandler(MainPage_MouseButton);
            this.MouseRightButtonUp += new MouseButtonEventHandler(MainPage_MouseButton);

            if (App.Current.IsRunningOutOfBrowser)
            {
                App.Current.MainWindow.Closing += new EventHandler<ClosingEventArgs>(MainWindow_Closing);
            }

            //listen for selection changes in our children
            WorkSpace.ValidationChecked += new EventHandler(WorkSpace_ValidationChecked);

            CompoundTable.ConstantClicked += new EventHandler(CompoundTable_ConstantClicked);

            //setup timer
            saveTimer.Interval = autoSaveTimeSpan;
            saveTimer.Tick += new EventHandler(autoSave);
            saveTimer.Start();

            //find our version number
            Assembly asm = Assembly.GetExecutingAssembly();
            if (asm.FullName != null)
            {
                AssemblyName assemblyName = new AssemblyName(asm.FullName);
                versionNumber = assemblyName.Version.ToString();
            }

            // Intialize the static App class
            Core.App.Init(this, PrimaryPalette);

            // Make sure that when the equation editor or compounds control gets focus that the 
            // control palette switches back to select mode
            WorkSpace.EquationEditor.GotFocus += delegate(object sender, RoutedEventArgs e)
            {
                Core.App.ControlPalette.SwitchToSelect();
            };
            CompoundTable.GotFocus += delegate(object sender, RoutedEventArgs e)
            {
                Core.App.ControlPalette.SwitchToSelect();
            };

            // Show the debug tab if this is a debug build
#if DEBUG
            DebugTab.Visibility = System.Windows.Visibility.Visible;

            m_workspace.StreamsCollectionChanged += new EventHandler(WorkspaceStreamsCollectionChanged);
            m_workspace.ProcessUnitsCollectionChanged += new EventHandler(WorkspaceProcessUnitsCollectionChanged);
#endif
        }

#if DEBUG
        private void WorkspaceProcessUnitsCollectionChanged(object sender, EventArgs e)
        {
            // Right now we don't monitor any properties of the process units. For more robust debug 
            // functionality in the future this should probably be implemented eventually.

            // Clear and rebuild
            ProcessUnitsDebugNode.Items.Clear();
            foreach (AbstractProcessUnit apu in m_workspace.ProcessUnits)
            {
                ProcessUnitsDebugNode.Items.Add(new TreeViewItem()
                {
                    Header = apu.UIDString + "(" + apu.Label + ")",
                    Tag = apu
                });
            }
        }

        private void WorkspaceStreamsCollectionChanged(object sender, EventArgs e)
        {
            // Remove previous event listeners
            foreach (TreeViewItem child in StreamsDebugNode.Items)
            {
                AbstractStream stream = child.Tag as AbstractStream;
                stream.PropertyChanged -= DebugStream_PropertyChanged;
            }
            
            // Clear and rebuild
            StreamsDebugNode.Items.Clear();
            foreach (AbstractStream stream in m_workspace.Streams)
            {
                StreamsDebugNode.Items.Add(new TreeViewItem()
                {
                    Header = stream.UIDString + "\n(src @ " + stream.SourceLocation.ToString() +
                        ")\n(dst @ " + stream.DestinationLocation.ToString() + ")",
                    
                    Tag = stream
                });

                stream.PropertyChanged += new PropertyChangedEventHandler(DebugStream_PropertyChanged);
            }
        }

        private void DebugStream_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("SourceLocation") || e.PropertyName.Equals("DestinationLocation"))
            {
                AbstractStream stream = sender as AbstractStream;
                
                // Find the tree view item for this stream
                TreeViewItem tvi = null;
                foreach (TreeViewItem tviTemp in StreamsDebugNode.Items)
                {
                    if (object.ReferenceEquals(tviTemp.Tag, stream))
                    {
                        tvi = tviTemp;
                        break;
                    }
                }

                if (null != tvi)
                {
                    tvi.Header = stream.UIDString + "\n(src @ " + stream.SourceLocation.ToString() +
                        ")\n(dst @ " + stream.DestinationLocation.ToString() + ")";
                }
            }
        }
#endif

        public void LoadChemProVFile(Stream stream)
        {
            XDocument doc = XDocument.Load(stream);
            m_workspace.Load(doc);

            //we dont want to load the config file so stop the event from firing
            this.Loaded -= new RoutedEventHandler(LoadConfigFile);
        }

        /// <summary>
        /// This is only called in out of the browser mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object sender, ClosingEventArgs e)
        {
            if (saveTimer.IsEnabled)
            {
                if (MessageBoxResult.Cancel == MessageBox.Show("You have unsaved work are you sure you want to exit?", "Exit?", MessageBoxButton.OKCancel))
                {
                    e.Cancel = true;
                }
            }
        }

        private void WorkSpace_ValidationChecked(object sender, EventArgs e)
        {
            if (!saveTimer.IsEnabled)
            {
                saveTimer.Start();
                Saving_TextBlock.Text = "";
                Saving_TextBlock.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void autoSave(object sender, EventArgs e)
        {
            // Don't save an empty workspace, as that would be pointless
            if (m_workspace.IsEmpty)
            {
                return;
            }
            
            Saving_TextBlock.Text = "Auto Saving...";
            Saving_TextBlock.Visibility = System.Windows.Visibility.Visible;
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(c_autoSaveFileName, FileMode.OpenOrCreate, isf))
                {
                    SaveChemProVFile(isfs);
                }
            }
            Saving_TextBlock.Text = "Last auto save was at " + DateTime.Now.ToString("t");
        }

        private void LoadConfigFile(object sender, RoutedEventArgs e)
        {
            /////////////////////////////////////////////////////////////////////////////////
            //This commented out line is testing purposes it will remove the configFile file so we can see what happens when nothing is there
            //or when it is corrupted
            //IsolatedStorageFile.GetUserStoreForApplication().DeleteFile(configFile);
            ////////////////////////////////////////////////////////////////////////////////

            bool loadConfigFile = true;

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(c_autoSaveFileName))
                {
                    if (MessageBoxResult.OK == MessageBox.Show("There appears to be an auto-saved file would you like to load it?", "Load saved file", MessageBoxButton.OKCancel))
                    {
                        using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(c_autoSaveFileName, FileMode.OpenOrCreate, isf))
                        {
                            try
                            {
                                LoadChemProVFile(isfs);
                                loadConfigFile = false;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Unable to load file, sorry!");
                                MessageBox.Show(ex.ToString());
                            }
                        }
                        isf.DeleteFile(c_autoSaveFileName);
                    }
                    else
                    {
                        //so they choose not to open the auto save file it will be removed
                        isf.DeleteFile(c_autoSaveFileName);
                    }
                }
                if (loadConfigFile)
                {
                    OptionDifficultySetting loadedSetting;
                    bool showOptionsWindow = false;
                    using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(c_configFile, FileMode.OpenOrCreate, isf))
                    {
                        using (StreamReader sw = new StreamReader(isfs))
                        {
                            string line = sw.ReadLine();
                            try
                            {
                                loadedSetting = (OptionDifficultySetting)Enum.Parse(typeof(OptionDifficultySetting), line, true);
                            }
                            catch
                            {
                                loadedSetting = OptionDifficultySetting.MaterialBalance;
                                showOptionsWindow = true;
                            }
                        }
                    }

                    m_workspace.TrySetDifficulty(loadedSetting);

                    if (showOptionsWindow)
                    {
                        //act like the user just clicked the OptionsButton
                        OptionsButton_Click(this, EventArgs.Empty as RoutedEventArgs);
                    }
                }
            }
        }

        private void CompoundTable_ConstantClicked(object sender, EventArgs e)
        {
            // E.O.
            // This code used to insert the constant into an equation in the equation editor. At 
            // some point we might bring that functionality back.
            //WorkSpace.EquationEditor.InsertConstant((sender as Button).Content as string);
        }

        public void SaveChemProVFile(Stream stream)
        {
            saveTimer.Stop();

            m_workspace.Save(stream, versionNumber);

            // Restart the auto-save timer
            saveTimer.Start();
        }

        private void SavePNG(Stream output)
        {
            //when saving to image, we really need to keep track of three things:
            //   1: the drawing drawing_canvas (duh?)
            //   2: the list of equations
            //   3: the feedback messages
            //
            //In order to do this, we need to create one master image that houses
            //all three subcomponents.
            //set 1: find the total size of the image to create:

            //note that we're using the Max height of the equation editor and feedback window
            //as they share the same space so we only need to know the size of the largest.
            int height = (int)WorkSpace.DrawingCanvas.ActualHeight
                       + Math.Max((int)WorkSpace.EquationEditor.ActualHeight, (int)WorkSpace.FeedbackWindow.ActualHeight);

            //width can just be the drawing_canvas as the drawing_canvas is always the largest object
            int width = (int)WorkSpace.DrawingCanvas.ActualWidth;

            //with width and height determined, create our writeable bitmap,
            //along with bitmaps for the drawing drawing_canvas, equation editor, and feedback window
            WriteableBitmap finalBmp = new WriteableBitmap(width, height);
            WriteableBitmap canvasBmp = new WriteableBitmap((int)WorkSpace.DrawingCanvas.ActualWidth, (int)WorkSpace.DrawingCanvas.ActualHeight);
            WriteableBitmap equationBmp = new WriteableBitmap((int)WorkSpace.EquationEditor.ActualWidth, (int)WorkSpace.EquationEditor.ActualHeight);
            WriteableBitmap feedbackBmp = new WriteableBitmap((int)WorkSpace.FeedbackWindow.ActualWidth, (int)WorkSpace.FeedbackWindow.ActualHeight);

            //step 2: tell each bmp to store an image of their respective controls
            canvasBmp.Render(WorkSpace.DrawingCanvas, null);
            canvasBmp.Invalidate();

            equationBmp.Render(WorkSpace.EquationEditor, null);
            equationBmp.Invalidate();

            feedbackBmp.Render(WorkSpace.FeedbackWindow, null);
            feedbackBmp.Invalidate();

            //step 3: compose all sub images into the final image
            //feedback / equations go on top
            for (int x = 0; x < feedbackBmp.PixelWidth; x++)
            {
                for (int y = 0; y < feedbackBmp.PixelHeight; y++)
                {
                    finalBmp.Pixels[y * finalBmp.PixelWidth + x] = feedbackBmp.Pixels[y * feedbackBmp.PixelWidth + x];
                }
            }

            //next to feedback goes equations
            for (int x = 0; x < equationBmp.PixelWidth; x++)
            {
                for (int y = 0; y < equationBmp.PixelHeight; y++)
                {
                    finalBmp.Pixels[y * finalBmp.PixelWidth + (feedbackBmp.PixelWidth + x)] = equationBmp.Pixels[y * equationBmp.PixelWidth + x];
                }
            }

            //finally, do the drawing drawing_canvas
            int verticalOffset = Math.Max((int)WorkSpace.EquationEditor.ActualHeight, (int)WorkSpace.FeedbackWindow.ActualHeight);
            for (int x = 0; x < canvasBmp.PixelWidth; x++)
            {
                for (int y = 0; y < canvasBmp.PixelHeight; y++)
                {
                    finalBmp.Pixels[(y + verticalOffset) * finalBmp.PixelWidth + x] = canvasBmp.Pixels[y * canvasBmp.PixelWidth + x];
                }
            }

            ImageTools.Image foo = finalBmp.ToImage();
            PngEncoder encoder = new PngEncoder();
            encoder.Encode(foo, output);
            output.Flush();
        }

        private void SaveFileAs_BtnClick(object sender, RoutedEventArgs e)
        {
            // If we have a non-null OSBLE state object and we're logged in, then we need to give the 
            // user a choice of saving to disk or to OSBLE
            if (null != Core.App.OSBLEState && Core.App.OSBLEState.IsLoggedIn)
            {
                UI.OSBLE.OSBLEOrDiskWindow win = new UI.OSBLE.OSBLEOrDiskWindow(
                    Core.App.OSBLEState, true);
                win.OnChooseDiskOption += delegate(object o, EventArgs ea)
                {
                    SaveFileToDisk();
                };
                win.Show();
            }
            else
            {
                SaveFileToDisk();
            }
        }

        private void SaveFileToDisk()
        {
            if (null == m_saveDialog)
            {
                m_saveDialog = new SaveFileDialog();
                m_saveDialog.Filter = c_saveFileFilter;
            }
            bool? saveResult = false;

            //BIG NOTE: When debuggin this application, make sure to put a breakpoint
            //AFTER the following TRY/CATCH block.  Otherwise, the application will
            //throw an exception.  This is a known issue with Silverlight.
            try
            {
                saveResult = m_saveDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }

            if (!saveResult.HasValue || !saveResult.Value)
            {
                // If the user didn't click "OK" in the dialog then we just return
                return;
            }

            // Open the output file stream. We will dispose it after writing the file.
            Stream stream = m_saveDialog.OpenFile();

            // For whatever reason, filter indices start at 1. An index of 1 means that we want 
            // to save the regular ChemProV XML.
            if (1 == m_saveDialog.FilterIndex)
            {
                SaveChemProVFile(stream);
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    //remove the temp file because we just saved;
                    if (isf.FileExists(c_autoSaveFileName))
                    {
                        isf.DeleteFile(c_autoSaveFileName);
                    }
                }
            }
            //filter index of 2 means save as PNG
            else if (m_saveDialog.FilterIndex == 2)
            {
                SavePNG(stream);
            }

            // Put the file name info in the save button's tooltip
            ToolTipService.SetToolTip(SaveAsButton, "Save as... (last save was \"" +
                m_saveDialog.SafeFileName + "\")");

            // Dispose the stream
            stream.Dispose();
        }

        /// <summary>
        /// Will open a new file to edit
        /// </summary>
        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            // If we have a non-null OSBLE state object and we're logged in, then we need to give the 
            // user a choice of saving to disk or to OSBLE
            if (null != Core.App.OSBLEState && Core.App.OSBLEState.IsLoggedIn)
            {
                UI.OSBLE.OSBLEOrDiskWindow win = new UI.OSBLE.OSBLEOrDiskWindow(
                    Core.App.OSBLEState, false);
                win.OnChooseDiskOption += delegate(object o, EventArgs ea)
                {
                    OpenFileFromDisk();
                };
                win.Show();
            }
            else
            {
                OpenFileFromDisk();
            }
        }

        private void OpenFileFromDisk()
        {            
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = c_loadFileFilter;
            bool? openFileResult = false;

            openFileResult = openDialog.ShowDialog();

            // Make sure that the user selected a file and clicked OK
            if (!openFileResult.HasValue || !openFileResult.Value)
            {
                return;
            }

            bool openFile = false;
            if (WorkSpace.DrawingCanvas.Children.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Opening a new file will erase the current process flow diagram.  " + 
                    "Click OK to continue or CANCEL to go back and save.  This action cannot be undone.",
                    "Open File Confirmation", MessageBoxButton.OKCancel);
                if (MessageBoxResult.OK == result)
                {
                    openFile = true;
                }
            }
            else
            {
                openFile = true;
            }

            if (openFile)
            {
                WorkSpace.DrawingCanvas.ClearDrawingCanvas();
                
                //delete the tempory file as they do not want it
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isf.FileExists(c_autoSaveFileName))
                    {
                        isf.DeleteFile(c_autoSaveFileName);
                    }
                }

                // Put the file name info in the save button's tooltip
                ToolTipService.SetToolTip(SaveAsButton, "Save as... (file was opened as \"" +
                    openDialog.File.Name + "\")");

                // Make sure we don't have a palette tool active
                PrimaryPalette.SwitchToSelect();

                FileStream fs;
                // Open the file for reading
                try
                {
                    fs = openDialog.File.OpenRead();
                }
                catch (Exception)
                {
                    MessageBox.Show("The specified file could not be opened");
                    return;
                }
                
                // This means we succeeded in opening the file for reading and writing
                LoadChemProVFile(fs);

                // We've loaded, so we're done with the stream
                fs.Dispose();
                fs = null;

                // Tell the drawing canvas to update stream positions now that everything is loaded
                WorkSpace.DrawingCanvas.UpdateAllStreamLocations();
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aw = new AboutWindow();
            aw.Show();
        }

        private void MainPage_MouseButton(object sender, MouseButtonEventArgs e)
        {
            WorkSpace.DrawingCanvas.HasFocus1 = false;
        }

        private void MainPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (WorkSpace.DrawingCanvas.HasFocus1)
            {
                WorkSpace.DrawingCanvas.GotKeyDown(sender, e);
            }
        }

        /// <summary>
        /// Handles the creation of a new file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewFileButton_Click(object sender, RoutedEventArgs e)
        {
            bool clearfile = false;
            //before creating a new file, check to see if our drawing drawing_canvas is not
            //empty.  If so, ask the user if they'd like to save the current file
            //before erasing everything.
            if (WorkSpace.DrawingCanvas.Children.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show("Creating a new file will erase the current process flow diagram.  Click OK to continue or CANCEL to go back and save.  This action will not be undo-able", "New File Confirmation", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    clearfile = true;
                }
            }
            else
            {
                clearfile = true;
            }

            if (clearfile)
            {
                //delete the tempory file as they do not want it
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isf.FileExists(c_autoSaveFileName))
                    {
                        isf.DeleteFile(c_autoSaveFileName);
                    }
                }

                // Call the clear function to clear everything on the page
                m_workspace.Clear();
                Clear();
            }
        }

        private void RedoClick_Click(object sender, RoutedEventArgs e)
        {
            WorkSpace.Redo();
            WorkSpace.CheckRulesForPFD(null, null);
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            WorkSpace.Undo();
            WorkSpace.CheckRulesForPFD(null, null);
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Application.Current.IsRunningOutOfBrowser)
            {
                try
                {
                    Application.Current.Install();
                }
                catch
                {
                    MessageBox.Show("Installation Failed: is it installed already? Please try refreshing this page");
                }
            }
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            OptionWindow optionWindow = new OptionWindow(m_workspace);
            optionWindow.Simplest.IsChecked = OptionDifficultySetting.MaterialBalance == m_workspace.Difficulty;
            optionWindow.Medium.IsChecked = OptionDifficultySetting.MaterialBalanceWithReactors == m_workspace.Difficulty;
            optionWindow.MostComplex.IsChecked = OptionDifficultySetting.MaterialAndEnergyBalance == m_workspace.Difficulty;
            optionWindow.Show();
        }

        /// <summary>
        /// This will set the mainpage up so it is ready to be deleted.
        /// i.e. it will stop listening for events
        /// </summary>
        public void Dispose()
        {
            this.Loaded -= new RoutedEventHandler(LoadConfigFile);

            this.MouseLeftButtonDown -= new MouseButtonEventHandler(MainPage_MouseButton);
            this.MouseLeftButtonUp -= new MouseButtonEventHandler(MainPage_MouseButton);
            this.MouseRightButtonDown -= new MouseButtonEventHandler(MainPage_MouseButton);
            this.MouseRightButtonUp -= new MouseButtonEventHandler(MainPage_MouseButton);

            if (App.Current.IsRunningOutOfBrowser)
            {
                App.Current.MainWindow.Closing -= new EventHandler<ClosingEventArgs>(MainWindow_Closing);
            }

            //unlisten for selection changes in our children
            WorkSpace.ValidationChecked -= new EventHandler(WorkSpace_ValidationChecked);

            CompoundTable.ConstantClicked -= new EventHandler(CompoundTable_ConstantClicked);

            //stop timer
            saveTimer.Tick -= new EventHandler(autoSave);
            saveTimer.Stop();
        }

        public WorkspaceControl WorkspaceReference
        {
            get
            {
                return WorkSpace;
            }
        }

        private void Clear()
        {
            // Clear the text in the degrees of freedom analysis text box
            DFAnalysisTextBox.Text = string.Empty;
            
            // Clear the workspace (which will clear the drawing canvas and equation editor)
            WorkSpace.ClearWorkSpace();

            // Clear the "last save" label
            Saving_TextBlock.Text = string.Empty;
            Saving_TextBlock.Visibility = System.Windows.Visibility.Collapsed;

            // Reset the saving related stuff
            ToolTipService.SetToolTip(SaveAsButton, "Save as...");
        }

        private void btnOSBLELogin_Click(object sender, RoutedEventArgs e)
        {
            LoginWin lw = new LoginWin();
            lw.LoginAttemptCompleted += new EventHandler(OSBLELoginAttemptCompleted);
            //lw.LoginAttemptCompleted += new LoginWindow.LoginAttemptCompletedDelegate(OSBLELoginAttemptCompleted);
            lw.Show();
        }

        void OSBLELoginAttemptCompleted(object sender, EventArgs e)
        {
            LoginWin lw = sender as LoginWin;
            ChemProV.Logic.OSBLE.OSBLEState state = lw.State;

            // Set the global
            Core.App.OSBLEState = state;

            if (state.IsLoggedIn)
            {
                // Show the assignment browser window. It will do the loading if the user chooses 
                // a file to open.
                AssignmentChooserWindow abw = new AssignmentChooserWindow(state, true, false);
                abw.Show();
            }
        }

        private void DFAnalysisTab_GotFocus(object sender, RoutedEventArgs e)
        {
            Core.App.ClosePopup();
        }

        private void Compounds_DF_TabControl_GotFocus(object sender, RoutedEventArgs e)
        {
            Core.App.ClosePopup();
        }

        private void DegreesOfFreedomAnalysis_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (m_ignoreWorkspaceChanges)
            {
                return;
            }

            DegreesOfFreedomAnalysis df = (sender as DegreesOfFreedomAnalysis);
            if (e.PropertyName.Equals("CommentsVisible"))
            {
                if (df.CommentsVisible)
                {
                    DFCommentsBorder.BorderThickness = new Thickness(2.0);
                    DFCommentsButton.Content = "Hide comments";
                }
                else
                {
                    DFCommentsBorder.BorderThickness = new Thickness(0.0);
                    DFCommentsButton.Content = (df.Comments.Count > 0) ?
                        "Show comments" : "Add comments";
                }
            }
            else if (e.PropertyName.Equals("Text"))
            {
                m_ignoreWorkspaceChanges = true;
                DFAnalysisTextBox.Text = df.Text;
                m_ignoreWorkspaceChanges = false;
            }
        }

        private void DFCommentsButton_Click(object sender, RoutedEventArgs e)
        {
            m_workspace.DegreesOfFreedomAnalysis.CommentsVisible =
                !m_workspace.DegreesOfFreedomAnalysis.CommentsVisible;
            
            // If we've made the degrees of freedom analysis comments visible, ensure that the 
            // comments pane is visible as well
            if (m_workspace.DegreesOfFreedomAnalysis.CommentsVisible)
            {
                WorkSpace.CommentsPaneVisible = true;
            }
        }

        private void DFAnalysisTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Prevent events from firing
            m_ignoreWorkspaceChanges = true;

            m_workspace.DegreesOfFreedomAnalysis.Text = DFAnalysisTextBox.Text;
            
            // Watch for events again
            m_ignoreWorkspaceChanges = false;
        }

        private void DFCommentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DegreesOfFreedomAnalysis_PropertyChanged(
                m_workspace.DegreesOfFreedomAnalysis, new PropertyChangedEventArgs("CommentsVisible"));
        }

        /// <summary>
        /// A MainPage object represents an entire UI for creating and editing a workspace. A workspace is a 
        /// data object that can be obtained with this call.
        /// At the time of this writing the Core.Workspace class doesn't yet contain all the relevant data, 
        /// but future refactoring should change this.
        /// </summary>
        public Logic.Workspace GetLogicalWorkspace()
        {
            return m_workspace;
        }

        private void CommentPaneButton_Click(object sender, RoutedEventArgs e)
        {
            WorkSpace.CommentsPaneVisible = !WorkSpace.CommentsPaneVisible;
        }
    }
}