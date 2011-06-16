using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Printing;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Xml;
using System.Collections.ObjectModel;

using Silverlight.Controls;

using ImageTools;
using ImageTools.Helpers;
using ImageTools.IO.Png;

using ChemProV.UI;
using ChemProV.UI.DrawingCanvas;
using ChemProV.PFD;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Streams.PropertiesTable.Chemical;
using ChemProV.PFD.Streams.PropertiesTable;
using ChemProV.Validation;
using ChemProV.Validation.Feedback;
using ChemProV.Validation.Rules;
using ChemProV.PFD.EquationEditor;
using ChemProV.PFD.StickyNote;


namespace ChemProV
{
    public partial class MainPage : UserControl
    {
        private string versionNumber = null;
        private RuleManager ruleManager = RuleManager.GetInstance();
        private const string saveFileFilter = "ChemProV PFD XML (*.cpml)|*.cpml|Portable Network Graphics (*.png)|*.png";
        private const string loadFileFilter = "ChemProV PFD XML (*.cpml)|*.cpml";

        public MainPage()
        {
            // Required to initialize variables
            InitializeComponent();

            if (Application.Current.IsRunningOutOfBrowser)
            {
                Install_Button.Click -= new RoutedEventHandler(InstallButton_Click);
                ButtonToolbar.Children.Remove(Install_Button);
            }

            //listen for selection changes in our children
            PuPalette.SelectionChanged += new EventHandler(PuPalette_PaletteSelectionChanged);
            this.SizeChanged += new SizeChangedEventHandler(MainPage_SizeChanged);
            DrawingCanvas.ToolPlaced += new EventHandler(DrawingCanvas_ToolPlaced);
            DrawingCanvas.PfdUpdated += new PfdUpdatedEventHandler(CheckRulesForIPfdElements);
            DrawingCanvas.SizeChanged += new SizeChangedEventHandler(DrawingCanvas_SizeChanged);

            EquationEditor.EquationTokensChanged += new EventHandler(CheckRulesForEquations);

            //find our version number
            Assembly asm = Assembly.GetExecutingAssembly();
            if (asm.FullName != null)
            {
                AssemblyName assemblyName = new AssemblyName(asm.FullName);
                versionNumber = assemblyName.Version.ToString();
            }
        }


        /// <summary>
        /// This fires when an equation is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CheckRulesForEquations(object sender, EventArgs e)
        {
            var uiElements = from c in DrawingCanvas.Children
                             where c is IPfdElement
                             select c;
            List<IPfdElement> pfdElements = new List<IPfdElement>();
            foreach (UIElement element in uiElements)
            {
                //this works becausee all the elements in uiElements are IPfdElements they just need cast as such
                pfdElements.Add(element as IPfdElement);
            }

            ruleManager.Validate(pfdElements, EquationEditor.EquationTokens);
            FeedbackWindow.updateFeedbackWindow(ruleManager.ErrorMessages);
        }

        /// <summary>
        /// This fires whenever a stream is moved since we possible moved onto or off of a processUnit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CheckRulesForIPfdElements(object sender, PfdUpdatedEventArgs e)
        {
            CompoundTable.UpdateCompounds(e.pfdElements);
            ruleManager.Validate(e.pfdElements, EquationEditor.EquationTokens);
            FeedbackWindow.updateFeedbackWindow(ruleManager.ErrorMessages);
        }



        void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //drawing drawing_canvas can't be smaller than the viewport size
            if (DrawingCanvas.Width < DrawingCanvasScollViewer.Width
                ||
                DrawingCanvas.Height < DrawingCanvasScollViewer.Height
            )
            {
                double height = WorkspaceGrid.RowDefinitions[0].ActualHeight;
                double width = WorkspaceGrid.ColumnDefinitions[0].ActualWidth;
                DrawingCanvas.Width = width + DrawingCanvas.SizeBuffer;
                DrawingCanvas.Height = height + DrawingCanvas.SizeBuffer;
            }
        }

        /// <summary>
        /// Since Canvas object don't auto-resize, this method needs to be called
        /// whenever the main window gets resized so that we can resize our drawing
        /// drawing_canvas appropriately.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //set the drawing_canvas' scroll viewer's size
            double height = WorkspaceGrid.RowDefinitions[0].ActualHeight;
            double width = WorkspaceGrid.ColumnDefinitions[0].ActualWidth;
            DrawingCanvasScollViewer.Width = width;
            DrawingCanvasScollViewer.Height = height;

            //set the feedback and equation window height
            height = WorkspaceGrid.RowDefinitions[2].ActualHeight;

            if (height < 33)
            {
                height = 33;
            }
            FeedbackWindow.Height = height;
            EquationEditor.Height = height;

            //check to see if the drawing drawing_canvas needs to be resized
            DrawingCanvas_SizeChanged(sender, e);
        }

        /// <summary>
        /// Called whenever the drawing_canvas places a new tool.  We use it here to coordinate with
        /// the process unit palette control.  Basically, whenenver the drawing drawing_canvas places
        /// a tool, we reset the tool palette back to the default selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DrawingCanvas_ToolPlaced(object sender, EventArgs e)
        {
            PuPalette.ResetSelection();
        }

        /// <summary>
        /// called whenever the user changes his selection in the process unit palette
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PuPalette_PaletteSelectionChanged(object sender, EventArgs e)
        {

            //update the change in the drawing drawing_canvas
            object tool = ((ProcessUnitPalette)sender).SelectedItem.Data;

            //because the selected tool is passed by reference, we need to create
            //a local copy for placement on the drawing drawing_canvas
            if (tool is IProcessUnit)
            {
                DrawingCanvas.SelectedPaletteItem = ProcessUnitFactory.ProcessUnitFromProcessUnit(tool as IProcessUnit);
            }
            else if (tool is IStream)
            {
                DrawingCanvas.SelectedPaletteItem = StreamFactory.StreamFromStreamObject(tool as IStream);
            }
            else if (((ProcessUnitPalette)sender).SelectedItem.Description == "Sticky Note")
            {
                DrawingCanvas.SelectedPaletteItem = new StickyNote();
            }
            //if we get to the ELSE, we must not care whether or not we're playing with a 
            //someone else's reference
            else
            {
                DrawingCanvas.SelectedPaletteItem = null;
            }


        }

        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            XmlSerializer canvasSerializer = new XmlSerializer(typeof(DrawingCanvas));
            XmlSerializer equationSerializer = new XmlSerializer(typeof(EquationEditor));
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
                MessageBox.Show(ex.ToString());
            }
            if (saveResult == true)
            {
                using (Stream stream = saveDialog.OpenFile())
                {
                    //filterIndex of zero corresponds to XML
                    if (saveDialog.FilterIndex == 1)
                    {
                        //make sure that out XML turns out pretty
                        XmlWriterSettings settings = new XmlWriterSettings();
                        settings.Indent = true;
                        settings.IndentChars = "   ";

                        //create our XML writer
                        using (XmlWriter writer = XmlWriter.Create(stream, settings))
                        {
                            try
                            {
                                //root node
                                writer.WriteStartElement("ProcessFlowDiagram");

                                //version number

                                writer.WriteAttributeString("ChemProV.version", versionNumber);

                                //write drawing_canvas properties
                                canvasSerializer.Serialize(writer, DrawingCanvas);

                                //write equations
                                equationSerializer.Serialize(writer, EquationEditor);

                                //end root node
                                writer.WriteEndElement();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToString());
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
                        int height = (int)DrawingCanvas.ActualHeight
                                   + Math.Max((int)EquationEditor.ActualHeight, (int)FeedbackWindow.ActualHeight);

                        //width can just be the drawing_canvas as the drawing_canvas is always the largest object
                        int width = (int)DrawingCanvas.ActualWidth;
                        
                        //with width and height determined, create our writeable bitmap,
                        //along with bitmaps for the drawing drawing_canvas, equation editor, and feedback window
                        WriteableBitmap finalBmp = new WriteableBitmap(width, height);
                        WriteableBitmap canvasBmp = new WriteableBitmap((int)DrawingCanvas.ActualWidth, (int)DrawingCanvas.ActualHeight);
                        WriteableBitmap equationBmp = new WriteableBitmap((int)EquationEditor.ActualWidth, (int)EquationEditor.ActualHeight);
                        WriteableBitmap feedbackBmp = new WriteableBitmap((int)FeedbackWindow.ActualWidth, (int)FeedbackWindow.ActualHeight);

                        //step 2: tell each bmp to store an image of their respective controls
                        canvasBmp.Render(DrawingCanvas, null);
                        canvasBmp.Invalidate();

                        equationBmp.Render(EquationEditor, null);
                        equationBmp.Invalidate();

                        feedbackBmp.Render(FeedbackWindow, null);
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
                                finalBmp.Pixels[y * finalBmp.PixelWidth + ( feedbackBmp.PixelWidth + x)] = equationBmp.Pixels[y * equationBmp.PixelWidth + x];
                            }
                        }

                        //finally, do the drawing drawing_canvas
                        int verticalOffset = Math.Max((int)EquationEditor.ActualHeight, (int)FeedbackWindow.ActualHeight);
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
            try
            {
                openFileResult = openDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            if (openFileResult == true)
            {
                try
                {

                    FileStream fs = openDialog.File.OpenRead();
                    XDocument doc = XDocument.Load(fs);

                    //clear out previous data
                    DrawingCanvas.ClearDrawingCanvas();
                    EquationEditor.ClearEquations();

                    //tell the drawing drawing_canvas to load its new children
                    DrawingCanvas.LoadXmlElements(doc.Descendants("DrawingCanvas").ElementAt(0));

                    //load the equations
                    EquationEditor.LoadXmlElements(doc.Descendants("EquationEditor").ElementAt(0));

                    //clear any existing messages in the feedback window and rerun the error checker
                    CheckRulesForEquations(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ChemProV Version " + versionNumber);
        }

        private void GridSplitter_MouseMove(object sender, MouseEventArgs e)
        {
            MainPage_SizeChanged(sender, null);
        }

        private void MainPage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DrawingCanvas.HasFocus1 = false;
        }

        private void MainPage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DrawingCanvas.HasFocus1 = false;
        }

        private void MainPage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DrawingCanvas.HasFocus1 = false;
        }

        private void MainPage_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DrawingCanvas.HasFocus1 = false;
        }

        private void MainPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (DrawingCanvas.HasFocus1)
            {
                DrawingCanvas.GotKeyDown(sender, e);
            }
        }

        /// <summary>
        /// Handles the creation of a new file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewFileButton_Click(object sender, RoutedEventArgs e)
        {
            //before creating a new file, check to see if our drawing drawing_canvas is not
            //empty.  If so, ask the user if they'd like to save the current file
            //before erasing everything.
            if (DrawingCanvas.Children.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show("Creating a new file will erase the current process flow diagram.  Click OK to continue or CANCEL to go back and save.", "New File Confirmation", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    //now, clear the drawing drawing_canvas
                    DrawingCanvas.ClearDrawingCanvas();
                    EquationEditor.ClearEquations();

                    //clear any existing messages in the feedback window and rerun the error checker
                    CheckRulesForEquations(this, EventArgs.Empty);

                }
            }
            else
            {
                //clear out the drawing drawing_canvas and equation editor
                DrawingCanvas.ClearDrawingCanvas();
                EquationEditor.ClearEquations();

                //clear any existing messages in the feedback window and rerun the error checker
                CheckRulesForEquations(this, EventArgs.Empty);
            }
        }

        private void RedoClick_Click(object sender, RoutedEventArgs e)
        {
            DrawingCanvas.DrawingCanvasRedo();
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            DrawingCanvas.DrawingCanvasUndo();
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
                    MessageBox.Show("Installation Failed: is it installed already? Try refressing this page");
                }
            }
        }
    }
}