using System;
using System.Collections.Generic;
using System.Text;

namespace InfiniteModuleReader
{
    public class ModuleItem
    {
        public Module Module { get; }

        public int ResourceCount { get; set; }
        public int ParentIndex { get; set; }
        public byte Flags { get; set; }
        public short BlockCount { get; set; }
        public int BlockIndex { get; set; }
        public int ResourceIndex { get; set; }
        public int ClassId { get; set; } //BigEndian
        public ulong DataOffset { get; set; }
        public uint TotalCompressedSize { get; set; }
        public uint TotalUncompressedSize { get; set; }
        public int GlobalTagId { get; set; }
        public long AssetId { get; set; }
        public long AssetChecksum { get; set; }
        public uint UncompressedHeaderSize { get; set; }
        public int NameOffset { get; set; }
        public uint UncompressedTagDataSize { get; set; }
        public uint UncompressedResourceDataSize { get; set; }
        public short HeaderBlockCount { get; set; }
        public short TagDataBlockCount { get; set; }
        public short ResourceBlockCount { get; set; }

        public string ClassCode => (ClassId == -1) ? null : Encoding.UTF8.GetString(BitConverter.GetBytes(ClassId));

        private string fileName => Module.Strings[NameOffset];

        public string FullPath
        {
            get
            {
                if (GlobalTagId == -1)
                    return fileName;

                var len = fileName.LastIndexOf('.');
                return fileName.Substring(0, len);
            }
        }

    }
}
