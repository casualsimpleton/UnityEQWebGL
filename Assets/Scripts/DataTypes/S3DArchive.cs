//An attempt to implement a S3D parser inside Unity. I suck at decyphering data files, so this is heavily based on EQ-Zip's work
//CasualSimpleton
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using EQBrowser;

namespace EQBrowser
{
    public class S3DArchive
    {
        #region Helpers/Classes
        public enum Result
        {
            OK,
            InvalidArgument,
            FileNotFound,
            DirectoryNotFound,
            FileTooLarge,
            FileReadError,
            FileWriteError,
            DataError,
            WrongFileType,
            NotImplemented,
            MalformedFile,
            CompressionError,
            DecompressionError,
            OutOfMemory
        }

        public struct Sizes
        {
            public UInt32 Compressed;
            public UInt32 Uncompressed;

            public override string  ToString()
            {
 	             return Compressed.ToString() + "/" + Uncompressed.ToString();
            }
        }

        public static List<string> imageFileExt = new List<string>() { ".dds", ".bmp", ".png", ".jpeg", ".jpg", ".gif", ".tga", ".tiff", ".tif" };
        public static List<string> archiveFileExt = new List<string>() { ".s3d", ".eqg", ".pfs", ".pak" };

        public static byte[] GetFileContents(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                return null;
            }

            byte[] data = null;

            try
            {
                using (System.IO.FileStream fileStream = System.IO.File.OpenRead(filePath))
                {
                    if ((fileStream.Length > 0) && (fileStream.Length <= Int32.MaxValue))
                    {
                        data = new byte[fileStream.Length];

                        if (data != null)
                        {
                            if (fileStream.Read(data, 0, data.Length) != data.Length)
                            {
                                data = null;
                            }
                        }
                    }
                }
            }
            catch (System.Exception exception)
            {
                data = null;
                Debug.LogErrorFormat("S3DArchive Exception: Failed to get file contents from: '{0}'. Exception: {1}", filePath, exception.ToString());
            }

            return data;
        }

        public static bool IsImageFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            return imageFileExt.Contains(System.IO.Path.GetExtension(fileName).ToLower());
        }
        #endregion

        //////////////////////////////////////////////////////////////////////

        public class S3DArchiveEntry
        {
            public class DataChunk
            {
                public byte[] CompressedData;
                public Sizes Size;

                public DataChunk()
                {
                }

                public DataChunk(byte[] data, UInt32 offset, UInt32 compressedSize, UInt32 uncompressedSize, bool isCompressed)
                {
                    if (isCompressed)
                    {
                        CompressedData = new byte[(int)compressedSize];
                        Size.Compressed = compressedSize;
                        Size.Uncompressed = uncompressedSize;

                        Array.Copy(data, (int)offset, CompressedData, 0, (int)compressedSize);
                    }
                    else
                    {
                        CompressFrom(data, offset, uncompressedSize);
                    }
                }

                public UInt32 CompressFrom(byte[] rawData, UInt32 offset, UInt32 rawBytes)
                {
                    using (System.IO.MemoryStream data = new System.IO.MemoryStream(rawData))
                    {
                        data.Seek((int)offset, System.IO.SeekOrigin.Begin);

                        CompressedData = zlib.Compress(data);
                        Size.Compressed = (UInt32)CompressedData.Length;
                        Size.Uncompressed = rawBytes;
                    }

                    return Size.Compressed;
                }

                public UInt32 DecompressTo(byte[] destination, UInt32 offset)
                {
                    using (System.IO.MemoryStream compressed = new System.IO.MemoryStream(CompressedData))
                    {
                        return (UInt32)zlib.Decompress(compressed, destination, (int)offset, (int)Size.Uncompressed);
                    }
                }
            }

            //////////////////////////////////////////////////////////////////////

            protected const UInt32 MAX_BLOCK_SIZE = 8192; //Per System.IO.Compression.Deflate documentation
            protected string _fileName = "(Untitled)";
            protected Texture2D _image;
            protected bool _isImageChecked;
            protected Texture2D _imageThumbnail;

            protected byte[] _uncompressedData;
            protected bool _isUnCompressed;
            protected int _alphaBits = -1;

            public List<DataChunk> CompressedChunks;
            public UInt32 FilePointer;
            public string ImageFormat;
            public string ImageSubFormat;

            public Sizes Size;
            public Result Status;

            public string FileName
            {
                get { return _fileName; }
                set
                {
                    _fileName = string.IsNullOrEmpty(value) ? "(Untitled)" : System.IO.Path.GetFileName(value);
                }
            }

            //////////////////////////////////////////////////////////////////////

            public S3DArchiveEntry() { }
            public S3DArchiveEntry(string filePath)
            {
                if (!string.IsNullOrEmpty(FileName))
                {
                    FileName = filePath;
                    SetContents(GetFileContents(filePath));
                }
            }

            public S3DArchiveEntry(string fileName, byte[] contents)
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    FileName = fileName;
                    SetContents(contents);
                }
            }

            //////////////////////////////////////////////////////////////////////

            public void AddChunk(DataChunk chunk)
            {
                if (chunk == null)
                {
                    Status = Result.InvalidArgument;
                    return;
                }

                if (CompressedChunks == null)
                {
                    CompressedChunks = new List<DataChunk>();
                    Size.Compressed = 0;
                    Size.Uncompressed = 0;
                }

                Size.Compressed += chunk.Size.Compressed;
                Size.Uncompressed += chunk.Size.Uncompressed;

                CompressedChunks.Add(chunk);
            }

            public S3DArchiveEntry AsFormat(string newFormat, bool changeExtension)
            {
                Debug.LogError("S3DArchiveEntry.AsFormat() not yet implemented.");
                return null;
            }

            public int GetAlphaBits()
            {
                //
                return 0;
            }

            public byte[] GetContents()
            {
                if (!_isUnCompressed)
                {
                    if (CompressedChunks == null)
                    {
                        return null;
                    }

                    if (Size.Uncompressed == 0)
                    {
                        if (CompressedChunks.Count >= 1)
                        {
                            Status = Result.DataError;

                            return null;
                        }

                        _isUnCompressed = true;

                        return null; //Empty data buffer. Zero Length.
                    }

                    try
                    {
                        try
                        {
                            _uncompressedData = new byte[Size.Uncompressed];
                        }
                        catch
                        {
                            _uncompressedData = null;
                        }

                        if (_uncompressedData == null)
                        {
                            Status = Result.OutOfMemory;

                            return null;
                        }

                        UInt32 bytesSoFar = 0;

                        for (int i = 0; i < CompressedChunks.Count; i++)
                        {
                            DataChunk chunk = CompressedChunks[i];

                            if (chunk.DecompressTo(_uncompressedData, bytesSoFar) != chunk.Size.Uncompressed)
                            {
                                _uncompressedData = null;

                                Status = Result.DecompressionError;

                                return null;
                            }

                            bytesSoFar += chunk.Size.Uncompressed;
                        }

                        if (bytesSoFar != Size.Uncompressed)
                        {
                            _uncompressedData = null;
                            Status = Result.DecompressionError;

                            return null;
                        }

                        _isUnCompressed = true;

                        Status = Result.OK;
                    }
                    catch (System.Exception exception)
                    {
                        _uncompressedData = null;
                        Status = Result.DecompressionError;

                        Debug.LogErrorFormat("S3DArchiveEntry.GetContents Exception: {0}", exception.ToString());
                    }
                }

                return _uncompressedData;
            }

            public Texture2D GetImage()
            {
                if (_isImageChecked)
                {
                    return _image;
                }

                if (!IsImageFile(FileName))
                {
                    _isImageChecked = true;
                    _image = null;

                    return null;
                }

                byte[] bytes = GetContents();

                _image = new Texture2D(4, 4);

                _image.LoadRawTextureData(bytes);
                _image.Apply();

                _isImageChecked = true;
                return _image;
            }

            public Texture GetThumbnail()
            {
                if (_imageThumbnail == null)
                {
                    return GetImage();
                }

                return _imageThumbnail;
            }

            public void SetImage(Texture2D newImage, string format)
            {
                Debug.LogError("S3DArchiveEntry.SetImage() not yet implemented"); 
                return;
            }

            public Result SetContents(byte[] data)
            {
                _uncompressedData = data;
                Size.Compressed = 0;
                Size.Uncompressed = 0;
                UInt32 sizeUnCompressed = (data == null) ? 0 : (UInt32)data.Length;
                _isUnCompressed = true;
                CompressedChunks = new List<DataChunk>();
                _image = null;

                if (data == null)
                {
                    _isImageChecked = true;

                    return Result.OK;
                }

                _isImageChecked = false;

                try
                {
                    UInt32 chunkSize = (sizeUnCompressed < MAX_BLOCK_SIZE) ? sizeUnCompressed : MAX_BLOCK_SIZE;
                    byte[] chunkBytes = new byte[chunkSize];

                    while (Size.Uncompressed < data.Length)
                    {
                        if ((data.Length - Size.Uncompressed) < chunkSize)
                        {
                            chunkSize = ((UInt32)data.Length - Size.Uncompressed);
                        }

                        DataChunk chunk = new DataChunk();
                        chunk.CompressFrom(data, Size.Uncompressed, chunkSize);

                        AddChunk(chunk);
                    }

                    Status = Result.OK;
                }
                catch
                {
                    Size.Compressed = 0;
                    Size.Uncompressed = 0;
                    CompressedChunks = new List<DataChunk>();
                    _isUnCompressed = true;
                    _uncompressedData = null;

                    Status = Result.DataError;
                }

                return Status;
            }

            public void SetThumbNail(Texture2D thumbnail)
            {
                _imageThumbnail = thumbnail;
            }
        }

        public class Header
        {
            public UInt32 IndexPointer;
            public UInt32 MagicNumber;
            public UInt32 VersionNumber;
            public UInt32 EntryCount;
            public UInt32 DateStamp;
        }

        //////////////////////////////////////////////////////////////////////
        
        protected string _fileName = "(Untitled)";
        protected static UInt32 _magicNumber = 0x20534650;
        protected static UInt32[] _fileNameCRCTable = new uint[256];
        protected static string _footerToken = "STEVE";
        
        protected static UInt32 GetFileNameCRC(string fileName)
        {
            if (fileName == null)
            {
                return 0;
            }

            UInt32 crc = 0;
            UInt32 index;
            char chr;

            for (int pointer = 0; pointer < fileName.Length; pointer++)
            {
                if (pointer == fileName.Length)
                {
                    chr = '\0';
                }
                else
                {
                    chr = fileName[pointer];
                }

                index = ((crc >> 24) ^ chr) & 0xFF;

                crc = ((crc << 8) ^ _fileNameCRCTable[index]);
            }

            return crc;
        }

        protected static UInt32 GetFileNameCRC(char[] fileName)
        {
            return GetFileNameCRC(fileName, (fileName == null) ? 0 : fileName.Length);
        }

        protected static UInt32 GetFileNameCRC(char[] fileName, int length)
        {
            UInt32 crc = 0;
            UInt32 index;

            if (fileName != null)
            {
                for (int pointer = 0; pointer < length; pointer++)
                {
                    index = ((crc >> 24) ^ fileName[pointer]) & 0xFF;
                    crc = ((crc << 8) ^ _fileNameCRCTable[index]);
                }
            }

            return crc;
        }

        public string FilePath = "";
        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _fileName = "(Untitled)";
                }
                else
                {
                    string path = System.IO.Path.GetDirectoryName(value);

                    if (!string.IsNullOrEmpty(path))
                    {
                        FilePath = path;
                    }

                    _fileName = System.IO.Path.GetFileName(value);
                }
            }
        }

        public SortedList<string, S3DArchiveEntry> Files = new SortedList<string, S3DArchiveEntry>();
        public bool IsDirty = false;
        public UInt32 SizeOnDisk = 0;
        public Result Status = Result.NotImplemented;

        //////////////////////////////////////////////////////////////////////

        public S3DArchiveEntry FindFile(string fileName)
        {
            try
            {
                return Files[System.IO.Path.GetFileName(fileName).ToLower()];
            }
            catch
            {
                return null;
            }
        }

        public S3DArchiveEntry FindFileOrSimilarImage(string fileName)
        {
            if (!IsImageFile(fileName))
            {
                return this.FindFile(fileName);
            }

            fileName = System.IO.Path.GetFileName(fileName);
            string _prefix = System.IO.Path.GetFileNameWithoutExtension(fileName);

            foreach (S3DArchiveEntry _file in this.Files.Values)
            {
                if (IsImageFile(_file.FileName))
                {
                    if (_prefix.Equals(System.IO.Path.GetFileNameWithoutExtension(_file.FileName), StringComparison.CurrentCultureIgnoreCase))
                    {
                        return _file;
                    }
                }
            }

            return null;
        }
    }
}