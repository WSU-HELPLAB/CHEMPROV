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
using System.Collections.ObjectModel;

namespace ChemProV.PFD.EquationEditor.Tokens
{
    public partial class TokenList : UserControl
    {
        private ObservableCollection<IEquationToken> _itemsSource = new ObservableCollection<IEquationToken>();
        public ObservableCollection<IEquationToken> ItemsSource
        {
            get
            {
                return _itemsSource;
            }
            set
            {
                _itemsSource = value;
                TokenItems.ItemsSource = value;
            }
        }
        public TokenList()
        {
            InitializeComponent();
        }

        private void TokenMouseEnter(object sender, MouseEventArgs e)
        {
            Border token = sender as Border;
            if (token == null)
            {
                return;
            }
            token.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TokenMouseLeave(object sender, MouseEventArgs e)
        {
            Border token = sender as Border;
            if (token == null)
            {
                return;
            }
            token.Background = new SolidColorBrush(Colors.Transparent);
        }
    }
}
