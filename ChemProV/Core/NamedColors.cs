using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ChemProV.Core
{
    public class NamedColors
    {
        /// <summary>
        /// Array of all possible subgroup colors. You can add or remove colors as you like and 
        /// they will appear as subprocess options when the user right-clicks on a process unit.
        /// </summary>
        public static readonly NamedColor[] All = new NamedColor[]{
            new NamedColor("None (white)", Colors.White), // White is the default
            new NamedColor("Red", Colors.Red), new NamedColor("Green", Colors.Green),
            new NamedColor("Blue", Colors.Blue), new NamedColor("Cyan", Colors.Cyan),
            new NamedColor("Magenta", Colors.Magenta), new NamedColor("Yellow", Colors.Yellow)};
    }

    /// <summary>
    /// Immutable structure for a color with a name.
    /// </summary>
    public struct NamedColor
    {
        private Color m_clr;
        private string m_name;

        public NamedColor(string name, Color color)
        {
            m_clr = color;
            m_name = name;
        }

        public Color Color
        {
            get
            {
                return m_clr;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }
    }
}
