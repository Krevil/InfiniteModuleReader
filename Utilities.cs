using System;
using System.IO;
using OodleSharp;
using System.Collections.Generic;
using System.Text;

namespace InfiniteModuleReader
{
    public static class Utilities
    {
        public static string ReverseHex(string hex)
        {
            char[] ReversedHex = new char[hex.Length];
            for (int i = 0; i < hex.Length; i += 2)
            {
                ReversedHex[i] = hex[hex.Length - 1 - i - 1];
                ReversedHex[i + 1] = hex[hex.Length - 1 - i];
            }
            return new string(ReversedHex);
        }

        public static string ReverseString(string s)
        {
            string output = "";
            for (int i = s.Length - 1; i > -1; i--)
            {
                output += s[i];
            }
            return output;
        }

        public static float Angle(float value)
        {
            return (value + 90) % 360;
        }

        public static void WriteObjectInfo(StreamWriter stream, object[] objlist, string objname)
        {
            int i = 1;
            foreach (object obj in objlist)
            {
                stream.WriteLine("{0} {1}:", objname, i);
                foreach (var a in obj.GetType().GetFields())
                {
                    stream.WriteLine("{0} : {1}", a.Name, a.GetValue(obj));
                }
                i++;
                stream.WriteLine();
            }
        }

        public static void WriteObjectInfo(StreamWriter stream, TagDependency[] objlist, string objname)
        {
            int i = 1;
            foreach (object obj in objlist)
            {
                stream.WriteLine("{0} {1}:", objname, i);
                foreach (var a in obj.GetType().GetFields())
                {
                    stream.WriteLine("{0} : {1}", a.Name, a.GetValue(obj));
                }
                i++;
                stream.WriteLine();
            }
        }

        public static void WriteObjectInfo(StreamWriter stream, DataBlock[] objlist, string objname)
        {
            int i = 1;
            foreach (object obj in objlist)
            {
                stream.WriteLine("{0} {1}:", objname, i);
                foreach (var a in obj.GetType().GetFields())
                {
                    stream.WriteLine("{0} : {1}", a.Name, a.GetValue(obj));
                }
                i++;
                stream.WriteLine();
            }
        }

        public static void WriteObjectInfo(StreamWriter stream, TagStruct[] objlist, string objname)
        {
            int i = 1;
            foreach (object obj in objlist)
            {
                stream.WriteLine("{0} {1}:", objname, i);
                foreach (var a in obj.GetType().GetFields())
                {
                    stream.WriteLine("{0} : {1}", a.Name, a.GetValue(obj));
                }
                i++;
                stream.WriteLine();
            }
        }

        public static void WriteObjectInfo(StreamWriter stream, DataReference[] objlist, string objname)
        {
            int i = 1;
            foreach (object obj in objlist)
            {
                stream.WriteLine("{0} {1}:", objname, i);
                foreach (var a in obj.GetType().GetFields())
                {
                    stream.WriteLine("{0} : {1}", a.Name, a.GetValue(obj));
                }
                i++;
                stream.WriteLine();
            }
        }
        public static void WriteObjectInfo(StreamWriter stream, TagReferenceFixup[] objlist, string objname)
        {
            int i = 1;
            foreach (object obj in objlist)
            {
                stream.WriteLine("{0} {1}:", objname, i);
                foreach (var a in obj.GetType().GetFields())
                {
                    stream.WriteLine("{0} : {1}", a.Name, a.GetValue(obj));
                }
                i++;
                stream.WriteLine();
            }
        }

        public static void WriteObjectInfo(StreamWriter stream, TagBlock[] objlist, string objname)
        {
            int i = 1;
            foreach (object obj in objlist)
            {
                stream.WriteLine("{0} {1}:", objname, i);
                foreach (var a in obj.GetType().GetFields())
                {
                    stream.WriteLine("{0} : {1}", a.Name, a.GetValue(obj));
                }
                i++;
                stream.WriteLine();
            }
        }

        public static void WriteObjectInfo(StreamWriter stream, TagReference[] objlist, string objname)
        {
            int i = 1;
            foreach (object obj in objlist)
            {
                stream.WriteLine("{0} {1}:", objname, i);
                foreach (var a in obj.GetType().GetFields())
                {
                    stream.WriteLine("{0} : {1}", a.Name, a.GetValue(obj));
                }
                i++;
                stream.WriteLine();
            }
        }

        public static string GetClassID(int i)
        {
            return ReverseString(Encoding.Default.GetString(BitConverter.GetBytes(i)));
        }

        public static void DecompressModuleItem(string item, int size)
        {
            FileStream fileStream = new FileStream(item, FileMode.Open);
            byte[] File = new byte[fileStream.Length];
            fileStream.Read(File, 0, File.Length);
            fileStream.Close();
            byte[] DecompressedFile = Oodle.Decompress(File, File.Length, size);
            FileStream outputStream = new FileStream(item + ".decompressed", FileMode.Create);
            outputStream.Write(DecompressedFile);
            outputStream.Close();
        }

        public static void CompressModuleItem(string item)
        {
            FileStream fileStream = new FileStream(item, FileMode.Open);
            byte[] File = new byte[fileStream.Length];
            fileStream.Read(File, 0, File.Length);
            fileStream.Close();
            byte[] CompressedFile = Oodle.Compress(File, File.Length, OodleFormat.Kraken, OodleCompressionLevel.Optimal5); //Set to optimal because a smaller file can be put back in but a bigger one is no bueno
            FileStream outputStream = new FileStream(item + ".compressed", FileMode.Create);
            outputStream.Write(CompressedFile);
            outputStream.Close();
        }
    }
}
