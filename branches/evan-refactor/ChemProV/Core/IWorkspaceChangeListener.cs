namespace ChemProV.Core
{
    public interface IWorkspaceChangeListener
    {
        void WorkspaceChanged(ChemProV.UI.WorkspaceControl workspace, WorkspaceChangeDetails details);
    }

    public struct WorkspaceChangeDetails
    {
        public bool CompoundsChanged;
        
        public bool EquationsModified;
        
        public bool PFDElementsAddedOrRemoved;

        public bool PFDOtherChange;

        public static readonly WorkspaceChangeDetails AllTrue =
            new WorkspaceChangeDetails()
            {
                CompoundsChanged = true,
                EquationsModified = true,
                PFDElementsAddedOrRemoved = true,
                PFDOtherChange = true
            };
    }
}
