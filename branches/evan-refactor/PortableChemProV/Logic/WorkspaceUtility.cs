using System.Collections.Generic;

namespace ChemProV.Logic
{
    public static class WorkspaceUtility
    {
        public static bool CollectionContainsItemWithText(IList<StickyNote> collection, string text)
        {
            foreach (StickyNote sn in collection)
            {
                if (sn.Text == text)
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Checks to see if a workspace contains a free-floating sticky note with the specified text and 
        /// location.
        /// </summary>
        /// <returns>True if such a sticky note is found, false otherwise.</returns>
        public static bool ContainsFFSNWithValues(Workspace workspace, string text, 
            MathCore.Vector location)
        {
            foreach (StickyNote sn in workspace.StickyNotes)
            {
                if (sn.Text == text && sn.LocationX == location.X &&
                    sn.LocationY == location.Y)
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Utility function to go through all the chemical stream property tables in the workspace 
        /// and through each row in those tables and build a list containing all the unique selected 
        /// compounds. Note that the list will contain any value seen as a SelectedCompound string, 
        /// provided it's not null or empty. This may include items like "Overall" which aren't 
        /// actually compounds.
        /// </summary>
        public static IList<string> GetUniqueSelectedCompounds(Workspace workspace)
        {
            List<string> list = new List<string>();
            foreach (AbstractStream stream in workspace.Streams)
            {
                ChemicalStream cs = stream as ChemicalStream;
                if (null != cs)
                {
                    // Go through all the rows in the properties table
                    foreach (IStreamDataRow row in cs.PropertiesTable.Rows)
                    {
                        string selectedCompound = (row as ChemicalStreamData).SelectedCompound;
                        if (!string.IsNullOrEmpty(selectedCompound) &&
                            !list.Contains(selectedCompound))
                        {
                            list.Add(selectedCompound);
                        }
                    }
                }
            }

            return list;
        }
    }
}
