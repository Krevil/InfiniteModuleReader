using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace InfiniteModuleReader
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Size = 80)]
    public struct FileHeader
    {
        [FieldOffset(0)]
        public int Magic;

        [FieldOffset(4)]
        public int Version;

        [FieldOffset(8)]
        public ulong UnknownProperty;

        [FieldOffset(16)]
        public ulong AssetChecksum;

        [FieldOffset(24)]
        public int DependencyCount;

        [FieldOffset(28)]
        public int DataBlockCount;

        [FieldOffset(32)]
        public int TagStructCount;

        [FieldOffset(36)]
        public int DataReferenceCOunt;

        [FieldOffset(40)]
        public int TagReferenceCount;

        [FieldOffset(44)]
        public uint StringTableSize;

        [FieldOffset(48)]
        public int StringIDCount;

        [FieldOffset(52)]
        public uint ZoneSetDataSize;

        [FieldOffset(56)]
        public uint HeaderSize;

        [FieldOffset(60)]
        public uint DataSize;

        [FieldOffset(64)]
        public uint ResourceDataSize;

        [FieldOffset(68)]
        public int UnknownProperty2;

        [FieldOffset(72)]
        public byte HeaderAlignment;

        [FieldOffset(73)]
        public byte TagDataAlightment;

        [FieldOffset(74)]
        public byte ResourceDataAligment;

        [FieldOffset(75)]
        public byte UnknownProperty3;

        [FieldOffset(76)]
        public int UnknownProperty4;
    }

    [StructLayout(LayoutKind.Explicit, Size = 24)]
    public struct TagDependency
    {
        [FieldOffset(0)]
        public int GroupTag;

        [FieldOffset(4)]
        public uint NameOffset;

        [FieldOffset(8)]
        public long AssetID;

        [FieldOffset(16)]
        public int GlobalID;

        [FieldOffset(20)]
        public int UnknownProperty;
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct DataBlock
    {
        [FieldOffset(0)]
        public uint Size;

        [FieldOffset(4)]
        public short UnknownProperty;

        [FieldOffset(6)]
        public short Section;

        [FieldOffset(8)]
        public ulong Offset;
    }

    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public struct TagStruct
    {
        [FieldOffset(0)]
        public byte[] GUID;

        [FieldOffset(16)]
        public short Type;

        [FieldOffset(18)]
        public short UnknownPropety;

        [FieldOffset(20)]
        public int TargetIndex;

        [FieldOffset(24)]
        public int FieldBlock;

        [FieldOffset(28)]
        public uint FieldOffset;
    }

    [StructLayout(LayoutKind.Explicit, Size = 20)]
    public struct DataReference
    {
        [FieldOffset(0)]
        public int ParentStructIndex;

        [FieldOffset(4)]
        public int UnknownProperty;

        [FieldOffset(8)]
        public int TargetIndex;

        [FieldOffset(12)]
        public int FieldBlock;

        [FieldOffset(16)]
        public uint FieldOffset;
    }

    [StructLayout(LayoutKind.Explicit, Size = 28)]
    public struct TagReferenceFixup
    {
        [FieldOffset(0)]
        public ulong TypeInfo;

        [FieldOffset(8)]
        public int GlobalID;

        [FieldOffset(12)]
        public long AssetID;

        [FieldOffset(20)]
        public int GroupTag;

        [FieldOffset(24)]
        public int LocalHandle;
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct StringID
    {
        [FieldOffset(0)]
        public uint UnknownProperty;

        [FieldOffset(4)]
        public uint StringOffset;
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct ZoneSetInformationHeader
    {
        [FieldOffset(0)]
        public long ZoneSetCount;

        [FieldOffset(8)]
        public long ZoneSetListOffset;
    }

    [StructLayout(LayoutKind.Explicit, Size = 40)]
    public struct ZoneSetEntry
    {
        [FieldOffset(0)]
        public int StringID;

        [FieldOffset(4)]
        public int UnknownProperty;

        [FieldOffset(8)]
        public long UnknownProperty2;

        [FieldOffset(16)]
        public long UnknownProperty3;

        [FieldOffset(24)]
        public long TagCount;

        [FieldOffset(32)]
        public ulong TagListOffset;
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct ZoneSetTag
    {
        [FieldOffset(0)]
        public uint GlobalID;

        [FieldOffset(4)]
        public int StringID;
    }

    ///
    ///Tag Data
    ///

    [StructLayout(LayoutKind.Explicit, Size = 28)]
    public struct TagBlock
    {
        [FieldOffset(0)]
        public ulong Elements;

        [FieldOffset(8)]
        public ulong TypeInfo;

        [FieldOffset(16)]
        public int Count;

        [FieldOffset(20)]
        public int UnknownProperty;

        [FieldOffset(24)]
        public int UnknownProperty2;
    }

    [StructLayout(LayoutKind.Explicit, Size = 28)]
    public struct TagReference
    {
        [FieldOffset(0)]
        public ulong TypeInfo;

        [FieldOffset(8)]
        public int GlobalID;

        [FieldOffset(12)]
        public long AssetID;

        [FieldOffset(20)]
        public int GroupTag;

        [FieldOffset(24)]
        public int LocalHandle;
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct PageableResource
    {
        [FieldOffset(0)]
        public ulong TypeInfo;

        [FieldOffset(8)]
        public uint Handle;

        [FieldOffset(12)]
        public int UnknownProperty;
    }
}
