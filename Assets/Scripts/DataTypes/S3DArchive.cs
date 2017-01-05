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
        public static string[] archiveFileExtEditor = new string[] {"EQ Archives", "s3d,eqg,pfs,pak", "All files", "*"};

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

                //http://answers.unity3d.com/questions/709405/reading-pixel-colors-of-an-image.html
                System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes);

                System.Drawing.Bitmap bmpImage = (System.Drawing.Bitmap)System.Drawing.Image.FromStream(ms);
                
                _image = BitmapToTexture(bmpImage);

                _isImageChecked = true;
                return _image;
            }

            public static float NormalizeBmp(float current, float min, float max)
            {
                return (current - min) / (max - min);
            }

            public Texture2D BitmapToTexture(System.Drawing.Bitmap img)
            {
                Texture2D newTex = new Texture2D(img.Width, img.Height);
                for (int x = 0; x < newTex.width; x++)
                {
                    for (int y = 0; y < newTex.height; y++)
                    {
                        float r = NormalizeBmp(img.GetPixel(x, y).R, 0f, 255f);
                        float g = NormalizeBmp(img.GetPixel(x, y).G, 0f, 255f);
                        float b = NormalizeBmp(img.GetPixel(x, y).B, 0f, 255f);
                        float a = NormalizeBmp(img.GetPixel(x, y).A, 0f, 255f);
                        Color nColor = new Color(r, g, b, a);
                        newTex.SetPixel(x, y, nColor);
                    }
                }
                newTex.Apply();
                return newTex;
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

        public SortedList<string, S3DArchiveEntry> ArtFiles = new SortedList<string, S3DArchiveEntry>();
        public SortedList<string, S3DArchiveEntry> WLDFiles = new SortedList<string, S3DArchiveEntry>();
        public bool IsDirty = false;
        public UInt32 SizeOnDisk = 0;
        public Result Status = Result.NotImplemented;

        //////////////////////////////////////////////////////////////////////

        public S3DArchiveEntry FindFile(string fileName)
        {
            try
            {
                return ArtFiles[System.IO.Path.GetFileName(fileName).ToLower()];
            }
            catch
            {
                return null;
            }
        }

        public static S3DArchive Load(string filePath)
        {
            return Load(filePath, GetFileContents(filePath));
        }

        public static S3DArchive Load(string fileName, byte[] contents)
        {
            if (string.IsNullOrEmpty(fileName) || contents == null)
            {
                //We got a bad file name, or the file is zero length, and thus not a PFS archive;

                return null;
            }

            Header header = new Header();

            using (System.IO.BinaryReader input = new System.IO.BinaryReader(new System.IO.MemoryStream(contents)))
            {
                try
                {
                    //
                    // 1. Read file header
                    //
                    header.IndexPointer = input.ReadUInt32();
                    header.MagicNumber = input.ReadUInt32();
                    header.VersionNumber = input.ReadUInt32();
                }
                catch (System.Exception exception)
                {
                    //Not a PFS archive
                    Debug.LogErrorFormat("S3DArchive Exception: {0}", exception.ToString());
                    return null;
                }

                S3DArchive archive = new S3DArchive();
                archive.FileName = fileName;
                archive.SizeOnDisk = (UInt32)contents.Length;

                try
                {
                    //
                    // 2. Read index of file pointers and sizes in archive
                    //

                    input.BaseStream.Seek(header.IndexPointer, System.IO.SeekOrigin.Begin);

                    header.EntryCount = input.ReadUInt32();

                    if (header.EntryCount == 0)
                    {
                        //Empty archive...?
                        archive.ArtFiles = new SortedList<string, S3DArchiveEntry>();
                        archive.WLDFiles = new SortedList<string, S3DArchiveEntry>();
                    }
                    else
                    {
                        archive.ArtFiles = new SortedList<string, S3DArchiveEntry>((int)header.EntryCount);
                        archive.WLDFiles = new SortedList<string, S3DArchiveEntry>();
                    }

                    //Filename directory is the "file" at the end of the archive (with the highest filepointer)
                    S3DArchiveEntry directory = null;

                    //For the verification later, which is optional, but will catch a malformed/corrupted archive.
                    Dictionary<UInt32, UInt32> fileNameCRCs = new Dictionary<uint, uint>();

                    //Files in a PFS archive tend to be stored by ascending order of filenamecrc.
                    //However the filename directory is sorted by filepointer
                    SortedList<UInt32, S3DArchiveEntry> files = new SortedList<uint, S3DArchiveEntry>();

                    for (UInt32 index = 0; index < header.EntryCount; index++)
                    {
                        S3DArchiveEntry file = new S3DArchiveEntry();
                        UInt32 fileNameCRC = input.ReadUInt32();
                        file.FilePointer = input.ReadUInt32();
                        file.Size.Uncompressed = input.ReadUInt32();
                        fileNameCRCs.Add(file.FilePointer, fileNameCRC);

                        if ((directory == null) || (file.FilePointer > directory.FilePointer))
                        {
                            directory = file;
                        }

                        files.Add(file.FilePointer, file);
                    }

                    if ((input.BaseStream.Length - input.BaseStream.Position) >= 9)
                    {
                        //PFS footer
                        char[] token = input.ReadChars(5);

                        if (new string(token).Equals("STEVE"))
                        {
                            //Valid footer token
                            header.DateStamp = input.ReadUInt32();
                        }
                    }

                    //
                    //  3. Read the compressed file entries (each split into compressed chunks)
                    //

                    foreach (S3DArchiveEntry file in files.Values)
                    {
                        //Seek to entry position in stream
                        input.BaseStream.Seek(file.FilePointer, System.IO.SeekOrigin.Begin);

                        UInt32 totalUncompressedBytes = file.Size.Uncompressed;
                        file.Size.Uncompressed = 0;

                        while ((file.Size.Uncompressed < totalUncompressedBytes) && (input.BaseStream.Position < input.BaseStream.Length))
                        {
                            UInt32 blockSizeCmp = input.ReadUInt32();
                            UInt32 blockSizeUnc = input.ReadUInt32();

                            //Sanity check 1: uncompressed data larger than what we were told?
                            if ((blockSizeUnc + file.Size.Uncompressed) > totalUncompressedBytes)
                            {
                                Debug.LogError("Data is larger than expected");
                            }

                            //Santity check 2: Compressed data goes past the end of the file?
                            if ((input.BaseStream.Position + blockSizeCmp) > input.BaseStream.Length)
                            {
                                Debug.LogError("Data goes past expected length");
                            }

                            file.AddChunk(new S3DArchiveEntry.DataChunk(contents, (UInt32)input.BaseStream.Position, blockSizeCmp, blockSizeUnc, true));

                            input.BaseStream.Position += blockSizeCmp;
                        }
                    }

                    //
                    //  4. Unpack and parse the directory of filesnames from the "file" at the end of the archive (highest filepointer);
                    //

                    //Remove directory from file entries in archive. We'll have to rebuild it when saving the archive anyway.
                    files.Remove(directory.FilePointer);
                    header.EntryCount--;

                    //Load filenames from directory
                    System.IO.BinaryReader dirStream = new System.IO.BinaryReader(new System.IO.MemoryStream(directory.GetContents()));

                    UInt32 fileNameCount = dirStream.ReadUInt32();

                    if (fileNameCount > header.EntryCount)
                    {
                        //If we somehow have more filenames than entries in the archive, ignore the glitched extras
                        fileNameCount = header.EntryCount;
                    }

                    archive.ArtFiles = new SortedList<string, S3DArchiveEntry>();
                    archive.WLDFiles = new SortedList<string, S3DArchiveEntry>();

                    foreach (S3DArchiveEntry file in files.Values)
                    {
                        Int32 len = dirStream.ReadInt32();
                        char[] inputName = dirStream.ReadChars(len);
                        UInt32 crc = GetFileNameCRC(inputName);

                        if (crc != fileNameCRCs[file.FilePointer])
                        {
                            //Filename doesn't match with what we were given in step 2
                            //This happens in gqeuip.s3d. We are ignoring it.
                            //Debug.LogError("Filename doesn't match. Unable to proceeed.");
                        }

                        file.FileName = new string(inputName, 0, len - 1);

                        string fileExt = System.IO.Path.GetExtension(file.FileName).ToLower();

                        if (fileExt == "wld" || fileExt == ".wld")
                        {
                            archive.WLDFiles.Add(file.FileName.ToLower(), file);
                        }
                        else
                        {
                            archive.ArtFiles.Add(file.FileName.ToLower(), file);
                        }
                    }

                    //All entries loaded and filenames read from directory.

                    archive.Status = Result.OK;
                }
                catch (System.Exception exception)
                {
                    Debug.LogErrorFormat("S3DArchive Load Exception: {0}", exception.ToString());
                    archive.Status = Result.MalformedFile;
                }

                return archive;
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

            foreach (S3DArchiveEntry _file in this.ArtFiles.Values)
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

        public Result Add(string fileName, bool replaceSimilarImage)
        {
            return Add(new S3DArchiveEntry(fileName, GetFileContents(fileName)), replaceSimilarImage);
        }

        public Result Add(string fileName, bool replaceSimilarImage, byte[] fileContents)
        {
            return Add(new S3DArchiveEntry(fileName, fileContents), replaceSimilarImage);
        }

        public Result Add(S3DArchiveEntry file)
        {
            return Add(file, false);
        }

        public Result Add(S3DArchiveEntry file, bool replaceSimilarImage)
        {
            if (file == null)
            {
                return Result.InvalidArgument;
            }

            S3DArchiveEntry archiveFile;

            if (replaceSimilarImage)
            {
                archiveFile = FindFileOrSimilarImage(FileName);
            }
            else
            {
                archiveFile = FindFile(FileName);
            }

            if (archiveFile != null)
            {
                //We already have a file that has a similar name. Import the new file AS that file, replacing it

                ArtFiles.RemoveAt(ArtFiles.IndexOfValue(archiveFile));

                file.FileName = archiveFile.FileName;
            }

            ArtFiles[file.FileName.ToLower()] = file;

            IsDirty = true;

            return file.Status;
        }

        public Result Remove(S3DArchiveEntry file)
        {
            if (file == null)
            {
                return Result.FileNotFound;
            }

            ArtFiles.RemoveAt(ArtFiles.IndexOfValue(file));

            IsDirty = true;

            return Result.OK;
        }

        public Result Remove(string fileName)
        {
            return Remove(FindFile(fileName));
        }

        public Result Save(string fileName)
        {
            FileName = fileName;
            return Save();
        }

        public Result Save()
        {
            if ((FilePath == "") || (FileName == "(Untitled)") || (FileName == ""))
            {
                return Result.InvalidArgument;
            }

            Result result = Result.OK;

            try
            {
                using (System.IO.BinaryWriter file = new System.IO.BinaryWriter(System.IO.File.Create(FilePath + @"\" + FileName)))
                {
                    //
                    //  Step 1 - Get an order of files by filenam CRC, per standard PFS archive practice.
                    //

                    SortedList<UInt32, S3DArchiveEntry> filesByCRC = new SortedList<uint, S3DArchiveEntry>();

                    foreach (S3DArchiveEntry entry in ArtFiles.Values)
                    {
                        filesByCRC.Add(GetFileNameCRC(entry.FileName), entry);
                    }

                    //
                    //  Step 2 - Build the directory of filenames and compress it for adding at the end of the archive
                    //

                    S3DArchiveEntry directory = new S3DArchiveEntry();

                    byte[] directoryBytes = new byte[64 * 1024]; //global_chr.s3d = ~29,000 bytes of filenames!

                    using (System.IO.BinaryWriter stream = new System.IO.BinaryWriter(new System.IO.MemoryStream(directoryBytes)))
                    {
                        UInt32 directorySize = 0;

                        stream.Write((UInt32)ArtFiles.Count);

                        foreach (S3DArchiveEntry entry in filesByCRC.Values)
                        {
                            stream.Write((UInt32)entry.FileName.Length + 1);
                            foreach (char c in entry.FileName)
                            {
                                stream.Write(c);
                            }
                            stream.Write('\0');
                        }

                        directorySize = (UInt32)stream.BaseStream.Position;

                        Array.Resize<byte>(ref directoryBytes, (int)directorySize);

                        directory.SetContents(directoryBytes);
                    }

                    //
                    //  Step 3 - Build the file header
                    //

                    Header header = new Header();
                    header.MagicNumber = _magicNumber;
                    header.VersionNumber = 0x00020000;

                    //a. Index pointer must be determined. Start with the size after the header itself
                    header.IndexPointer = 4 + 4 + 4;

                    //b. Add in the size of all of the compressed chunks and their two size values
                    foreach (S3DArchiveEntry entry in ArtFiles.Values)
                    {
                        header.IndexPointer += (4 + 4) * (entry.CompressedChunks == null ? 1 : (UInt32)entry.CompressedChunks.Count);
                        header.IndexPointer += entry.Size.Compressed;
                    }

                    //c. Add in the size of the compressed filename directory and its size values
                    header.IndexPointer += (4 + 4) * (UInt32)directory.CompressedChunks.Count + directory.Size.Compressed;

                    //
                    //  Step 4 - Write the file header
                    //

                    file.Write(header.IndexPointer);
                    file.Write(header.MagicNumber);
                    file.Write(header.VersionNumber);

                    //
                    //  Step 5 - Compressed file chunks
                    //

                    foreach (S3DArchiveEntry entry in filesByCRC.Values)
                    {
                        entry.FilePointer = (UInt32)file.BaseStream.Position;

                        foreach (S3DArchiveEntry.DataChunk chunk in entry.CompressedChunks)
                        {
                            file.Write(chunk.Size.Compressed);
                            file.Write(chunk.Size.Uncompressed);
                            file.Write(chunk.CompressedData, 0, (int)chunk.Size.Compressed);
                        }
                    }

                    //
                    //  Step 6 - Filename directory compressed chunks at the end

                    directory.FilePointer = (UInt32)file.BaseStream.Position;

                    foreach (S3DArchiveEntry.DataChunk chunk in directory.CompressedChunks)
                    {
                        file.Write(chunk.Size.Compressed);
                        file.Write(chunk.Size.Uncompressed);
                        file.Write(chunk.CompressedData, 0, (int)chunk.Size.Compressed);
                    }

                    //
                    //  Step 7 - Index of file entries
                    //

                    file.Write((UInt32)(ArtFiles.Count + 1));

                    foreach (KeyValuePair<UInt32, S3DArchiveEntry> kvp in filesByCRC)
                    {
                        file.Write(kvp.Key);
                        file.Write(kvp.Value.FilePointer);
                        file.Write(kvp.Value.Size.Uncompressed);
                    }

                    //
                    //  Step 8 - Index of file entries
                    //

                    file.Write((UInt32)(ArtFiles.Count + 1));

                    foreach (KeyValuePair<UInt32, S3DArchiveEntry> kvp in filesByCRC)
                    {
                        file.Write(kvp.Key);
                        file.Write(kvp.Value.FilePointer);
                        file.Write(kvp.Value.Size.Uncompressed);
                    }

                    //
                    //  Step 8 - Add filename directory to end of index
                    //

                    file.Write(0xFFFFFFFFU);
                    file.Write(directory.FilePointer);
                    file.Write(directory.Size.Uncompressed);

                    //
                    //  Step 9 - PFS Footer
                    //

                    foreach (char letter in _footerToken)
                    {
                        file.Write(letter);
                    }

                    file.Write(header.DateStamp);

                    file.Close();
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogErrorFormat("S3DArchive Exception: {0}.", exception.ToString());
                return Result.FileWriteError;
            }

            if (result == Result.OK)
            {
                IsDirty = false;
            }

            return result;
        }
    }
}