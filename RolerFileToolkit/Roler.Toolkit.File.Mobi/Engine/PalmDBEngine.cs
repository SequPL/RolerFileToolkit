using System.IO;
using System.Text;
using Roler.Toolkit.File.Mobi.Entity;

namespace Roler.Toolkit.File.Mobi.Engine
{
    internal static class PalmDBEngine
    {
        #region Const 

        private const int HeaderByteLength = 72;
        private const int NameByteLength = 32;

        #endregion

        public static PalmDB Read(Stream stream)
        {
            PalmDB result = null;
            stream.Seek(0, SeekOrigin.Begin);
            byte[] buffer = new byte[NameByteLength];
            if (stream.Read(buffer, 0, NameByteLength) == NameByteLength)
            {
                // Extract null-terminated string from name field for better compatibility
                string name = ExtractNullTerminatedString(buffer);
                result = new PalmDB { Name = name };
                
                if (stream.TryReadUshort(out ushort attribute))
                {
                    result.Attribute = (PalmDBAttribute)attribute;
                }
                if (stream.TryReadUshort(out ushort version))
                {
                    result.Version = version;
                }
                
                // Keep original skip - this works correctly for MOBI files
                // Skip: CreationDate(4), ModificationDate(4), LastBackupDate(4), ModificationNumber(4), AppInfoID(4), SortInfoID(4) = 24 bytes
                stream.Seek(4 * 6, SeekOrigin.Current);

                if (stream.TryReadUint(out uint type))
                {
                    result.Type = type;
                }
                if (stream.TryReadUint(out uint creator))
                {
                    result.Creator = creator;
                }
                if (stream.TryReadUint(out uint uniqueIDseed))
                {
                    result.UniqueIDseed = uniqueIDseed;
                }
                if (stream.TryReadUint(out uint nextRecordListID))
                {
                    result.NextRecordListID = nextRecordListID;
                }
                if (stream.TryReadUshort(out ushort recordCount))
                {
                    result.RecordCount = recordCount;
                }

                for (ushort i = 0; i < recordCount; i++)
                {
                    var recordInfo = new PalmDBRecordInfo();
                    if (stream.TryReadUint(out uint recordInfoOffset))
                    {
                        recordInfo.Offset = recordInfoOffset;
                    }
                    if (stream.TryReadByte(out byte recordInfoAttribute))
                    {
                        recordInfo.Attribute = (PalmDBRecordAttribute)recordInfoAttribute;
                    }
                    var recordUniqueIDBuff = new byte[3];
                    if (stream.Read(recordUniqueIDBuff, 0, 3) == 3)
                    {
                        recordInfo.UniqueID = recordUniqueIDBuff.ToUInt32();
                    }
                    result.RecordInfoList.Add(recordInfo);
                }
            }

            return result;
        }

        /// <summary>
        /// Extracts null-terminated string from byte array
        /// </summary>
        /// <param name="buffer">Byte array containing string data</param>
        /// <returns>Extracted string without null terminator and padding</returns>
        private static string ExtractNullTerminatedString(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
                return string.Empty;

            // Find the first null byte
            int nullIndex = -1;
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == 0)
                {
                    nullIndex = i;
                    break;
                }
            }

            // If no null terminator found, use entire buffer
            int length = nullIndex >= 0 ? nullIndex : buffer.Length;
            
            // Remove any trailing null bytes and whitespace
            while (length > 0 && (buffer[length - 1] == 0 || buffer[length - 1] == 32))
            {
                length--;
            }

            return length > 0 ? Encoding.UTF8.GetString(buffer, 0, length) : string.Empty;
        }

        public static void Write(PalmDB file, Stream stream)
        {
        }

    }
}
