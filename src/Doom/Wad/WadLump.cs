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
using System.Text.RegularExpressions;

namespace PNG2WAD.Doom.Wad
{
    /// <summary>
    /// A lump in a Doom wad file.
    /// </summary>
    public struct WadLump
    {
        /// <summary>
        /// Regex pattern used to remove invalid characters from the lump name.
        /// </summary>
        private const string LUMP_REGEX_PATTERN = "[^A-Z0-9_]";

        /// <summary>
        /// Lump name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Lump content, as an array of bytes.
        /// </summary>
        public byte[] Bytes { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the lump.</param>
        /// <param name="bytes">The content of the lump, as an array of bytes.</param>
        public WadLump(string name, byte[] bytes)
        {
            Bytes = bytes ?? Array.Empty<byte>(); // Make sure bytes is not null
            if (string.IsNullOrEmpty(name)) name = "NULL";
            Name = Regex.Replace(name.ToUpperInvariant(), LUMP_REGEX_PATTERN, "");
            if (Name.Length > WadFile.MAX_LUMP_NAME_LENGTH)
                Name = Name[..WadFile.MAX_LUMP_NAME_LENGTH];
        }
    }
}
