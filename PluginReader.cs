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
    public class PluginReader
    {
        public List<PluginItem> LoadPlugin(string PluginPath)
        {
            List<PluginItem> PluginItems = new List<PluginItem>();
            XmlDocument PluginXml = new XmlDocument();
            PluginXml.Load(PluginPath);
            XmlNodeList AllNodes = PluginXml.SelectNodes("//*");
            int Position = 0;
            foreach (XmlNode Node in AllNodes)
            {
                switch (Node.Name.ToLower()) //get item names for enums, flags
                {
                    case "plugin": //ignore, we know it's a plugin
                    case "item": //do something else for this you lazy bum
                        break;
                    case "_field_pad":
                    case "_field_skip":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.Padding, Offset = Position });
                        Position += int.Parse(Node.Attributes.GetNamedItem("length").Value);
                        break;
                    case "_field_block_64":
                    case "_field_block_v2":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.TagBlock, Offset = Position });
                        break;
                    case "_field_array":
                    case "_field_struct":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.TagStruct, Offset = Position });
                        break;
                    case "_field_explanation":
                    case "_field_custom":
                    case "_field_comment":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.Comment, Offset = Position });
                        break;
                    case "_field_reference_v2":
                    case "_field_reference_64":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.TagReference, Offset = Position });
                        Position += 28;
                        break;
                    case "_field_angle_bounds":
                    case "_field_fraction_bounds":
                    case "_field_real_bounds":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.RealBounds, Offset = Position });
                        Position += 8;
                        break;
                    case "_field_real_point_2d":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.RealPoint2D, Offset = Position });
                        Position += 8;
                        break;
                    case "_field_real_point_3d":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.RealPoint3D, Offset = Position });
                        Position += 12;
                        break;
                    case "_field_real_vector_2d":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.Vector2D, Offset = Position });
                        Position += 8;
                        break;
                    case "_field_real_vector_3d":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.Vector3D, Offset = Position });
                        Position += 12;
                        break;
                    case "_field_real_quaternion": //i am in hell and this is the devil
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.Quaternion, Offset = Position });
                        Position += 16;
                        break;
                    case "_field_real_plane_2d":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.Plane2D, Offset = Position });
                        Position += 8;
                        break;
                    case "_field_real_plane_3d":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.Plane3D, Offset = Position });
                        Position += 12;
                        break;
                    case "_field_real_euler_angles_2d":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.EulerAngle2D, Offset = Position });
                        Position += 8;
                        break;
                    case "_field_real_euler_angles_3d":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.EulerAngle3D, Offset = Position });
                        Position += 12;
                        break;
                    case "_field_rgb_color":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.RGBColor, Offset = Position });
                        Position += 4;
                        break;
                    case "_field_real_rgb_color":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.RealRGBColor, Offset = Position });
                        Position += 12;
                        break;
                    case "_field_real_argb_color":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.RealARGBColor, Offset = Position });
                        Position += 16;
                        break;
                    case "_field_real":
                    case "_field_real_fraction":
                    case "_field_angle": //do something for this
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.Real, Offset = Position });
                        Position += 4;
                        break;
                    case "_field_int64_integer":
                    case "_field_qword_integer":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.Int64, Offset = Position });
                        Position += 8;
                        break;
                    case "_field_long_integer":
                    case "_field_dword_integer":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.Int32, Offset = Position });
                        Position += 4;
                        break;
                    case "_field_string_id":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.StringID, Offset = Position });
                        Position += 4;
                        break;
                    case "_field_short_integer":
                    case "_field_word_integer":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.Int16, Offset = Position });
                        Position += 2;
                        break;
                    case "_field_char_integer":
                    case "_field_byte_integer":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.Int8, Offset = Position });
                        Position += 1;
                        break;
                    case "_field_point_2d":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.Point2D, Offset = Position });
                        Position += 4;
                        break;
                    case "_field_long_enum":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.Enum32, Offset = Position });
                        Position += 4;
                        break;
                    case "_field_short_enum":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.Enum16, Offset = Position });
                        Position += 2;
                        break;
                    case "_field_char_enum":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.Enum8, Offset = Position });
                        Position += 1;
                        break;
                    case "_field_long_flags":
                    case "_field_long_block_flags":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.Flags32, Offset = Position });
                        Position += 4;
                        break;
                    case "_field_short_flags":
                    case "_field_word_flags":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.Flags16, Offset = Position });
                        Position += 2;
                        break;
                    case "_field_char_flags":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.Flags8, Offset = Position });
                        Position += 28;
                        break;
                    case "_field_data_64":
                    case "_field_data_v2":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.DataReference, Offset = Position });
                        Position += 24;
                        break;
                    case "_field_long_string":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.LongString, Offset = Position });
                        Position += 256;
                        break;
                    case "_field_string":
                        PluginItems.Add(new PluginItem { Name = Node.Attributes.GetNamedItem("name").Value, FieldType = PluginField.String, Offset = Position });
                        Position += 32;
                        break;
                    default:
                        Console.WriteLine("Found unexpected NodeItem: {0}", Node.Name);
                        Console.WriteLine("Offsets may be incorrect");
                        break;
                }
            }
            return PluginItems;
            /*
            foreach (PluginItem pluginItem in PluginItems)
            {
                Console.WriteLine("Item: {0} | Type: {1} | Offset: {2}", pluginItem.Name, pluginItem.FieldType, pluginItem.Offset);
            }
            */
        }
    }

    public class PluginItem
    {
        public string Name;
        public PluginField FieldType;
        public int Offset;
        public object Value;
        public bool Modified;

        public override string ToString()
        {
            return Name.PadRight(75) + " | Type: " + FieldType.ToString().PadRight(25) + " | Offset: " + Offset.ToString().PadRight(15) + " | Value: " + Value;
        }
    }

    public enum PluginField
    {
        Comment,
        Padding,
        Flags8,
        Flags16,
        Flags32,
        Flags64,
        Enum8,
        Enum16,
        Enum32,
        Enum64,
        Int8,
        Int16,
        Int32,
        Int64,
        Real,
        Double,
        RealBounds,
        Point2D,
        Point3D,
        RealPoint2D,
        RealPoint3D,
        Vector2D,
        Vector3D,
        Quaternion,
        Plane2D,
        Plane3D,
        Rectangle2D,
        EulerAngle2D,
        EulerAngle3D,
        RGBColor,
        RealRGBColor,
        RealARGBColor,
        RealHSVColor,
        RealAHSVColor,
        StringID,
        String,
        LongString,
        DataReference,
        TagReference,
        TagBlock,
        TagStruct,
    }
}
