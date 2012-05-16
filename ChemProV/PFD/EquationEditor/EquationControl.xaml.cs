/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using ChemProV.PFD.EquationEditor.Tokens;
using ChemProV.Validation;
using Antlr.Runtime;
using System.IO;
using ChemProV.Grammars;
using Antlr.Runtime.Tree;
using ChemProV.PFD.EquationEditor.Models;

namespace ChemProV.PFD.EquationEditor
{
    public partial class EquationControl : UserControl//, IXmlSerializable, INotifyPropertyChanged
    {
        private EquationModel m_model;

        public EquationControl(EquationEditor parent)
        {
            InitializeComponent();
            
            m_model = new EquationModel();
            SetScopeOptions(parent.EquationScopes);
            SetTypeOptions(parent.EquationTypes);
            this.DataContext = m_model;

            // Setup the annotation button
            RefreshAnnotationButton();
            AnnotationButton.Click += delegate(object sender, RoutedEventArgs e)
            {
                PFD.EquationEditor.Views.AnnotateWindow window =
                    new PFD.EquationEditor.Views.AnnotateWindow();
                window.DataContext = m_model;
                window.Show();
            };
            // Monitor when the annotation changes so that we can set the button's icon to a grayed-out sticky 
            // note when the annotation is empty and a yellow sticky note when it's not
            m_model.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                if (e.PropertyName.Equals("Annotation"))
                {
                    // Refresh the button's icon
                    RefreshAnnotationButton();
                }
            };
        }
        
        /// <summary>
        /// Gets or sets the equation text, which is just the text in the equation text box. However, it 
        /// also sets the equation in the model and does relevant updates.
        /// Use this property instead of directly accessing EquationTextBox.
        /// </summary>
        public string EquationText
        {
            get
            {
                return EquationTextBox.Text;
            }
            set
            {
                EquationTextBox.Text = value;
                m_model.Equation = value;
            }
        }

        private void EquationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Update the model
            m_model.Equation = EquationTextBox.Text;
        }

        /// <summary>
        /// Gets a boolean value that indicates whether or not a control within this control 
        /// is the current focused element.
        /// </summary>
        public bool HasFocus
        {
            get
            {
                object focus = FocusManager.GetFocusedElement();
                return object.ReferenceEquals(focus, AnnotationButton) ||
                    object.ReferenceEquals(focus, TypeComboBox) ||
                    object.ReferenceEquals(focus, ScopeComboBox) ||
                    object.ReferenceEquals(focus, EquationTextBox);
            }
        }

        /// <summary>
        /// Gets the equation model. This control exists to provide a user interface relevant to creating 
        /// this equation model.
        /// </summary>
        public EquationModel Model
        {
            get
            {
                return m_model;
            }
        }

        #region Compatibility stuff until further refactoring

        /// <summary>
        /// Compatibility item
        /// Other code references this, but I'm fairly sure all that code is obsolete. But rather than trying 
        /// to do a major clean-out of all unused stuff right now, I'm just going to keep compatibility.
        /// </summary>
        public EquationType SelectedItem
        {
            get { return TypeComboBox.SelectedItem as EquationType; }
        }

        /// <summary>
        /// Compatibility item
        /// (read comment on SelectedItem above... same deal here)
        /// </summary>
        public ObservableCollection<IEquationToken> EquationTokens
        {
            get;
            set;
        }

        public void HighlightFeedback(bool highlight)
        {
            throw new NotImplementedException();
        }

        public void RemoveFeedback()
        {
            throw new NotImplementedException();
        }

        public void SetFeedback(string message, int errorNumber)
        {
            throw new NotImplementedException();
        }

        public string Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        public void LoadFrom(XElement equationModelElement)
        {
            m_model = EquationModel.FromXml(equationModelElement);
            EquationTextBox.Text = m_model.Equation;
            this.DataContext = m_model;

            // Setup the annotation button
            RefreshAnnotationButton();
            // Monitor when the annotation changes so that we can set the button's icon to a grayed-out sticky 
            // note when the annotation is empty and a yellow sticky note when it's not
            m_model.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                if (e.PropertyName.Equals("Annotation"))
                {
                    // Refresh the button's icon
                    RefreshAnnotationButton();
                }
            };

            // Setup the type combo box
            foreach (object typeObj in TypeComboBox.Items)
            {
                if (m_model.Type.Equals(typeObj))
                {
                    TypeComboBox.SelectedItem = typeObj;
                }
            }
            
            // Setup the scope combo box
            foreach (EquationScope item in ScopeComboBox.Items)
            {
                if (item.Equals(m_model.Scope))
                {
                    if (ScopeComboBox.SelectedItem != item)
                    {
                        ScopeComboBox.SelectedItem = item;
                    }
                }
            }

            // Last but not least, the text box
            EquationTextBox.DataContext = m_model;
        }

        private void RefreshAnnotationButton()
        {
            Image img2 = Core.App.CreateImageFromSource(
                string.IsNullOrEmpty(m_model.Annotation) ?
                    "palette_stickyNote_16x16_gray.png" :
                    "palette_stickyNote_16x16.png");
            img2.Width = img2.Height = 16;
            AnnotationButton.Content = img2;
        }

        private void ScopeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_model.Scope = ScopeComboBox.SelectedItem as EquationScope;
        }

        /// <summary>
        /// Refreshes the options in the "Scope" combo box. Preserves the selected value if possible.
        /// </summary>
        public void SetScopeOptions(ObservableCollection<EquationScope> options)
        {
            // We want to avoid firing the SelectionChanged event during this method
            ScopeComboBox.SelectionChanged -= ScopeComboBox_SelectionChanged;
            
            // Before we clear, store the selected element
            object selected = ScopeComboBox.SelectedItem;

            // Clear and rebuild
            this.ScopeComboBox.Items.Clear();
            foreach (EquationScope es in options)
            {
                ScopeComboBox.Items.Add(es);

                // If the item we just added is equal to the previously selected one, then select it
                if (es.Equals(selected))
                {
                    ScopeComboBox.SelectedItem = es;
                }
            }

            // Go back to watching for selection changes
            ScopeComboBox.SelectionChanged += ScopeComboBox_SelectionChanged;
        }

        /// <summary>
        /// Refreshes the options in the "Type" combo box. Preserves the selected value if possible.
        /// </summary>
        public void SetTypeOptions(ObservableCollection<EquationType> options)
        {
            // We want to avoid firing the SelectionChanged event during this method
            TypeComboBox.SelectionChanged -= TypeComboBox_SelectionChanged;
            
            // Before we clear, store the selected element
            object selected = TypeComboBox.SelectedItem;

            // Clear and rebuild
            this.TypeComboBox.Items.Clear();
            foreach (EquationType et in options)
            {
                TypeComboBox.Items.Add(et);

                // If the item we just added is equal to the previously selected one, then select it
                if (et.Equals(selected))
                {
                    TypeComboBox.SelectedItem = et;
                }
            }

            // Go back to watching for selection changes
            TypeComboBox.SelectionChanged += TypeComboBox_SelectionChanged;
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_model.Type = TypeComboBox.SelectedItem as EquationType;
        }

        // ----- I'm doing an entire rewrite of this control -----
        // ----- Until finished, I'm leaving the old code commented out below
        
        //#region Delegates

        //public event PropertyChangedEventHandler PropertyChanged = delegate { };
        //public event EventHandler MyTextChanged = delegate { };
        //public event EventHandler EquationTokensChagned = delegate { };
        //public event EventHandler ReceivedFocus = delegate { };

        //#endregion Delegates

        //#region Fields

        ///// <summary>
        ///// Keeps track of the process unit's unique ID number.  Needed when parsing
        ///// to/from XML for saving and loading
        ///// </summary>
        //private static int equationIdCounter = 0;
        //private string equationId;

        //private bool isReadOnly = false;

        //private ObservableCollection<EquationType> equationTypes = new ObservableCollection<EquationType>();

        //public ObservableCollection<EquationType> EquationTypes
        //{
        //    get { return equationTypes; }
        //}

        //#endregion Fields

        //#region Properties

        //public EquationType SelectedItem
        //{
        //    get { return EquationType.SelectedItem as EquationType; }
        //}

        //public string EquationText
        //{
        //    get
        //    {
        //        return EquationTextBox.Text;
        //    }
        //    set
        //    {
        //        EquationTextBox.Text = value;
        //    }
        //}

        //public bool IsReadOnly
        //{
        //    get { return isReadOnly; }
        //    set
        //    {
        //        isReadOnly = value;
        //        if (IsReadOnly)
        //        {
        //            EquationTextBox.Visibility = System.Windows.Visibility.Collapsed;
        //            TokenListControl.Visibility = System.Windows.Visibility.Visible;
        //        }
        //        else
        //        {
        //            EquationTextBox.Visibility = System.Windows.Visibility.Visible;
        //            TokenListControl.Visibility = System.Windows.Visibility.Collapsed;
        //        }
        //    }
        //}

        ///// <summary>
        ///// Gets or sets the equation's unique ID number
        ///// </summary>
        //public String Id
        //{
        //    get
        //    {
        //        return equationId;
        //    }
        //    set
        //    {
        //        equationId = value;
        //    }
        //}

        //public ObservableCollection<string> VariableNames
        //{
        //    get;
        //    set;
        //}

        //public ObservableCollection<IEquationToken> EquationTokens
        //{
        //    get;
        //    set;
        //}

        //#endregion Properties

        //#region Constructor

        //public EquationControl()
        //{
        //    InitializeComponent();

        //    LocalInit(false);
        //}

        //public EquationControl(bool isReadOnly)
        //{
        //    InitializeComponent();
        //    LocalInit(isReadOnly);
        //}

        //#endregion Constructor

        //#region Initializes

        //private void LocalInit(bool isReadOnly)
        //{
        //    this.isReadOnly = isReadOnly;
        //    this.DataContext = this;
        //    equationIdCounter++;
        //    equationId = "Eq_" + equationIdCounter;
        //    EquationTokens = new ObservableCollection<IEquationToken>();
        //    TokenListControl.ItemsSource = EquationTokens;
        //    VariableNames = new ObservableCollection<string>();
        //    EquationType.SelectionChanged += new SelectionChangedEventHandler(EquationType_SelectionChanged);
        //    EquationTextBox.TextChanged += new TextChangedEventHandler(EquationTextBox_TextChanged);
        //    EquationTextBox.KeyDown += new KeyEventHandler(EquationTextBox_KeyDown);
        //    EquationTextBox.GotFocus += new RoutedEventHandler(EquationTextBox_GotFocus);
        //    this.MouseLeftButtonDown += new MouseButtonEventHandler(EquationClick);
        //}

        //void EquationClick(object sender, MouseButtonEventArgs e)
        //{
        //    ReceivedFocus(this, EventArgs.Empty);
        //}

        //#endregion Initializes

        //#region Public Methods

        ///*
        //public void UpdateEquationTypeComboBox(ObservableCollection<EquationType> newItems)
        //{
        //    ComboBoxEquationTypeItem currentSelected = EquationType.SelectedItem as ComboBoxEquationTypeItem;

        //    equationTypes.Clear();

        //    ComboBoxEquationTypeItem newCurrentSelected = null;
        //    foreach (var item in newItems)
        //    {
        //        ComboBoxEquationTypeItem temp = new ComboBoxEquationTypeItem(item.Classification, item.Content as string);
        //        equationTypes.Add(temp);
        //        if (currentSelected != null && currentSelected.Classification == item.Classification && (currentSelected.Content as string) == (item.Content as string))
        //        {
        //            newCurrentSelected = temp;
        //        }
        //    }

        //    PropertyChanged(this, new PropertyChangedEventArgs("EquationTypes"));

        //    if (newCurrentSelected != null)
        //    {
        //        EquationType.SelectedIndex = equationTypes.IndexOf(newCurrentSelected);
        //    }
        //    else
        //    {
        //        if (equationTypes.Count > 0)
        //        {
        //            EquationType.SelectedIndex = 0;
        //        }
        //        else
        //        {
        //            EquationType.SelectedIndex = -1;
        //        }
        //    }
        //}
        // * */

        ///// <summary>
        ///// highlight or unhighlights the feedback area
        ///// </summary>
        ///// <param name="highlight">true if you want highlight, false if u want to unhighlight</param>
        //public void HighlightFeedback(bool highlight)
        //{
        //    if (highlight)
        //    {
        //        this.EquationFeedback.Background = new SolidColorBrush(Colors.Yellow);
        //    }
        //    else
        //    {
        //        this.EquationFeedback.Background = new SolidColorBrush(Colors.White);
        //    }
        //}

        //public void SetFeedback(string message, int errorNumber)
        //{
        //    //make and set tooltip
        //    DependencyObject parent = this.Parent;
        //    string checkMessage = message;

        //    while (!(parent is EquationEditor))
        //    {
        //        parent = (parent as FrameworkElement).Parent;
        //    }

        //    while (checkMessage[0] != '-')
        //    {
        //        checkMessage = checkMessage.Remove(0, 1);
        //    }
        //    checkMessage = checkMessage.Remove(0, 1);

        //    if (checkMessage.Trim() == ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Solvable).Trim())
        //    {
        //        (parent as EquationEditor).ChangeSolveabiltyStatus(true);
        //    }
        //    else
        //    {
        //        (parent as EquationEditor).ChangeSolveabiltyStatus(false);
        //    }
        //    Silverlight.Controls.ToolTip tooltip = new Silverlight.Controls.ToolTip();

        //    if (EquationFeedback.Text == null || EquationFeedback.Text == "")
        //    {
        //        EquationFeedback.Text = "[" + errorNumber + "]";
        //    }
        //    else
        //    {
        //        EquationFeedback.Text = EquationFeedback.Text.Remove(EquationFeedback.Text.Length - 1) + "," + errorNumber + "]";
        //    }
        //    EquationFeedback.Visibility = Visibility.Visible;
        //    tooltip.InitialDelay = new Duration(new TimeSpan(0, 0, 1));
        //    tooltip.DisplayTime = new Duration(new TimeSpan(1, 0, 0));
        //    tooltip.Content = message;
        //    Silverlight.Controls.ToolTipService.SetToolTip(EquationFeedback, tooltip);
        //}

        //public void RemoveFeedback()
        //{
        //    EquationFeedback.Text = "";
        //    EquationFeedback.Visibility = Visibility.Collapsed;
        //    Silverlight.Controls.ToolTipService.SetToolTip(EquationFeedback, null);
        //}

        //public void EquationTextChanged()
        //{
        //    EquationTextBox_TextChanged(this.EquationTextBox, EventArgs.Empty as TextChangedEventArgs);
        //}

        //#endregion Public Methods

        //#region Private Functions

        //private void EquationType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    EquationTokensChagned(this, EventArgs.Empty);
        //}

        ///// <summary>
        ///// We use this as a hack because TextChanged does not fire if Cntrl V is used so we call it ourselves
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void EquationTextBox_KeyDown(object sender, KeyEventArgs e)
        //{
        //    //if a key combo was pressed that could change the tokens then assume it changed
        //    if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V ||
        //        Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z ||
        //        Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Y ||
        //        Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.X)
        //    {
        //        EquationTextBox_TextChanged(sender, EventArgs.Empty as TextChangedEventArgs);
        //    }
        //}

        //private void EquationTextBox_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    ReceivedFocus(this, e);
        //}

        //private void highlightText(bool isValid)
        //{
        //    //set the color of the background and foreground based on if it is valid or not.
        //    if (isValid)
        //    {
        //        if (isReadOnly)
        //        {
        //            //(EquationTextBox as Label).Background = new SolidColorBrush(Colors.White);
        //            //(EquationTextBox as Label).Foreground = new SolidColorBrush(Colors.Black);
        //        }
        //        else
        //        {
        //            (EquationTextBox as TextBox).Background = new SolidColorBrush(Colors.White);
        //            (EquationTextBox as TextBox).Foreground = new SolidColorBrush(Colors.Black);
        //        }
        //    }
        //    else
        //    {
        //        if (isReadOnly)
        //        {
        //            //(EquationTextBox as Label).Background = new SolidColorBrush(Colors.Red);
        //            //(EquationTextBox as Label).Foreground = new SolidColorBrush(Colors.White);
        //        }
        //        else
        //        {
        //            (EquationTextBox as TextBox).Background = new SolidColorBrush(Colors.Red);
        //            (EquationTextBox as TextBox).Foreground = new SolidColorBrush(Colors.White);
        //        }
        //    }
        //}

        //private void EquationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    //we fire MyTextChanged because SL's textChanged may not have fired if cntrl-V was used
        //    MyTextChanged(this, EventArgs.Empty);

        //    //convert the equation text box into a memory stream to be consumed by the parser
        //    MemoryStream stream = new MemoryStream();
        //    StreamWriter writer = new StreamWriter(stream);
        //    writer.WriteLine(this.EquationTextBox.Text);
        //    writer.Flush();
        //    stream.Position = 0;

        //    //Use our ANTLR grammar to parse the text
        //    try
        //    {
        //        ANTLRInputStream input = new ANTLRInputStream(stream);
        //        ChemProVLexer lexer = new ChemProVLexer(input);
        //        CommonTokenStream tokens = new CommonTokenStream(lexer);
        //        ChemProVParser parser = new ChemProVParser(tokens);
        //        AstParserRuleReturnScope<CommonTree, IToken> result = parser.program();
        //        CommonTree tree = result.Tree;
        //        CommonTreeNodeStream nodes = new CommonTreeNodeStream(tree);
        //        nodes.TokenStream = tokens;
        //        ChemProVTree walker = new ChemProVTree(nodes);
        //        walker.program();
        //    }
        //    catch (Exception ex)
        //    {
        //        //ignore failed parse
        //    }

        //    //close stream writer and memory stream
        //    writer.Close();
        //    stream.Close();

        //    EquationTokensChagned(this, new EventArgs());
        //}

        //#endregion Private Functions

        //#region IXmlSerializable Members

        //public System.Xml.Schema.XmlSchema GetSchema()
        //{
        //    return null;
        //}

        //public void ReadXml(XmlReader reader)
        //{
        //    //parsing of XML data is handled in the main page
        //}

        ///// <summary>
        ///// Called when we try to serialize this object
        ///// </summary>
        ///// <param name="writer"></param>
        //public void WriteXml(XmlWriter writer)
        //{
        //    //simply write what's in our equation text box
        //    writer.WriteAttributeString("EquationText", EquationText);
        //    writer.WriteStartElement("EquationType");
        //    writer.WriteAttributeString("SelectedItemContent", (EquationType.SelectedItem as EquationType).Name as string);
        //    writer.WriteAttributeString("SelectedItemClassification", ((int)(EquationType.SelectedItem as EquationType).Classification).ToString());
        //    writer.WriteEndElement();
        //}

        //#endregion IXmlSerializable Members

        //#region IComparable Members

        //public int CompareTo(object obj)
        //{
        //    if (obj is EquationControl)
        //    {
        //        EquationControl other = obj as EquationControl;
        //        return Id.CompareTo(other.Id);
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //#endregion IComparable Members
    }
}