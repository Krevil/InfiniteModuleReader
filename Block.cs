using System;
using System.Collections.Generic;
using System.Text;

namespace InfiniteModuleReader
{
    public class Block
    {
        public uint CompressedOffset { get; set; }
        public uint CompressedSize { get; set; }
        public uint UncompressedOffset { get; set; }
        public uint UncompressedSize { get; set; }
        public bool Compressed { get; set; }
    }
}
