using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace InfiniteModuleReader
{
    class Program
    {
        static void Main(string[] args)
        {
            ReadModule();
        }

        static string ReverseHex(string hex)
        {
            char[] ReversedHex = new char[hex.Length];
            for (int i = 0; i < hex.Length; i += 2)
            {
                ReversedHex[i] = hex[hex.Length - 1 - i - 1];
                ReversedHex[i + 1] = hex[hex.Length -1 - i];
            }
            return new string(ReversedHex);
        }

        static void ReadModule()
        {
            Console.WriteLine("Enter path to module:");
            string ModulePath = Console.ReadLine();
            //string ModulePath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Halo Infinite\\deploy\\any\\globals\\forge\\forge_objects-rtx-new - Copy.module";
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
            module.StringTableOffset = module.ItemCount * 88 + 72;

            int ItemsSize = module.ItemCount * 88;
            byte[] ModuleItems = new byte[ItemsSize];
            fileStream.Read(ModuleItems, 0, ItemsSize);
            fileStream.Seek(8, SeekOrigin.Current); //No idea what these bytes are for
            byte[] ModuleStrings = new byte[module.StringsSize];
            fileStream.Read(ModuleStrings, 0, module.StringsSize);

            Dictionary<int, string> StringList = new Dictionary<int, string>();

            for (int i = 0; i < ItemsSize; i += 88)
            {
                ModuleItem moduleItem = new ModuleItem
                {
                    NameOffset = BitConverter.ToInt32(ModuleItems, i + 64),
                    DataOffset = BitConverter.ToUInt64(ModuleItems, i + 80),
                    GlobalTagId = BitConverter.ToInt32(ModuleItems, i + 40)
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
            }

            StreamWriter output = new StreamWriter(module.ModuleId.ToString() + ".txt");
            foreach (KeyValuePair<int, string> kvp in StringList)
            {
                output.WriteLine(ReverseHex(kvp.Key.ToString("X8")) + " : " + kvp.Value);
            }

            module.PrintInfo();
            fileStream.Dispose();
            fileStream.Close();
            output.Dispose();
            output.Close();
        }
    }
}
