/*
==========================================================================
This file is part of Pixels of Doom, a tool to create Doom maps from PNG files
by @akaAgar (https://github.com/akaAgar/pixels-of-doom)
Pixels of Doom is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
Pixels of Doom is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with Pixels of Doom. If not, see https://www.gnu.org/licenses/
==========================================================================
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PixelsOfDoom.Wad
{
    public sealed class WadFile : IDisposable
    {
        /// <summary>
        /// Max length of the lump name entry.
        /// </summary>
        public const int MAX_LUMP_NAME_LENGTH = 8;

        /// <summary>
        /// A list of all the lumps in the wad file.
        /// </summary>
        private readonly List<WadLump> Lumps = new List<WadLump>();

        /// <summary>
        /// Is the wad an IWAD (true) or a PWAD (false)?
        /// </summary>
        public bool IWAD { get; set; } = false;

        /// <summary>
        /// The number of lumps in the file.
        /// </summary>
        public int LumpCount { get { return Lumps.Count; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        public WadFile() { }

        /// <summary>
        /// IDisposeable implementation.
        /// </summary>
        public void Dispose() { Clear(); }

        /// <summary>
        /// Remove all lumps.
        /// </summary>
        public void Clear() { Lumps.Clear(); }

        /// <summary>
        /// Saves the content of the .wad to a file.
        /// </summary>
        public void SaveToFile(string wadFilePath)
        {
            int directoryOffset = 12;
            foreach (WadLump l in Lumps) directoryOffset += l.Bytes.Length;

            // Write the 12-bytes wad file header.
            // 4 bytes: an ASCII string which must be either "IWAD" or "PWAD"
            // 4 bytes: an integer which is the number of lumps in the wad
            // 4 bytes: an integer which is the file offset to the start of the directory
            List<byte> headerBytes = new List<byte>();
            headerBytes.AddRange(Encoding.ASCII.GetBytes(IWAD ? "IWAD" : "PWAD"));
            headerBytes.AddRange(BitConverter.GetBytes(Lumps.Count));
            headerBytes.AddRange(BitConverter.GetBytes(directoryOffset));

            // Writes the file directory
            List<byte> directoryBytes = new List<byte>();
            int byteOffset = 12;
            foreach (WadLump l in Lumps)
            {
                directoryBytes.AddRange(BitConverter.GetBytes(byteOffset));
                directoryBytes.AddRange(BitConverter.GetBytes(l.Bytes.Length));
                directoryBytes.AddRange(GetStringBytes(l.LumpName));
                byteOffset += l.Bytes.Length;
            }

            List<byte> wadBytes = new List<byte>();
            wadBytes.AddRange(headerBytes);
            foreach (WadLump l in Lumps) wadBytes.AddRange(l.Bytes);
            wadBytes.AddRange(directoryBytes);
            File.WriteAllBytes(wadFilePath, wadBytes.ToArray());

            Console.WriteLine($"Saved wad to {Path.GetFileName(wadFilePath)}, {LumpCount} lumps, {wadBytes.Count} bytes.");
        }

        /// <summary>
        /// Convert an ASCII string to an array of bytes.
        /// </summary>
        /// <param name="text">String to convert</param>
        /// <param name="length">Length of the byte array</param>
        /// <returns>A byte array</returns>
        public static byte[] GetStringBytes(string text, int length = MAX_LUMP_NAME_LENGTH)
        {
            if (text == null) return new byte[length];

            byte[] bytes = Encoding.ASCII.GetBytes(text);
            Array.Resize(ref bytes, length);
            return bytes;
        }

        /// <summary>
        /// Adds a lump to the wad.
        /// </summary>
        /// <param name="lumpName">The name of the lump.</param>
        /// <param name="bytes">The content of the lump, as a byte array.</param>
        public void AddLump(string lumpName, byte[] bytes)
        {
            Lumps.Add(new WadLump(lumpName, bytes));
        }
    }
}
