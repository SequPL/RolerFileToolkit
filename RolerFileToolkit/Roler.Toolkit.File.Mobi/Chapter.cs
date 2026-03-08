using System.Collections.Generic;

namespace Roler.Toolkit.File.Mobi
{
    public class Chapter
    {
        public string Title { get; set; }
        public uint FileOffset { get; set; }
        public int Length { get; set; }
        public IList<Chapter> Chapters { get; } = new List<Chapter>();
    }
}
