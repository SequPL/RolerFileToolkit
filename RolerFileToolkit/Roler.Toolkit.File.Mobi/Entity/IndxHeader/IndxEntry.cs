namespace Roler.Toolkit.File.Mobi.Entity
{
    public class IndxEntry
    {
        /// <summary>
        /// Gets or sets the title/label of the index entry (chapter name).
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the offset in the text where this entry points to.
        /// </summary>
        public uint Offset { get; set; }

        /// <summary>
        /// Gets or sets the length of the content for this entry.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets the parent index (for hierarchical structure).
        /// </summary>
        public int ParentIndex { get; set; } = -1;
    }
}
