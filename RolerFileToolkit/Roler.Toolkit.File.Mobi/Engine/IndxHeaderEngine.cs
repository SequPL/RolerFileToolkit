using System.IO;
using System.Collections.Generic;
using System.Text;
using Roler.Toolkit.File.Mobi.Entity;

namespace Roler.Toolkit.File.Mobi.Engine
{
    internal static class IndxHeaderEngine
    {
        #region Const String

        public const string Identifier = "INDX";

        #endregion

        public static bool TryRead(Stream stream, long offset, out IndxHeader indxHeader)
        {
            bool result = false;
            indxHeader = null;
            stream.Seek(offset, SeekOrigin.Begin);
            if (stream.CheckStart(4, Identifier))
            {
                indxHeader = Read(stream, offset);
                result = true;
            }

            if (!result)
            {
                stream.Seek(offset, SeekOrigin.Begin);
            }

            return result;
        }

        public static IndxHeader Read(Stream stream, long offset)
        {
            IndxHeader result = new IndxHeader();
            stream.Seek(offset, SeekOrigin.Begin);
            if (stream.TryReadString(4, out string identifier))
            {
                result.Identifier = identifier;
            }
            if (stream.TryReadUint(out uint length))
            {
                result.Length = length;
            }
            if (stream.TryReadUint(out uint indexType))
            {
                result.IndexType = (IndexType)indexType;
            }

            stream.Seek(8, SeekOrigin.Current); //skip 8 unknown bytes.

            if (stream.TryReadUint(out uint idxtStart))
            {
                result.IdxtStart = idxtStart;
            }
            if (stream.TryReadUint(out uint indexCount))
            {
                result.IndexCount = indexCount;
            }
            if (stream.TryReadUint(out uint indexEncoding))
            {
                result.IndexEncoding = (TextEncoding)indexEncoding;
            }
            if (stream.TryReadString(4, out string indexLanguage))
            {
                result.IndexLanguage = indexLanguage;
            }
            if (stream.TryReadUint(out uint totalIndexCount))
            {
                result.TotalIndexCount = totalIndexCount;
            }
            if (stream.TryReadUint(out uint ordtStart))
            {
                result.OrdtStart = ordtStart;
            }
            if (stream.TryReadUint(out uint ligtStart))
            {
                result.LigtStart = ligtStart;
            }

            stream.Seek(8, SeekOrigin.Current); //skip 8 unknown bytes.
            stream.Seek(offset + length, SeekOrigin.Begin); //skip to end.

            return result;
        }

        /// <summary>
        /// Reads INDX entries from the IDXT section within an INDX record.
        /// </summary>
        public static List<IndxEntry> ReadEntries(Stream stream, long recordOffset, IndxHeader header, IList<PalmDBRecord> palmDBRecords)
        {
            var entries = new List<IndxEntry>();
            
            if (header.IdxtStart == 0)
            {
                return entries;
            }

            // Position to IDXT section
            long idxtOffset = recordOffset + header.IdxtStart;
            stream.Seek(idxtOffset, SeekOrigin.Begin);

            // Read IDXT identifier
            if (!stream.TryReadString(4, out string idxtIdentifier) || idxtIdentifier != "IDXT")
            {
                return entries;
            }

            // Read IDXT entries
            for (int i = 0; i < header.IndexCount; i++)
            {
                if (!stream.TryReadUshort(out ushort entryLength))
                {
                    break;
                }

                // Read entry data
                if (!stream.TryReadBytes(entryLength, out byte[] entryData))
                {
                    break;
                }

                var entry = ParseIndexEntry(entryData, header.IndexEncoding);
                if (entry != null)
                {
                    entries.Add(entry);
                }
            }

            return entries;
        }

        private static IndxEntry ParseIndexEntry(byte[] data, TextEncoding encoding)
        {
            if (data == null || data.Length < 4)
            {
                return null;
            }

            var entry = new IndxEntry();
            int position = 0;

            try
            {
                // Read label length (variable-length encoding)
                int labelLength = data[position++];
                
                if (labelLength > data.Length - position)
                {
                    return null;
                }

                // Read label text
                var textEncoding = encoding == TextEncoding.UTF8 ? Encoding.UTF8 : Encoding.GetEncoding(1252);
                entry.Label = textEncoding.GetString(data, position, labelLength);
                position += labelLength;

                // Read offset if available
                if (position + 4 <= data.Length)
                {
                    entry.Offset = ((uint)data[position] << 24) |
                                   ((uint)data[position + 1] << 16) |
                                   ((uint)data[position + 2] << 8) |
                                   (uint)data[position + 3];
                }

                return entry;
            }
            catch
            {
                return null;
            }
        }

        public static void Write(IndxHeader file, Stream stream)
        {
        }

    }
}
