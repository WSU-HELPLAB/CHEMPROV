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

using ChemProV.UI;
using ChemProV.UI.DrawingCanvas;
using ChemProV.PFD;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Streams.PropertiesTable.Chemical;
using ChemProV.PFD.Streams.PropertiesTable;
using ChemProV.Validation;
using ChemProV.Validation.Rules;
using ChemProV.PFD.EquationEditor;


namespace ChemProV
{
    public partial class MainPage : UserControl
    {
        private string versionNumber = null;
        private RuleManager ruleManager = RuleManager.GetInstance();
        private const string fileFilter = "ChemProV PFD XML (*.cpml)|*.cpml";
        private TableManager tableManager = TableManager.GetInstance();
        private List<Feedback> listOfFeedback = new List<Feedback>();
        private Feedback selectedFeedback;

        /// <summary>
        /// SelectedFeedback is how we do the highlighting for feedback messages
        /// </summary>
        public Feedback SelectedFeedback
        {
            get { return selectedFeedback; }
            set
            {
                
                if (selectedFeedback != null)
                {
                    //we gotta changed the old one back to white
                    
                    //Set the textbox back to white
                    selectedFeedback.boarder.Background = new SolidColorBrush(Colors.White);

                    if (selectedFeedback.target is ChemicalStream)
                    {
                        ChemicalStreamPropertiesTable table = (selectedFeedback.target as ChemicalStream).Table as ChemicalStreamPropertiesTable;

                        //we get the fourth coulmn because that is the column that has the feedback
                        Label tb = (table.PropertiesTable.Columns[4].GetCellContent((table.ItemSource)[0]) as Label);
                        tb.Background = new SolidColorBrush(Colors.White);
                    }
                    else if (selectedFeedback.target is ChemicalStreamPropertiesTable)
                    {
                        ChemicalStreamPropertiesTable table = selectedFeedback.target as ChemicalStreamPropertiesTable;

                        //we get the fourth coulmn because that is the column that has the feedback
                        Label tb = (table.PropertiesTable.Columns[4].GetCellContent((table.ItemSource)[0]) as Label);
                        tb.Background = new SolidColorBrush(Colors.White);
                    }
                    else if (selectedFeedback.target is Equation)
                    {
                        (selectedFeedback.target as Equation).EquationFeedback.Background = new SolidColorBrush(Colors.White);
                    }
                }
                if (value != null)
                {
                    //we gotta changed the new one to yellow
                    value.boarder.Background = new SolidColorBrush(Colors.Yellow);
                    if (value.target is ChemicalStream)
                    {
                        ChemicalStreamPropertiesTable table = (value.target as ChemicalStream).Table as ChemicalStreamPropertiesTable;
                        Label tb = (table.PropertiesTable.Columns[4].GetCellContent((table.ItemSource)[0]) as Label);
                        tb.Background = new SolidColorBrush(Colors.Yellow);
                    }
                    else if (value.target is ChemicalStreamPropertiesTable)
                    {
                        ChemicalStreamPropertiesTable table = value.target as ChemicalStreamPropertiesTable;
                        Label tb = (table.PropertiesTable.Columns[4].GetCellContent((table.ItemSource)[0]) as Label);
                        tb.Background = new SolidColorBrush(Colors.Yellow);
                    }
                    else if (value.target is Equation)
                    {
                        (value.target as Equation).EquationFeedback.Background = new SolidColorBrush(Colors.Yellow);
                    }
                }

                selectedFeedback = value;
            }
        }

        public MainPage()
        {
            // Required to initialize variables
            InitializeComponent();

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
            foreach(UIElement element in uiElements)
            {
                //this works becausee all the elements in uiElements are IPfdElements they just need cast as such
                pfdElements.Add(element as IPfdElement);
            }
             
            ruleManager.Validate(pfdElements, EquationEditor.EquationTokens);
            updateFeedbackWindow(ruleManager.ErrorMessages);
        }

        /// <summary>
        /// This fires whenever a stream is moved since we possible moved onto or off of a processUnit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CheckRulesForIPfdElements(object sender, PfdUpdatedEventArgs e)
        {
            ruleManager.Validate(e.pfdElements, EquationEditor.EquationTokens);
            updateFeedbackWindow(ruleManager.ErrorMessages);
        }

        /// <summary>
        /// This function sets what should be in the feedback window as well as setting the corrosponding object in either the equation textbox or 
        /// </summary>
        /// <param name="messages"></param>
        void updateFeedbackWindow(Dictionary<object, List<string>> messages)
        {
            //Set the SelectedFeedback to null since we will removing everything.
            SelectedFeedback = null;

            //We need to remove the old feedback stuff since we will add the "new" feedback later
            foreach (Feedback fb in listOfFeedback)
            {
                FeedBackStackPanel.Children.Remove(fb.textBlock);
                if (fb.target is ChemicalStream)
                {
                    ChemicalStreamPropertiesTable table = (fb.target as ChemicalStream).Table as ChemicalStreamPropertiesTable;

                    //gotta make a new list or the table will not update
                    ObservableCollection<ChemicalStreamData> source = table.ItemSource;

                    source[0].Feedback = "";
                    source[0].ToolTipMessage = "";
                }
                else if (fb.target is ChemicalStreamPropertiesTable)
                {
                    ChemicalStreamPropertiesTable table = fb.target as ChemicalStreamPropertiesTable;
                    ObservableCollection<ChemicalStreamData> source = table.ItemSource;

                        source[0].Feedback = "";
                        source[0].ToolTipMessage = "";
                }
                else if (fb.target is Equation)
                {
                    Equation equation = fb.target as Equation;
                    equation.EquationFeedback.Text = "";
                    equation.EquationFeedback.Visibility = Visibility.Collapsed;
                }
            }
            listOfFeedback.Clear();
            FeedBackStackPanel.Children.Clear();
            if (messages != null)
            {
                foreach(object key in messages.Keys)
                {
                    //sometimes, the key can be a list of objects.  In this scenario, we need to break
                    //things down further
                    if (key is IEnumerable<IStream>)
                    {
                        foreach (IStream stream in key as IEnumerable<IStream>)
                        {
                            AttachFeedbackMessage(stream, String.Join("\n", messages[key].ToArray()));
                        }
                    }
                    else
                    {
                        AttachFeedbackMessage(key, String.Join("\n", messages[key].ToArray()));
                    }
                }
            }
        }

        private void AttachFeedbackMessage(object target, string message)
        {
            Feedback fb = new Feedback(target, message);
            fb.textBlock.MouseLeftButtonDown += new MouseButtonEventHandler(textBox_MouseLeftButtonDown);
            FeedBackStackPanel.Children.Add(fb.boarder);
            listOfFeedback.Add(fb);
            if (fb.target is ChemicalStream)
            {
                ChemicalStreamPropertiesTable table = (fb.target as ChemicalStream).Table as ChemicalStreamPropertiesTable;
                
                //gotta make a new list or the table will not update
                ObservableCollection<ChemicalStreamData> source = table.ItemSource;

                source[0].Feedback = "[" + message[1] + "]";
                source[0].ToolTipMessage = fb.textBlock.Text;
            }
            if (fb.target is ChemicalStreamPropertiesTable)
            {
                ChemicalStreamPropertiesTable table = fb.target as ChemicalStreamPropertiesTable;
                //gotta make a new list or the table will not update
                ObservableCollection<ChemicalStreamData> source = table.ItemSource;

                source[0].Feedback = "[" + message[1] + "]";
                source[0].ToolTipMessage = fb.textBlock.Text;

            }
            else if (fb.target is Equation)
            {
                Equation equation = fb.target as Equation;

                //make and set tooltip
                Silverlight.Controls.ToolTip tooltip = new Silverlight.Controls.ToolTip();
                equation.EquationFeedback.Text = "[" + message[1] + "]";
                equation.EquationFeedback.Visibility = Visibility.Visible;
                tooltip.InitialDelay = new Duration(new TimeSpan(0, 0, 1));
                tooltip.DisplayTime = new Duration(new TimeSpan(1, 0, 0));
                tooltip.Content = fb.textBlock.Text;
                Silverlight.Controls.ToolTipService.SetToolTip(equation.EquationFeedback, tooltip);
                // .SetToolTip(equation.EquationFeedback, tuple.Item2);
            }
        }


        void textBox_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            //need to find which textbox this in our list of textboxes
            foreach (Feedback fb in listOfFeedback)
            {
                if (fb.textBlock == sender)
                {
                    if (SelectedFeedback == fb)
                    {
                        SelectedFeedback = null;
                        break;
                    }
                    else
                    {
                        SelectedFeedback = fb;
                        break;
                    }
                }
            }
        }


        void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //drawing canvas can't be smaller than the viewport size
            if (DrawingCanvas.Width < DrawingCanvasScollViewer.Width
                ||
                DrawingCanvas.Height < DrawingCanvasScollViewer.Height
            )
            {
                MainPage_SizeChanged(sender, e);
            }
        }

        /// <summary>
        /// Since Canvas object don't auto-resize, this method needs to be called
        /// whenever the main window gets resized so that we can resize our drawing
        /// canvas appropriately.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //set the scroll viewer's size
            double height = WorkspaceGrid.RowDefinitions[0].ActualHeight;
            double width = WorkspaceGrid.ColumnDefinitions[0].ActualWidth;
            DrawingCanvasScollViewer.Width = width;
            DrawingCanvasScollViewer.Height = height;
            DrawingCanvas.Width = width + DrawingCanvas.SizeBuffer;
            DrawingCanvas.Height = height + DrawingCanvas.SizeBuffer;
        }

        /// <summary>
        /// Called whenever the canvas places a new tool.  We use it here to coordinate with
        /// the process unit palette control.  Basically, whenenver the drawing canvas places
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

            //update the change in the drawing canvas
            object tool = ((ProcessUnitPalette)sender).SelectedItem.Data;

            //because the selected tool is passed by reference, we need to create
            //a local copy for placement on the drawing canvas
            if (tool is IProcessUnit)
            {
                DrawingCanvas.SelectedPaletteItem = ProcessUnitFactory.ProcessUnitFromProcessUnit(tool as IProcessUnit);
            }
            else if (tool is IStream)
            {
                DrawingCanvas.SelectedPaletteItem = StreamFactory.StreamFromStreamObject(tool as IStream);
            }
            //if we get to the ELSE, we must not care whether or not we're playing with a 
            //someone else's reference
            else
            {
                DrawingCanvas.SelectedPaletteItem = null;
            }


        }

        //not used, should be removed soon
        private void PrintImg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PrintDocument doc = new PrintDocument();
            doc.PrintPage += (s, args) =>
            {
                args.PageVisual = DrawingCanvas;
            };
            doc.Print("");
        }

        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            XmlSerializer canvasSerializer = new XmlSerializer(typeof(DrawingCanvas));
            XmlSerializer equationSerializer = new XmlSerializer(typeof(EquationEditor));
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = fileFilter;
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

                            //write canvas properties
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
            }
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = fileFilter;
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
                    
                    //tell the drawing canvas to load its new children
                    DrawingCanvas.Children.Clear();
                    DrawingCanvas.LoadXmlElements(doc.Descendants("DrawingCanvas").ElementAt(0));

                    //load the equations
                    EquationEditor.EquationStackPanel.Children.Clear();
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
    }
}