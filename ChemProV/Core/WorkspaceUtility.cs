using System.Collections.Generic;

namespace ChemProV.Core
{
    public static class WorkspaceUtility
    {
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
                    foreach (IStreamData row in cs.PropertiesTable.Rows)
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
