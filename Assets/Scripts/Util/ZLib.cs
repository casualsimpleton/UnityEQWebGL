//An attempt to implement a S3D parser inside Unity. I suck at decyphering data files, so this is heavily based on EQ-Zip's work
//CasualSimpleton
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.IO.Compression;
using EQBrowser;

namespace EQBrowser
{
    public class zlib
    {
        public static int Decompress(Stream Source, byte[] Destination, int Offset)
        {
            return Decompress(Source, Destination, Offset, (Destination == null) ? 0 : (Destination.Length - Offset));
        }
        public static int Decompress(Stream Source, byte[] Destination, int Offset, int BlockSize)
        {
            if (Source.CanSeek)
            {
                Source.Seek(2, System.IO.SeekOrigin.Current);
            }
            else
            {
                Source.ReadByte();
                Source.ReadByte();
            }

            int _bytesRead = 0;

            using (DeflateStream _inflater = new DeflateStream(Source, CompressionMode.Decompress, true))
            {
                _bytesRead = _inflater.Read(Destination, Offset, BlockSize);
                _inflater.Close();
            }

            return _bytesRead;
        }

        public static byte[] Compress(Stream Source) { return Compress(Source, 8192); }
        public static byte[] Compress(Stream Source, int BlockSize)
        {
            byte[] _chunk;

            using (MemoryStream _compressed = new MemoryStream())
            {
                _compressed.WriteByte(0x58);
                _compressed.WriteByte(0x85);

                byte[] _data = new byte[BlockSize];

                int _dataSize = Source.Read(_data, 0, BlockSize);

                using (DeflateStream _deflater = new DeflateStream(_compressed, CompressionMode.Compress, true))
                {
                    _deflater.Write(_data, 0, _dataSize);
                    _deflater.Close();
                }

                uint _adler32 = Adler32(_data, 0, _dataSize);

                _compressed.WriteByte((byte)(_adler32 >> 24));
                _compressed.WriteByte((byte)(_adler32 >> 16));
                _compressed.WriteByte((byte)(_adler32 >> 8));
                _compressed.WriteByte((byte)_adler32);

                _compressed.Flush();

                _chunk = _compressed.ToArray();
            }

            return _chunk;
        }

        public static uint Adler32(byte[] Data, int Offset, int Length)
        {
            uint _adler32a = 1;
            uint _adler32b = 0;

            if (Data != null)
            {
                while (Length > 0)
                {
                    int _rounds = (Length < 3800) ? Length : 3800;
                    Length -= _rounds;

                    while (_rounds-- > 0)
                    {
                        _adler32a += Data[Offset++];
                        _adler32b += _adler32a;
                    }

                    _adler32a %= 65521;
                    _adler32b %= 65521;
                }
            }

            return (_adler32b << 16) | _adler32a;
        }
    }
}