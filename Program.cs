using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;
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
            ReadModule();
            //ReadTag("ability_grapple_hook.grapplehookdefinitiontag");
        }

        static void ReadModule()
        {
            Console.WriteLine("Enter path to module:");
            //string ModulePath = Console.ReadLine();
            string ModulePath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Halo Infinite\\deploy\\any\\globals\\globals-rtx-new.module";
            Console.WriteLine("Search for tag to edit:");
            //string SearchTerm = Console.ReadLine();
            string SearchTerm = "ability_grapple_hook.grapplehookdefinitiontag";
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
                long BlockInsertionPoint = 0;
                uint DataCompressedSize = 0;
                ulong FirstBlockOffset = moduleItem.DataOffset + (ulong)module.FileDataOffset;
                string ShortTagName = TagName.Substring(TagName.LastIndexOf("\\") + 1, TagName.Length - TagName.LastIndexOf("\\") - 2);
                FileStream outputStream = new FileStream(ShortTagName, FileMode.Create);
                if (moduleItem.BlockCount != 0)
                {         
                    for (int y = 0; y < moduleItem.BlockCount; y++)
                    {
                        byte[] BlockBuffer = new byte[20];
                        fileStream.Seek((moduleItem.BlockIndex * 20)  + module.BlockListOffset + (y * 20), 0);
                        //Console.WriteLine("Block Info Location: {0}", fileStream.Position);
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
                        if (y == 1)
                        {
                            BlockInsertionPoint = fileStream.Position;
                            DataCompressedSize = block.CompressedSize;
                        }
                        //Console.WriteLine("Block Location {1}: {0}, Block Size: {2}", fileStream.Position, y + 1, block.CompressedSize); //Insert block back in here
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
                    //Console.WriteLine("The second block list index will be for the data and where you should reinsert the file when compressed");
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

                Tag tag = ReadTag(ShortTagName);

                FileStream TagStream = new FileStream(ShortTagName, FileMode.Open);

                Console.WriteLine("Type an offset and a new value to edit the tag\nWhen finished editing, type Done and the tag will be saved and added back to the file\nRefer to the generated fileinfo.txt for tag info");
                bool Editing = true;
                while (Editing)
                {
                    Console.Write("Offset: ");
                    int OffsetToEdit;
                    string Input = Console.ReadLine();
                    if (Input.ToLower() == "done")
                    {
                        Editing = false;
                        continue;
                    }
                    else
                    {
                        try
                        {
                            OffsetToEdit = int.Parse(Input);
                        }
                        catch
                        {
                            Console.WriteLine("Couldn't parse offset. Make sure you are typing an integer value matching one of the offsets in the generated fileinfo.txt");
                            continue;
                        }
                    }
                    Console.Write("New value: ");
                    string NewValue = Console.ReadLine();
                    if (NewValue.ToLower() == "done")
                    {
                        Editing = false;
                        continue;
                    }
                    else
                    {
                        if (tag.TagValues.Exists(x => x.Offset == OffsetToEdit))
                        {
                            PluginItem TagItem = tag.TagValues.Find(x => x.Offset == OffsetToEdit);
                            switch (TagItem.FieldType)
                            {
                                case PluginField.Real:
                                    try
                                    {
                                        TagItem.Value = float.Parse(NewValue);
                                    }
                                    catch
                                    {
                                        Console.WriteLine("{0} is the wrong type of value for field {1} - it requires a {2}", NewValue, TagItem.Name, TagItem.FieldType);
                                        continue;
                                    }
                                    break;
                                case PluginField.StringID:
                                case PluginField.Int32:
                                case PluginField.Flags32:
                                case PluginField.Enum32:
                                    try
                                    {
                                        TagItem.Value = uint.Parse(NewValue);
                                    }
                                    catch
                                    {
                                        Console.WriteLine("{0} is the wrong type of value for field {1} - it requires a {2}", NewValue, TagItem.Name, TagItem.FieldType);
                                        continue;
                                    }
                                    break;
                                case PluginField.Int16:
                                case PluginField.Flags16:
                                case PluginField.Enum16:
                                    try
                                    {
                                        TagItem.Value = ushort.Parse(NewValue);
                                    }
                                    catch
                                    {
                                        Console.WriteLine("{0} is the wrong type of value for field {1} - it requires a {2}", NewValue, TagItem.Name, TagItem.FieldType);
                                        continue;
                                    }
                                    break;
                                case PluginField.Enum8:
                                case PluginField.Int8:
                                case PluginField.Flags8:
                                    try
                                    {
                                        TagItem.Value = byte.Parse(NewValue);
                                    }
                                    catch
                                    {
                                        Console.WriteLine("{0} is the wrong type of value for field {1} - it requires a {2}", NewValue, TagItem.Name, TagItem.FieldType);
                                        continue;
                                    }
                                    break;
                                case PluginField.TagReference:
                                    Console.WriteLine("For a tag reference, type the Global ID of the tag you want and its class ID, ie effe or bipd");
                                    Console.Write("Global ID: ");
                                    string TempGlobalID = Console.ReadLine();
                                    Console.Write("Class ID: ");
                                    string TempClassID = Console.ReadLine();
                                    try
                                    {
                                        TagReference TagRef = (TagReference)TagItem.Value;
                                        TagRef.GlobalID = int.Parse(TempGlobalID);
                                        byte[] ClassID = new byte[4];
                                        int Pos = 3;
                                        foreach (char c in TempClassID)
                                        {
                                            ClassID[Pos] = (byte)c;
                                            Pos--;
                                        }
                                        TagRef.GroupTag = BitConverter.ToInt32(ClassID);
                                        TagItem.Value = TagRef;
                                    }
                                    catch
                                    {
                                        Console.WriteLine("{0} is the wrong type of value for field {1} - it requires a {2}", NewValue, TagItem.Name, TagItem.FieldType);
                                        continue;
                                    }
                                    break;
                                case PluginField.DataReference:
                                    Console.WriteLine("Data References can't be edited right now, sorry");
                                    continue;
                                case PluginField.RealBounds:
                                    try
                                    {
                                        Console.WriteLine("For a real bounds, you also need a max value");
                                        Console.Write("Max value: ");
                                        string MaxValue = Console.ReadLine();
                                        RealBounds Bounds = new RealBounds
                                        {
                                            MinBound = float.Parse(NewValue),
                                            MaxBound = float.Parse(MaxValue)
                                        };
                                        TagItem.Value = Bounds;
                                    }
                                    catch
                                    {
                                        Console.WriteLine("{0} is the wrong type of value for field {1} - it requires a {2}", NewValue, TagItem.Name, TagItem.FieldType);
                                        continue;
                                    }
                                    break;
                                case PluginField.Vector3D:
                                    try
                                    {
                                        Console.WriteLine("For a vector you also need a value for J and K");
                                        Console.Write("J: ");
                                        string JValue = Console.ReadLine();
                                        Console.Write("K: ");
                                        string KValue = Console.ReadLine();
                                        RealVector3D Vector = new RealVector3D
                                        {
                                            I = float.Parse(NewValue),
                                            J = float.Parse(JValue),
                                            K = float.Parse(KValue)
                                        };
                                        TagItem.Value = Vector;
                                    }
                                    catch
                                    {
                                        Console.WriteLine("{0} is the wrong type of value for field {1} - it requires a {2}", NewValue, TagItem.Name, TagItem.FieldType);
                                        continue;
                                    }
                                    break;
                                default:
                                    Console.WriteLine("Unrecognized field type {0} in Item {1} at offset {2}", TagItem.FieldType, TagItem.Name, TagItem.Offset);
                                    break;
                            }
                            tag.TagValues.Find(x => x.Offset == OffsetToEdit).Value = TagItem.Value;
                            tag.TagValues.Find(x => x.Offset == OffsetToEdit).Modified = true;
                            Console.WriteLine("{0} at offset {1} now has a value of {2}", TagItem.Name, TagItem.Offset, TagItem.Value);
                        }
                    }
                    foreach (PluginItem Item in tag.TagValues)
                    {
                        if (Item.Modified)
                        {
                            TagStream.Seek(Item.Offset + tag.Header.HeaderSize, SeekOrigin.Begin);
                            switch (Item.FieldType)
                            {
                                case PluginField.Real:
                                    TagStream.Write(BitConverter.GetBytes((float)Item.Value));
                                    break;
                                case PluginField.StringID:
                                case PluginField.Int32:
                                case PluginField.Flags32:
                                case PluginField.Enum32:
                                    TagStream.Write(BitConverter.GetBytes((uint)Item.Value));
                                    break;
                                case PluginField.Int16:
                                case PluginField.Flags16:
                                case PluginField.Enum16:
                                    TagStream.Write(BitConverter.GetBytes((ushort)Item.Value));
                                    break;
                                case PluginField.Enum8:
                                case PluginField.Int8:
                                case PluginField.Flags8:
                                    TagStream.WriteByte((byte)Item.Value);
                                    break;
                                case PluginField.TagReference:
                                    TagReference TagRef = (TagReference)Item.Value;
                                    TagStream.Seek(8, SeekOrigin.Current);
                                    TagStream.Write(BitConverter.GetBytes(TagRef.GlobalID));
                                    TagStream.Seek(8, SeekOrigin.Current);
                                    TagStream.Write(BitConverter.GetBytes(TagRef.GroupTag));
                                    break;
                                case PluginField.DataReference:
                                    DataReferenceField DataRef = (DataReferenceField)Item.Value;
                                    TagStream.Seek(20, SeekOrigin.Current);
                                    TagStream.Write(BitConverter.GetBytes(DataRef.Size));
                                    break;
                                case PluginField.RealBounds:
                                    RealBounds Bounds = (RealBounds)Item.Value;
                                    TagStream.Write(BitConverter.GetBytes(Bounds.MinBound));
                                    TagStream.Write(BitConverter.GetBytes(Bounds.MaxBound));
                                    break;
                                case PluginField.Vector3D:
                                    RealVector3D Vector = (RealVector3D)Item.Value;
                                    TagStream.Write(BitConverter.GetBytes(Vector.I));
                                    TagStream.Write(BitConverter.GetBytes(Vector.J));
                                    TagStream.Write(BitConverter.GetBytes(Vector.K));
                                    break;
                                default:
                                    Console.WriteLine("Unrecognized field type {0} in Item {1} at offset {2}", Item.FieldType, Item.Name, Item.Offset);
                                    break;
                            }
                        }
                    }
                }
                byte[] ModifiedTag = new byte[tag.Header.DataSize];
                TagStream.Seek(tag.Header.HeaderSize, SeekOrigin.Begin);
                TagStream.Read(ModifiedTag, 0, (int)tag.Header.DataSize);
                TagStream.Close();
                fileStream.Seek(BlockInsertionPoint, SeekOrigin.Begin);
                byte[] CompressedModifiedTag = Oodle.Compress(ModifiedTag, ModifiedTag.Length, OodleFormat.Kraken, OodleCompressionLevel.Optimal5); //Set to optimal because a smaller file can be put back in but a bigger one is no bueno
                if (CompressedModifiedTag.Length <= DataCompressedSize)
                {
                    fileStream.Write(CompressedModifiedTag);
                    Console.WriteLine("Done!");
                }
                else
                {
                    Console.WriteLine("Compression failed - Could not compress to or below desired size: {0}, the size it got was {1}", CompressedModifiedTag.Length, DataCompressedSize);
                }

                fileStream.Close();
            }

            StreamWriter output = new StreamWriter(Path.GetFileName(ModulePath) + ".txt");
            foreach (KeyValuePair<int, string> kvp in StringList)
            {
                output.WriteLine(Utilities.ReverseHex(kvp.Key.ToString("X8")) + " : " + kvp.Value);
            }

            //module.PrintInfo();
            fileStream.Close();
            output.Close();
        }

        static Tag ReadTag(string FilePath)
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

            PluginReader pluginReader = new PluginReader();
            string PluginToLoad;
            switch (Path.GetExtension(FilePath))
            {
                case "grapplehookdefinitiontag":
                    PluginToLoad = "(sagh)GrappleHookDefinitionTag.xml";
                    break;
                case "weapon":
                    PluginToLoad = "(weap)weapon.xml";
                    break;
                default:
                    throw new Exception("Couldn't find a suitable plugin for tag type " + Path.GetExtension(FilePath));
            }
            List<PluginItem> PluginItems = pluginReader.LoadPlugin(PluginToLoad);
            

            foreach (PluginItem Item in PluginItems)
            {
                switch (Item.FieldType)
                {
                    case PluginField.Real:
                        Item.Value = BitConverter.ToSingle(tag.TagData, Item.Offset);
                        break;
                    case PluginField.StringID:
                    case PluginField.Int32:
                        Item.Value = BitConverter.ToUInt32(tag.TagData, Item.Offset);
                        break;
                    case PluginField.Int16:
                        Item.Value = BitConverter.ToUInt16(tag.TagData, Item.Offset);
                        break;
                    case PluginField.Int8:
                        Item.Value = tag.TagData[Item.Offset];
                        break;
                    case PluginField.TagReference:
                        Item.Value = new TagReference
                        {
                            TypeInfo = BitConverter.ToUInt64(tag.TagData, Item.Offset),
                            GlobalID = BitConverter.ToInt32(tag.TagData, Item.Offset + 8),
                            AssetID = BitConverter.ToInt64(tag.TagData, Item.Offset + 12),
                            GroupTag = BitConverter.ToInt32(tag.TagData, Item.Offset + 20),
                            LocalHandle = BitConverter.ToInt32(tag.TagData, Item.Offset + 24)
                        };
                        break;
                    case PluginField.DataReference:
                        Item.Value = new DataReferenceField
                        {
                            Data = BitConverter.ToUInt64(tag.TagData, Item.Offset),
                            TypeInfo = BitConverter.ToUInt64(tag.TagData, Item.Offset + 8),
                            UnknownProperty = BitConverter.ToInt32(tag.TagData, Item.Offset + 16),
                            Size = BitConverter.ToUInt32(tag.TagData, Item.Offset + 20)
                        };
                        break;
                    case PluginField.RealBounds:
                        Item.Value = new RealBounds
                        {
                            MinBound = BitConverter.ToSingle(tag.TagData, Item.Offset),
                            MaxBound = BitConverter.ToSingle(tag.TagData, Item.Offset + 4)
                        };
                        break;
                    case PluginField.Vector3D:
                        Item.Value = new RealVector3D
                        {
                            I = BitConverter.ToSingle(tag.TagData, Item.Offset),
                            J = BitConverter.ToSingle(tag.TagData, Item.Offset + 4),
                            K = BitConverter.ToSingle(tag.TagData, Item.Offset + 8)
                        };
                        break;
                    case PluginField.Flags8:
                        Item.Value = tag.TagData[Item.Offset];
                        break;
                    case PluginField.Flags16:
                        Item.Value = BitConverter.ToUInt16(tag.TagData, Item.Offset);
                        break;
                    case PluginField.Flags32:
                        Item.Value = BitConverter.ToUInt32(tag.TagData, Item.Offset);
                        break;
                    case PluginField.Enum8:
                        Item.Value = tag.TagData[Item.Offset];
                        break;
                    case PluginField.Enum16:
                        Item.Value = BitConverter.ToUInt16(tag.TagData, Item.Offset);
                        break;
                    case PluginField.Enum32:
                        Item.Value = BitConverter.ToUInt32(tag.TagData, Item.Offset);
                        break;
                    default:
                        break;
                }
            }
            
            fileStream.Close();
            WriteTagInfo(FilePath, tag, PluginItems);
            tag.TagValues = PluginItems;
            return tag;
        }

        static void WriteTagInfo(string FilePath, Tag tag, List<PluginItem> PluginItems)
        {
            StreamWriter TextOutput = new StreamWriter(Path.GetFileName(FilePath) + ".fileinfo" + ".txt")
            {
                AutoFlush = true //Otherwise it caps at 4096 bytes unless you flush manually
            };
            TextOutput.WriteLine("File Header:");
            TextOutput.WriteLine();
            foreach (var a in tag.Header.GetType().GetFields())
            {
                TextOutput.WriteLine("{0} : {1}", a.Name, a.GetValue(tag.Header));
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
            TextOutput.WriteLine();
            TextOutput.WriteLine("Tag Data:");
            TextOutput.WriteLine();
            foreach (PluginItem Item in PluginItems)
            {
                //if (Item.Value != null)
                    TextOutput.WriteLine(Item);
            }
        }
    } 
}
