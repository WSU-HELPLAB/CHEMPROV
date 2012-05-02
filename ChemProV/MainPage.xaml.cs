/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using ChemProV.PFD.EquationEditor;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.StickyNote;
using ChemProV.PFD.Streams;
using ChemProV.UI;
using ChemProV.UI.DrawingCanvas;
using ChemProV.UI.UserDefinedVariableWindow;
using ChemProV.Validation.Feedback;
using ImageTools;
using ImageTools.IO.Png;
using ChemProv.Library.OSBLE.Views;
using ChemProv.Library.OsbleServices;

namespace ChemProV
{
    /// <summary>
    /// Each of these builds of the previous therefore order matters.
    /// The first (0) is the simplest the last is the most complex
    /// </summary>
    public enum OptionDifficultySetting
    {
        MaterialBalance = 0,
        MaterialBalanceWithReactors = 1,
        MaterialAndEnergyBalance = 2
    }

    public partial class MainPage : UserControl
    {
        /// <summary>
        /// This requests for a NewBlankMainPage to be created
        /// </summary>
        public event EventHandler RequestNewBlankMainPage = delegate { };
        public event RequestOpenFileEventHandler RequestOpenFile = delegate { };

        private UserDefinedVariableWindow userDefinedVariableWindow;

        private string versionNumber = "";
        private const string saveFileFilter = "ChemProV PFD XML (*.cpml)|*.cpml|Portable Network Graphics (*.png)|*.png";
        private const string loadFileFilter = "ChemProV PFD XML (*.cpml)|*.cpml";
        private const string autoSaveFileName = "autoSave.cpml";
        private const string configFile = "cpv.config";
        private TimeSpan autoSaveTimeSpan = new TimeSpan(0, 1, 0);

        /// <summary>
        /// Use the public version of this unless you want to change it without having everyone know
        /// </summary>
        private OptionDifficultySetting currentDifficultySetting;
        private DispatcherTimer saveTimer = new DispatcherTimer();

        /// <summary>
        /// This gets or sets the current difficulty setting
        /// </summary>
        public OptionDifficultySetting CurrentDifficultySetting
        {
            get { return currentDifficultySetting; }
            set
            {
                if (OptionDifficultySettingChanged(value))
                {
                    currentDifficultySetting = value;

                    if (currentDifficultySetting == OptionDifficultySetting.MaterialBalance)
                    {
                        CompoundTable.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        CompoundTable.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private bool OptionDifficultySettingChanged(OptionDifficultySetting value)
        {
            try
            {
                //tell workspace that the difficultyChanged
                WorkSpace.CurrentDifficultySetting = value;

                //save the change in the config file
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(configFile, FileMode.Create, isf))
                    {
                        using (StreamWriter sr = new StreamWriter(isfs))
                        {
                            sr.WriteLine(value.ToString());
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Use of advance process units or streams detected. Please remove them before changing the setting.");
                return false;
            }

            // Tell the control palette that the difficulty changed
            PrimaryPalette.RefreshPalette(value);

            return true;
        }

        public List<Tuple<string, EquationControl>> UserDefinedVaraibles
        {
            get
            {
                return userDefinedVariableWindow.VariableDictionary;
            }
        }

        public MainPage(FileInfo fileInfo = null)
        {
            // Required to initialize variables
            InitializeComponent();

            userDefinedVariableWindow = new UserDefinedVariableWindow(WorkSpace.IsReadOnly);

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
            WorkSpace.CompoundsUpdated += new EventHandler(WorkSpace_UpdateCompounds);
            WorkSpace.ValidationChecked += new EventHandler(WorkSpace_ValidationChecked);
            userDefinedVariableWindow.UserDefinedVariablesUpdated += new EventHandler(userDefinedVariableWindow_UserDefinedVariablesUpdated);

            CompoundTable.ConstantClicked += new EventHandler(CompoundTable_ConstantClicked);

            //setup timer
            saveTimer.Interval = autoSaveTimeSpan;
            saveTimer.Tick += new EventHandler(autoSave);

            //find our version number
            Assembly asm = Assembly.GetExecutingAssembly();
            if (asm.FullName != null)
            {
                AssemblyName assemblyName = new AssemblyName(asm.FullName);
                versionNumber = assemblyName.Version.ToString();
            }

            if (fileInfo != null)
            {
                OpenFile(fileInfo);
            }

            // E.O.
            // Intialize the static App class
            Core.App.Init(WorkSpace, PrimaryPalette);
        }

        private void OpenFile(FileInfo file)
        {
            FileStream fs = file.OpenRead();
            XDocument doc = XDocument.Load(fs);

            string setting = doc.Element("ProcessFlowDiagram").Attribute("DifficultySetting").Value;
            CurrentDifficultySetting = (OptionDifficultySetting)Enum.Parse(typeof(OptionDifficultySetting), setting, true);

            WorkSpace.LoadXmlElements(doc);

            userDefinedVariableWindow.LoadXmlElements(doc);

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
            if (saveTimer.IsEnabled == false)
            {
                saveTimer.Start();
                Saving_TextBlock.Text = "";
            }
        }

        private void autoSave(object sender, EventArgs e)
        {
            Saving_TextBlock.Text = "Auto Saving...";
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(autoSaveFileName, FileMode.OpenOrCreate, isf))
                {
                    SaveChemProVFile(isfs);
                }
            }
            Saving_TextBlock.Text = "Auto Saved";
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
                if (isf.FileExists(autoSaveFileName))
                {
                    if (MessageBoxResult.OK == MessageBox.Show("There appears to be an auto-saved file would you like to load it?", "Load saved file", MessageBoxButton.OKCancel))
                    {
                        using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(autoSaveFileName, FileMode.OpenOrCreate, isf))
                        {
                            try
                            {
                                XDocument doc = XDocument.Load(isfs);

                                string setting = doc.Element("ProcessFlowDiagram").Attribute("DifficultySetting").Value;
                                CurrentDifficultySetting = (OptionDifficultySetting)Enum.Parse(typeof(OptionDifficultySetting), setting, true);

                                WorkSpace.LoadXmlElements(doc);
                                loadConfigFile = false;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Unable to load file, sorry!");
                                MessageBox.Show(ex.ToString());
                            }
                        }
                        isf.DeleteFile(autoSaveFileName);
                    }
                    else
                    {
                        //so they choose not to open the auto save file it will be removed
                        isf.DeleteFile(autoSaveFileName);
                    }
                }
                if (loadConfigFile)
                {
                    OptionDifficultySetting loadedSetting;
                    bool showOptionsWindow = false;
                    using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(configFile, FileMode.OpenOrCreate, isf))
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

                    CurrentDifficultySetting = loadedSetting;

                    if (showOptionsWindow)
                    {
                        //act like the user just clicked the OptionsButton
                        OptionsButton_Click(this, EventArgs.Empty as RoutedEventArgs);
                    }
                }
            }
        }

        private void userDefinedVariableWindow_UserDefinedVariablesUpdated(object sender, EventArgs e)
        {
            WorkSpace.UserDefinedVariablesUpdated(UserDefinedVaraibles);
        }

        private void WorkSpace_UpdateCompounds(object sender, EventArgs e)
        {
            CompoundTable.UpdateCompounds(WorkSpace.Compounds);
        }

        private void CompoundTable_ConstantClicked(object sender, EventArgs e)
        {
            WorkSpace.EquationEditor.InsertConstant((sender as Button).Content as string);
        }

        private void SaveChemProVFile(Stream stream)
        {
            XmlSerializer canvasSerializer = new XmlSerializer(typeof(DrawingCanvas));
            XmlSerializer equationSerializer = new XmlSerializer(typeof(EquationEditor));
            XmlSerializer feedbackWindowSerializer = new XmlSerializer(typeof(FeedbackWindow));
            XmlSerializer userDefinedVariablesSerializer = new XmlSerializer(typeof(UserDefinedVariableWindow));
            // XmlSerializer userDefinedVariablesSerializer = new XmlSerializer(typeof(EquationEditor));
            //make sure that out XML turns out pretty
            XmlWriterSettings settings = new XmlWriterSettings();
            saveTimer.Stop();
            settings.Indent = true;
            settings.IndentChars = "   ";

            //create our XML writer
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                //root node
                writer.WriteStartElement("ProcessFlowDiagram");

                //version number
                writer.WriteAttributeString("ChemProV.version", versionNumber);

                //setting
                writer.WriteAttributeString("DifficultySetting", currentDifficultySetting.ToString());

                //write drawing_canvas properties
                canvasSerializer.Serialize(writer, WorkSpace.DrawingCanvas);

                //write equations
                equationSerializer.Serialize(writer, WorkSpace.EquationEditor);

                //write feedback
                feedbackWindowSerializer.Serialize(writer, WorkSpace.FeedbackWindow);

                //write userdefinedvariables
                userDefinedVariablesSerializer.Serialize(writer, userDefinedVariableWindow);

                //end root node
                writer.WriteEndElement();
            }
        }

        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = saveFileFilter;
            bool? saveResult = false;

            //BIG NOTE: When debuggin this application, make sure to put a breakpoint
            //AFTER the following TRY/CATCH block.  Otherwise, the application will
            //throw an exception.  This is a known issue with Silverlight.
            try
            {
                saveResult = saveDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
            if (saveResult == true)
            {
                using (Stream stream = saveDialog.OpenFile())
                {
                    //filterIndex of zero corresponds to XML
                    if (saveDialog.FilterIndex == 1)
                    {
                        SaveChemProVFile(stream);
                        stream.Close();
                        using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            //remove the temp file because we just saved;
                            if (isf.FileExists(autoSaveFileName))
                            {
                                isf.DeleteFile(autoSaveFileName);
                            }
                        }
                    }
                    //filter index of 2 means save as PNG
                    else if (saveDialog.FilterIndex == 2)
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
                        encoder.Encode(foo, stream);
                    }
                }
            }
        }

        /// <summary>
        /// Will open a new file to edit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = loadFileFilter;
            bool? openFileResult = false;

            openFileResult = openDialog.ShowDialog();

            if (openFileResult == true)
            {
                bool openFile = false;
                if (WorkSpace.DrawingCanvas.Children.Count > 0)
                {
                    MessageBoxResult result = MessageBox.Show("Opening a new file will erase the current process flow diagram.  Click OK to continue or CANCEL to go back and save.  This action cannot be undone.", "Open File Confirmation", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
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
                    //delete the tempory file as they do not want it
                    using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (isf.FileExists(autoSaveFileName))
                        {
                            isf.DeleteFile(autoSaveFileName);
                        }
                    }

                    RequestOpenFile(this, new RequestOpenFileArgs(openDialog.File));
                }
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
                    if (isf.FileExists(autoSaveFileName))
                    {
                        isf.DeleteFile(autoSaveFileName);
                    }
                }

                this.userDefinedVariableWindow.Window.IsOpen = false;

                //why mess around this will completely reset everything
                RequestNewBlankMainPage(this, EventArgs.Empty);
            }
        }

        private void RedoClick_Click(object sender, RoutedEventArgs e)
        {
            WorkSpace.Redo();
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            WorkSpace.Undo();
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
                    MessageBox.Show("Installation Failed: is it installed already? Try refreshing this page");
                }
            }
        }

        private void UserDefinedVariableButton_Click(object sender, RoutedEventArgs e)
        {
            userDefinedVariableWindow.Window.HorizontalOffset = this.ActualWidth / 2 - 50;
            userDefinedVariableWindow.Window.VerticalOffset = this.ActualHeight / 2 - 50;
            userDefinedVariableWindow.Window.IsOpen = true;
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            OptionWindow optionWindow = new OptionWindow();
            optionWindow.Simplest.IsChecked = OptionDifficultySetting.MaterialBalance == currentDifficultySetting;
            optionWindow.Medium.IsChecked = OptionDifficultySetting.MaterialBalanceWithReactors == currentDifficultySetting;
            optionWindow.MostComplex.IsChecked = OptionDifficultySetting.MaterialAndEnergyBalance == currentDifficultySetting;
            optionWindow.Closed += new EventHandler(optionWindow_Closed);
            optionWindow.Show();
        }

        private void optionWindow_Closed(object sender, EventArgs e)
        {
            OptionWindow optionWindow = sender as OptionWindow;
            optionWindow.Closed -= new EventHandler(optionWindow_Closed);
            if (optionWindow.DialogResult.Value == true)
            {
                CurrentDifficultySetting = optionWindow.OptionSelection;
            }
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
            WorkSpace.CompoundsUpdated -= new EventHandler(WorkSpace_UpdateCompounds);
            WorkSpace.ValidationChecked -= new EventHandler(WorkSpace_ValidationChecked);
            userDefinedVariableWindow.UserDefinedVariablesUpdated -= new EventHandler(userDefinedVariableWindow_UserDefinedVariablesUpdated);

            userDefinedVariableWindow.Window.IsOpen = false;

            CompoundTable.ConstantClicked -= new EventHandler(CompoundTable_ConstantClicked);

            //stop timer
            saveTimer.Tick -= new EventHandler(autoSave);
            saveTimer.Stop();
        }
    }
}