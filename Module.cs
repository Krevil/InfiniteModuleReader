using System;
using System.Collections.Generic;
using System.Text;

namespace InfiniteModuleReader
{
    public class Module
    {
        public string Head { get; set; }
        public int Version { get; set; }
        public long ModuleId { get; set; }
        public int ItemCount { get; set; }
        public int ManifestCount { get; set; }
        public int ResourceIndex { get; set; }
        public int StringsSize { get; set; }
        public int ResourceCount { get; set; }
        public int BlockCount { get; set; }

        public int StringTableOffset { get; set; }
        public int ResourceListOffset { get; set; }
        public int BlockListOffset { get; set; }
        public int FileDataOffset { get; set; }

        public Dictionary<int, string> Strings { get; set; }


        public void PrintInfo()
        {
            Console.WriteLine("Header: " + Head);
            Console.WriteLine("Version: " + Version);
            Console.WriteLine("ModuleId: " + ModuleId);
            Console.WriteLine("Item Count: " + ItemCount);
            Console.WriteLine("Manifest Count: " + ManifestCount);
            Console.WriteLine("Resource Index: " + ResourceIndex);
            Console.WriteLine("Strings Size: " + StringsSize);
            Console.WriteLine("Resource Count: " + ResourceCount);
            Console.WriteLine("Block Count: " + BlockCount);
            Console.WriteLine();
            Console.WriteLine("String Table Offset: 0x" + StringTableOffset.ToString("X8"));
            Console.WriteLine("Resource List Offset: 0x" + ResourceListOffset.ToString("X8"));
            Console.WriteLine("Block List Offset: 0x" + BlockListOffset.ToString("X8"));
            Console.WriteLine("File Data Offset: 0x" + FileDataOffset.ToString("X8"));
        }
    }
}
