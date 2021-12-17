using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using OodleSharp;

namespace InfiniteModuleReader
{
    class Program
    {
        static void Main(string[] args)
        {
            //DecompressModuleItem("E:\\Mod Tools\\HIMU-main\\loadmanifest_bazaar", 383268); //need the size
            //CompressModuleItem("E:\\Mod Tools\\HIMU-main\\loadmanifest_bazaar.decompressed");
            //CompressModuleItem("E:\\CSharp\\InfiniteModuleReader\\bin\\Debug\\netcoreapp3.1\\masterchief_openworld.model_Data");
            //ReadModule();
            ReadTag("ability_grapple_hook.grapplehookdefinitiontag");
        }

        static void ReadModule()
        {
            Console.WriteLine("Enter path to module:");
            //string ModulePath = Console.ReadLine();
            string ModulePath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Halo Infinite\\deploy\\any\\globals\\globals-rtx-new - Copy.module";
            Console.WriteLine("Search for tag to edit:");
            //string SearchTerm = Console.ReadLine();
            string SearchTerm = "masterchief_openworld.biped";
            FileStream fileStream = new FileStream(ModulePath, FileMode.Open);
            byte[] ModuleHeader = new byte[72];
            fileStream.Read(ModuleHeader, 0, 72);
            Module module = new Module
            {
                Head = Encoding.ASCII.GetString(ModuleHeader, 0, 4),
                Version = BitConverter.ToInt32(ModuleHeader, 4),
                ModuleId = BitConverter.ToInt64(ModuleHeader, 8),
                ItemCount = BitConverter.ToInt32(ModuleHeader, 16),
                ManifestCount = BitConverter.ToInt32(ModuleHeader, 20),
                ResourceIndex = BitConverter.ToInt32(ModuleHeader, 32),
                StringsSize = BitConverter.ToInt32(ModuleHeader, 36),
                ResourceCount = BitConverter.ToInt32(ModuleHeader, 40),
                BlockCount = BitConverter.ToInt32(ModuleHeader, 44)
            };
            module.StringTableOffset = module.ItemCount * 88 + 72; //72 is header size
            module.ResourceListOffset = module.StringTableOffset + module.StringsSize + 8; //Still dunno why these 8 bytes are here
            module.BlockListOffset = module.ResourceCount * 4 + module.ResourceListOffset;
            module.FileDataOffset = module.BlockCount * 20 + module.BlockListOffset; //inaccurate, need to skip past a bunch of 00s
            
            int ItemsSize = module.ItemCount * 88;
            byte[] ModuleItems = new byte[ItemsSize];
            fileStream.Read(ModuleItems, 0, ItemsSize);
            fileStream.Seek(8, SeekOrigin.Current); //No idea what these bytes are for
            byte[] ModuleStrings = new byte[module.StringsSize];
            fileStream.Read(ModuleStrings, 0, module.StringsSize);

            //To fix the data offset
            fileStream.Seek(module.FileDataOffset, SeekOrigin.Begin);
            while (fileStream.ReadByte() == 0)
            {
                continue;
            }
            module.FileDataOffset = fileStream.Position - 1;

            Dictionary<int, string> StringList = new Dictionary<int, string>();

            for (int i = 0; i < ItemsSize; i += 88)
            {
                ModuleItem moduleItem = new ModuleItem
                {
                    ResourceCount = BitConverter.ToInt32(ModuleItems, i),
                    ParentIndex = BitConverter.ToInt32(ModuleItems, i + 4), //Seems to always be 0
                    //unknown int16 8
                    BlockCount = BitConverter.ToInt16(ModuleItems, i + 10),
                    BlockIndex = BitConverter.ToInt32(ModuleItems, i + 12),
                    ResourceIndex = BitConverter.ToInt32(ModuleItems, i + 16),
                    ClassId = BitConverter.ToInt32(ModuleItems, i + 20),
                    DataOffset = BitConverter.ToUInt32(ModuleItems, i + 24), //some special stuff needs to be done here, check back later
                    //unknown int16 30
                    TotalCompressedSize = BitConverter.ToUInt32(ModuleItems, i + 32),
                    TotalUncompressedSize = BitConverter.ToUInt32(ModuleItems, i + 36),
                    GlobalTagId = BitConverter.ToInt32(ModuleItems, i + 40),
                    UncompressedHeaderSize = BitConverter.ToUInt32(ModuleItems, i + 44),
                    UncompressedTagDataSize = BitConverter.ToUInt32(ModuleItems, i + 48),
                    UncompressedResourceDataSize = BitConverter.ToUInt32(ModuleItems, i + 52),
                    HeaderBlockCount = BitConverter.ToInt16(ModuleItems, i + 56),
                    TagDataBlockCount = BitConverter.ToInt16(ModuleItems, i + 58),
                    ResourceBlockCount = BitConverter.ToInt16(ModuleItems, i + 60),
                    //padding
                    NameOffset = BitConverter.ToInt32(ModuleItems, i + 64),
                    //unknown int32 68 //Seems to always be -1
                    AssetChecksum = BitConverter.ToInt64(ModuleItems, i + 72),
                    AssetId = BitConverter.ToInt64(ModuleItems, i + 80)
                };
                if (moduleItem.GlobalTagId == -1)
                {
                    continue;
                }
                ModuleItem moduleItemNext = new ModuleItem();
                string TagName = "";
                if (i + 88 != ItemsSize)
                {
                    moduleItemNext.NameOffset = BitConverter.ToInt32(ModuleItems, i + 88 + 64);
                    TagName = Encoding.ASCII.GetString(ModuleStrings, moduleItem.NameOffset, moduleItemNext.NameOffset - moduleItem.NameOffset);
                }
                else
                {
                    TagName = Encoding.ASCII.GetString(ModuleStrings, moduleItem.NameOffset, module.StringsSize - moduleItem.NameOffset);
                }
                StringList.Add(moduleItem.GlobalTagId, TagName);
                //for testing
                if (!TagName.Contains(SearchTerm))
                {
                    continue;
                }
                if (moduleItem.TotalUncompressedSize == 0)
                {
                    continue;
                }
                ulong FirstBlockOffset = moduleItem.DataOffset + (ulong)module.FileDataOffset;
                string ShortTagName = TagName.Substring(TagName.LastIndexOf("\\") + 1, TagName.Length - TagName.LastIndexOf("\\") - 2);
                FileStream outputStream = new FileStream(ShortTagName, FileMode.Create);
                if (moduleItem.BlockCount != 0)
                {         
                    for (int y = 0; y < moduleItem.BlockCount; y++)
                    {
                        byte[] BlockBuffer = new byte[20];
                        fileStream.Seek((moduleItem.BlockIndex * 20)  + module.BlockListOffset + (y * 20), 0);
                        Console.WriteLine("Block Info Location: {0}", fileStream.Position);
                        fileStream.Read(BlockBuffer, 0, 20);
                        Block block = new Block
                        {
                            CompressedOffset = BitConverter.ToUInt32(BlockBuffer, 0),
                            CompressedSize = BitConverter.ToUInt32(BlockBuffer, 4),
                            UncompressedOffset = BitConverter.ToUInt32(BlockBuffer, 8),
                            UncompressedSize = BitConverter.ToUInt32(BlockBuffer, 12),
                            Compressed = BitConverter.ToBoolean(BlockBuffer, 16)
                        };

                        //This is where it gets ugly-er
                        string CurrentBlock = (y == 0) ? "Header" : "Data";
                        byte[] BlockFile = new byte[block.CompressedSize];
                        ulong BlockOffset = FirstBlockOffset + block.CompressedOffset;
                        fileStream.Seek((long)BlockOffset, 0);
                        Console.WriteLine("Block Location {1}: {0}, Block Size: {2}", fileStream.Position, y + 1, block.CompressedSize); //Insert block back in here
                        fileStream.Read(BlockFile, 0, (int)block.CompressedSize);
                        if (block.Compressed)
                        {
                            byte[] DecompressedFile = Oodle.Decompress(BlockFile, BlockFile.Length, (int)block.UncompressedSize);
                            if (y == 1) //if block is tag data
                            {
                                Console.WriteLine();
                            }
                            outputStream.Write(DecompressedFile);
                            FileStream testStream = new FileStream(ShortTagName + "_" + CurrentBlock, FileMode.Create);
                            testStream.Write(DecompressedFile);
                            testStream.Close();
                        }
                        else //if the block file is uncompressed
                        {
                            outputStream.Write(BlockFile);
                        }
                    }
                    Console.WriteLine("The second block list index will be for the data and where you should reinsert the file when compressed");
                }
                else
                {
                    byte[] CompressedFile = new byte[moduleItem.TotalCompressedSize];
                    fileStream.Seek((int)moduleItem.DataOffset, 0);
                    fileStream.Read(CompressedFile, 0, (int)moduleItem.TotalCompressedSize);
                    byte[] DecompressedFile = Oodle.Decompress(CompressedFile, (int)moduleItem.TotalCompressedSize, (int)moduleItem.TotalUncompressedSize);
                    outputStream.Write(DecompressedFile);
                }
                outputStream.Close();
            }

            StreamWriter output = new StreamWriter(Path.GetFileName(ModulePath) + ".txt");
            foreach (KeyValuePair<int, string> kvp in StringList)
            {
                output.WriteLine(Utilities.ReverseHex(kvp.Key.ToString("X8")) + " : " + kvp.Value);
            }

            module.PrintInfo();
            fileStream.Close();
            output.Close();
        }

        static void ReadTag(string FilePath)
        {
            
            Tag tag = new Tag();
            byte[] TagHeader = new byte[80];
            

            FileStream fileStream = new FileStream(FilePath, FileMode.Open);
            fileStream.Read(TagHeader, 0, 80);
            

            GCHandle HeaderHandle = GCHandle.Alloc(TagHeader, GCHandleType.Pinned);
            tag.Header = (FileHeader)Marshal.PtrToStructure(HeaderHandle.AddrOfPinnedObject(), typeof(FileHeader)); //No idea how this magic bytes to structure stuff works, I just got this from github
            HeaderHandle.Free();

            tag.TagDependencyList = new TagDependency[tag.Header.DependencyCount];
            tag.DataBlockList = new DataBlock[tag.Header.DataBlockCount];
            tag.TagStructList = new TagStruct[tag.Header.TagStructCount];
            tag.DataReferenceList = new DataReference[tag.Header.DataReferenceCount];
            tag.TagReferenceFixupList = new TagReferenceFixup[tag.Header.TagReferenceCount];
            //tag.StringIDList = new StringID[tag.Header.StringIDCount]; //Not sure about the StringIDCount. Needs investigation
            tag.StringTable = new byte[tag.Header.StringTableSize];

            for (long l = 0; l < tag.Header.DependencyCount; l++) //For each tag dependency, fill in its values
            {
                byte[] TagDependencyBytes = new byte[Marshal.SizeOf(tag.TagDependencyList[l])];
                fileStream.Read(TagDependencyBytes, 0, Marshal.SizeOf(tag.TagDependencyList[l]));
                GCHandle TagDependencyHandle = GCHandle.Alloc(TagDependencyBytes, GCHandleType.Pinned);
                tag.TagDependencyList[l] = (TagDependency)Marshal.PtrToStructure(TagDependencyHandle.AddrOfPinnedObject(), typeof(TagDependency));
                TagDependencyHandle.Free();
            }

            for (long l = 0; l < tag.Header.DataBlockCount; l++)
            {
                byte[] DataBlockBytes = new byte[Marshal.SizeOf(tag.DataBlockList[l])];
                fileStream.Read(DataBlockBytes, 0, Marshal.SizeOf(tag.DataBlockList[l]));
                GCHandle DataBlockHandle = GCHandle.Alloc(DataBlockBytes, GCHandleType.Pinned);
                tag.DataBlockList[l] = (DataBlock)Marshal.PtrToStructure(DataBlockHandle.AddrOfPinnedObject(), typeof(DataBlock));
                DataBlockHandle.Free();
            }

            for (long l = 0; l < tag.Header.TagStructCount; l++)
            {
                byte[] TagStructBytes = new byte[Marshal.SizeOf(tag.TagStructList[l])];
                fileStream.Read(TagStructBytes, 0, Marshal.SizeOf(tag.TagStructList[l]));
                GCHandle TagStructHandle = GCHandle.Alloc(TagStructBytes, GCHandleType.Pinned);
                tag.TagStructList[l] = (TagStruct)Marshal.PtrToStructure(TagStructHandle.AddrOfPinnedObject(), typeof(TagStruct));
                TagStructHandle.Free();
            }

            for (long l = 0; l < tag.Header.DataReferenceCount; l++)
            {
                byte[] DataReferenceBytes = new byte[Marshal.SizeOf(tag.DataReferenceList[l])];
                fileStream.Read(DataReferenceBytes, 0, Marshal.SizeOf(tag.DataReferenceList[l]));
                GCHandle DataReferenceHandle = GCHandle.Alloc(DataReferenceBytes, GCHandleType.Pinned);
                tag.DataReferenceList[l] = (DataReference)Marshal.PtrToStructure(DataReferenceHandle.AddrOfPinnedObject(), typeof(DataReference));
                DataReferenceHandle.Free();
            }

            for (long l = 0; l < tag.Header.TagReferenceCount; l++)
            {
                byte[] TagReferenceBytes = new byte[Marshal.SizeOf(tag.TagReferenceFixupList[l])];
                fileStream.Read(TagReferenceBytes, 0, Marshal.SizeOf(tag.TagReferenceFixupList[l]));
                GCHandle TagReferenceHandle = GCHandle.Alloc(TagReferenceBytes, GCHandleType.Pinned);
                tag.TagReferenceFixupList[l] = (TagReferenceFixup)Marshal.PtrToStructure(TagReferenceHandle.AddrOfPinnedObject(), typeof(TagReferenceFixup));
                TagReferenceHandle.Free();
            }
            
            fileStream.Read(tag.StringTable, 0, (int)tag.Header.StringTableSize); //better hope this never goes beyond sizeof(int)


            //Not sure about this stuff, might not be in every tag?
            /*
            if (tag.Header.ZoneSetDataSize > 1)
            {
                byte[] ZoneSetHeader = new byte[16];
                fileStream.Read(ZoneSetHeader, 0, 16);
                GCHandle ZoneSetHandle = GCHandle.Alloc(ZoneSetHeader, GCHandleType.Pinned);
                tag.ZoneSetInfoHeader = (ZoneSetInformationHeader)Marshal.PtrToStructure(ZoneSetHandle.AddrOfPinnedObject(), typeof(ZoneSetInformationHeader));
                ZoneSetHandle.Free();

                tag.ZoneSetEntryList = new ZoneSetEntry[tag.ZoneSetInfoHeader.ZoneSetCount];
                long ZoneSetTagCount = 0;
                foreach (ZoneSetEntry zse in tag.ZoneSetEntryList)
                {
                    ZoneSetTagCount += zse.TagCount;
                }
                tag.ZoneSetTagList = new ZoneSetTag[ZoneSetTagCount];
            }
            */

            fileStream.Seek(tag.Header.StringIDCount, SeekOrigin.Current); //Data starts here after the "StringID" section which is probably something else
            tag.TagData = new byte[tag.Header.DataSize];
            fileStream.Read(tag.TagData, 0, (int)tag.Header.DataSize);
            GrappleHookDefinition grappleHookDefinition = new GrappleHookDefinition();

            GCHandle TagDataHandle = GCHandle.Alloc(tag.TagData, GCHandleType.Pinned);
            grappleHookDefinition = (GrappleHookDefinition)Marshal.PtrToStructure(TagDataHandle.AddrOfPinnedObject(), typeof(GrappleHookDefinition));
            TagDataHandle.Free();


            foreach (var a in grappleHookDefinition.GetType().GetFields())
            {
                Console.WriteLine("{0} : {1}", a.Name, a.GetValue(grappleHookDefinition));
            }
            fileStream.Close();
            //WriteTagInfo(FilePath, tag);
        }

        static void WriteTagInfo(string FilePath, Tag tag)
        {
            StreamWriter TextOutput = new StreamWriter(Path.GetFileName(FilePath) + ".fileinfo" + ".txt")
            {
                AutoFlush = true //Otherwise it caps at 4096 bytes unless you flush manually
            };
            TextOutput.WriteLine("File Header:");
            TextOutput.WriteLine();
            foreach (var a in tag.Header.GetType().GetFields())
            {
                TextOutput.WriteLine("  {0} : {1}", a.Name, a.GetValue(tag.Header));
            }
            TextOutput.WriteLine();
            TextOutput.WriteLine("Tag Depdendencies:");
            TextOutput.WriteLine();
            Utilities.WriteObjectInfo(TextOutput, tag.TagDependencyList, "Tag Dependency");
            TextOutput.WriteLine("Data Blocks:");
            TextOutput.WriteLine();
            Utilities.WriteObjectInfo(TextOutput, tag.DataBlockList, "Data Block");
            TextOutput.WriteLine("Tag Structs:");
            TextOutput.WriteLine();
            Utilities.WriteObjectInfo(TextOutput, tag.TagStructList, "Tag Struct");
            TextOutput.WriteLine("Data References:");
            TextOutput.WriteLine();
            Utilities.WriteObjectInfo(TextOutput, tag.DataReferenceList, "Data Reference");
            TextOutput.WriteLine("Tag Reference Fixups:");
            TextOutput.WriteLine();
            Utilities.WriteObjectInfo(TextOutput, tag.TagReferenceFixupList, "Tag Reference Fixup");
        }
    } 
}
