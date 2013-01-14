using System;

namespace ChemProV.Logic
{
    public interface IStreamDataRow
    {
        /// <summary>
        /// Returns an object that represents what should be used to build the UI at the 
        /// specified column index. This can be a string if the column is just a text 
        /// value or a collection of strings if the column needs a combo box.
        /// Returns null if the column index is out of range.
        /// </summary>
        object GetColumnUIObject(int columnIndex, out string propertyName);

        string Label
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the cell value at the specified column index. A get for an index 
        /// beyond the last cell will return null.
        /// </summary>
        object this[int columnIndex]
        {
            get;
            set;
        }

        bool UserHasRenamed
        {
            get;
            set;
        }

        void WriteXml(System.Xml.XmlWriter writer);

        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
