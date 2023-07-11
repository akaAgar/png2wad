/*
==========================================================================
This file is part of Tools of Doom, a library providing a collection of
classes to load/edit/save Doom maps and wad archives, created by @akaAgar
(https://github.com/akaAgar/tools-of-doom).

Tools of Doom is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Tools of Doom is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Tools of Doom. If not, see https://www.gnu.org/licenses/
==========================================================================
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PNG2WAD.Doom.Wad
{
    /// <summary>
    /// A Doom wad archive.
    /// </summary>
    public sealed class WadFile : IDisposable
    {
        /// <summary>
        /// Max length of a lump name.
        /// </summary>
        public const int MAX_LUMP_NAME_LENGTH = 8;

        /// <summary>
        /// Is the wad an IWAD (true) or a PWAD (false)?
        /// </summary>
        public bool IWAD { get; set; } = false;

        /// <summary>
        /// The number of lumps in the archive.
        /// </summary>
        public int LumpCount { get { return Lumps.Count; } }

        /// <summary>
        /// A list of all the lumps in the wad archive.
        /// </summary>
        // Cannot be a string/byte[] dictionary because a wad archive can have multiple lumps with the same names (and often does, as each map has its own THINGS, LINEDEFS, etc.)
        private readonly List<WadLump> Lumps = new();

        /// <summary>
        /// Constructor. Creates an empty wad archive.
        /// </summary>
        public WadFile() { }

        /// <summary>
        /// Constructor. Loads a wad archive from a file.
        /// </summary>
        /// <param name="filePath">Path to the wad file</param>
        public WadFile(string filePath)
        {
            if (!File.Exists(filePath)) return;

            byte[] buffer4 = new byte[4];
            byte[] buffer8 = new byte[8];

            int i;

            using FileStream fs = new(filePath, FileMode.Open);

            fs.Read(buffer4, 0, 4); // Bytes 0-3 (ASCII string): IWAD or PWAD
            switch (GetStringFromBytes(buffer4))
            {
                case "IWAD": IWAD = true; break;
                case "PWAD": IWAD = false; break;
                default: return; // Neither an IWAD or a PWAD, invalid format
            }

            fs.Read(buffer4, 0, 4); // Bytes 4-7 (int): lump count
            int lumpCount = Convert.ToInt32(buffer4);
            if (lumpCount <= 0) return;

            fs.Read(buffer4, 0, 4); // Bytes 8-11 (int): directory offset
            int directoryOffset = Convert.ToInt32(buffer4);
            if (directoryOffset < 12) return;

            for (i = 0; i < lumpCount; i++)
            {
                fs.Seek(directoryOffset + 16 * i, SeekOrigin.Begin);

                fs.Read(buffer4, 0, 4);
                int lumpOffset = Convert.ToInt32(buffer4);
                fs.Read(buffer4, 0, 4);
                int lumpSize = Convert.ToInt32(buffer4);
                fs.Read(buffer4, 0, 8);
                string lumpName = GetStringFromBytes(buffer8);

                byte[] lumpbytes = new byte[lumpSize];
                fs.Seek(lumpOffset, SeekOrigin.Begin);
                fs.Read(lumpbytes, 0, lumpSize);
                Lumps.Add(new WadLump(lumpName, lumpbytes));
            }
        }

        /// <summary>
        /// Remove all lumps.
        /// </summary>
        public void ClearLumps() { Lumps.Clear(); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lumpName"></param>
        /// <returns></returns>
        public WadLump? this[string lumpName]
        {
            get
            {
                int index = GetFirstIndexByLumpName(lumpName);
                if (index == -1) return null;
                return Lumps[index];
            }
        }

        /// <summary>
        /// Gets the lump with the provided index, or returns null if index is invalid.
        /// </summary>
        /// <param name="index">Index of the lump</param>
        /// <returns>A wad lump, or null if index was invalid</returns>
        public WadLump? this[int index]
        {
            get
            {
                if ((index < 0) || (index >= Lumps.Count)) return null;
                return Lumps[index];
            }
        }

        /// <summary>
        /// Gets the indices of all lumps with this name.
        /// </summary>
        /// <param name="lumpName">Lump name to search for</param>
        /// <returns></returns>
        public int[] GetIndicesByLumpName(string lumpName)
        {
            if (string.IsNullOrEmpty(lumpName)) return Array.Empty<int>();
            lumpName = lumpName.ToUpperInvariant();
            
            List<int> indices = new();

            for (int i = 0; i < Lumps.Count; i++)
                if (Lumps[i].Name == lumpName)
                    indices.Add(i);

            return indices.ToArray();
        }

        public int GetFirstIndexByLumpName(string lumpName)
        {
            int[] indices = GetIndicesByLumpName(lumpName);
            return (indices.Length == 0) ? -1 : indices[0];
        }

        /// <summary>
        /// Removes node(s) with the provided name.
        /// </summary>
        /// <param name="lumpName">Name of the lump(s) to remove</param>
        /// <param name="removeAll">If false, only the first node with this name will be removed. If true, all nodes with this name will be removed</param>
        /// <returns>True if at least one lump was removed, false if no lump with this name was found</returns>
        public bool RemoveLump(string lumpName, bool removeAll = false)
        {
            if (removeAll)
            {
                int lumpIndex = GetFirstIndexByLumpName(lumpName);
                if (lumpIndex < 0) return false;

                while (lumpIndex >= 0)
                {
                    Lumps.RemoveAt(lumpIndex);
                    lumpIndex = GetFirstIndexByLumpName(lumpName);
                }

                return true;
            }

            return RemoveLump(GetFirstIndexByLumpName(lumpName));
        }

        /// <summary>
        /// Removes the lump at the provided index.
        /// </summary>
        /// <param name="index">Index of the lump to remove</param>
        /// <returns>True if the lump was removed succesfully, false if index was invalid</returns>
        public bool RemoveLump(int index)
        {
            if ((index < 0) || (index >= Lumps.Count)) return false;
            Lumps.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// The names of all lumps in the file.
        /// </summary>
        public string[] LumpNames { get { return (from WadLump lump in Lumps select lump.Name).ToArray(); } }

        /// <summary>
        /// Saves the content of the .wad to a file.
        /// </summary>
        public void SaveToFile(string wadFilePath)
        {
            // Directory offset is 12 (header length) plus the sum of the length of all lumps
            int directoryOffset = 12 + (from WadLump lump in Lumps select lump.Bytes.Length).Sum();

            // Write the 12-bytes wad file header.
            // 4 bytes: an ASCII string which must be either "IWAD" or "PWAD"
            // 4 bytes: an integer which is the number of lumps in the wad
            // 4 bytes: an integer which is the file offset to the start of the directory
            List<byte> headerBytes = new();
            headerBytes.AddRange(Encoding.ASCII.GetBytes(IWAD ? "IWAD" : "PWAD"));
            headerBytes.AddRange(BitConverter.GetBytes(Lumps.Count));
            headerBytes.AddRange(BitConverter.GetBytes(directoryOffset));

            // Writes the file directory
            List<byte> directoryBytes = new();
            int byteOffset = 12;
            foreach (WadLump l in Lumps)
            {
                directoryBytes.AddRange(BitConverter.GetBytes(byteOffset));
                directoryBytes.AddRange(BitConverter.GetBytes(l.Bytes.Length));
                directoryBytes.AddRange(GetBytesFromString(l.Name));
                byteOffset += l.Bytes.Length;
            }

            List<byte> wadBytes = new();
            wadBytes.AddRange(headerBytes);
            foreach (WadLump l in Lumps) wadBytes.AddRange(l.Bytes);
            wadBytes.AddRange(directoryBytes);
            File.WriteAllBytes(wadFilePath, wadBytes.ToArray());
        }

        /// <summary>
        /// Convert an ASCII string to an array of bytes.
        /// </summary>
        /// <param name="text">String to convert</param>
        /// <param name="length">Fixed length of the returned byte array</param>
        /// <returns>A byte array</returns>
        public static byte[] GetBytesFromString(string text, int length = MAX_LUMP_NAME_LENGTH)
        {
            if (length <= 0) return Array.Empty<byte>();
            if (string.IsNullOrEmpty(text)) return new byte[length];

            byte[] bytes = Encoding.ASCII.GetBytes(text);
            Array.Resize(ref bytes, length);
            return bytes;
        }

        /// <summary>
        /// Converts an array of bytes into an ASCII string.
        /// </summary>
        /// <param name="asciiBytes">Array of bytes to convert</param>
        /// <returns>A ASCII string</returns>
        public static string GetStringFromBytes(params byte[] asciiBytes)
        {
            if (asciiBytes == null) return "";

            return Encoding.ASCII.GetString(asciiBytes).Trim('\0').ToUpperInvariant();
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

        /// <summary>
        /// IDisposeable implementation.
        /// </summary>
        public void Dispose() { ClearLumps(); }
    }
}
