namespace ContainerLib
{
    public interface IMetaDialogSet
    {
        /// <summary>A friendly description of the dialog set.</summary>
        string Name { get; }

        /// <summary>The default "entry" dialog in the set.</summary>
        string Default { get; }
    }
}
