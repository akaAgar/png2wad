/*
==========================================================================
This file is part of PNG2WAD, a tool to create Doom maps from PNG files,
created by @akaAgar (https://github.com/akaAgar/png2wad)

PNG2WAD is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

PNG2WAD is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with PNG2WAD. If not, see https://www.gnu.org/licenses/
==========================================================================
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PNG2WAD.INI
{
    /// <summary>
    /// Loads and saves Windows .ini files.
    /// </summary>
    public class INIFile : IDisposable
    {
        private const StringSplitOptions SPLIT_OPTIONS_REMOVE_EMPTY_AND_TRIM = StringSplitOptions.RemoveEmptyEntries;

        private static readonly Regex KEY_NORMALIZATION_REGEX = new("[^a-z0-9-\\.]");

        private readonly Dictionary<string, Dictionary<string, string>> entries = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Constructor.
        /// </summary>
        public INIFile()
        {
            entries.Clear();
        }

        public INIFile(string filePath, Encoding encoding = null)
        {
            LoadFromFile(filePath, encoding);
        }

        public void Clear()
        {
            foreach (string entryKey in entries.Keys)
                entries[entryKey].Clear();

            entries.Clear();
        }

        public string[] GetAllSections()
        {
            return entries.Keys.OrderBy(x => x).ToArray();
        }

        public void LoadFromFile(string filePath, Encoding encoding = null)
        {
            Clear();
            if (!File.Exists(filePath)) return;
            string dataString = File.ReadAllText(filePath, encoding ?? Encoding.UTF8);

            ParseString(dataString);
        }

        public void LoadFromDataString(string dataString)
        {
            ParseString(dataString);
        }

        public string[] GetAllKeysInSection(string sectionKey, bool topLevelOnly = false)
        {
            if (!entries.ContainsKey(sectionKey)) return Array.Empty<string>();

            if (topLevelOnly)
                return (from string key in entries[sectionKey].Keys select key.Split('.')[0]).Distinct().OrderBy(x => x).ToArray();

            return entries[sectionKey].Keys.OrderBy(x => x).ToArray();
        }

        public bool SectionExists(string sectionKey)
        {
            return entries.ContainsKey(sectionKey);
        }

        public bool ValueExists(string sectionKey, string keyID)
        {
            return entries.ContainsKey(sectionKey) && entries[sectionKey].ContainsKey(keyID);
        }

        public void RemoveSection(string sectionKey)
        {
            if (!entries.ContainsKey(sectionKey)) return;

            entries.Remove(sectionKey);
        }

        public void RemoveKey(string sectionKey, string keyID)
        {
            if (!entries.ContainsKey(sectionKey) || !entries[sectionKey].ContainsKey(keyID)) return;

            entries[sectionKey].Remove(keyID);
        }

        protected string GetRawValue(string sectionKey, string keyID)
        {
            sectionKey = NormalizeKey(sectionKey);
            keyID = NormalizeKey(keyID);

            if (!ValueExists(sectionKey, keyID)) return null;

            return entries[sectionKey][keyID];
        }

        protected void SetRawValue(string sectionKey, string keyID, string value)
        {
            sectionKey = NormalizeKey(sectionKey);
            keyID = NormalizeKey(keyID);
            value ??= "";

            if (!entries.ContainsKey(sectionKey))
                entries.Add(sectionKey, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

            if (!entries[sectionKey].ContainsKey(keyID))
                entries[sectionKey].Add(keyID, value);
            else
                entries[sectionKey][keyID] = value;
        }

        protected string[] GetAllValuesKeys()
        {
            return (
                from Dictionary<string, string> section in entries.Values
                select section.Keys).SelectMany(i => i).Distinct().OrderBy(x => x).ToArray();
        }

        private void ParseString(string dataString)
        {
            Clear();

            string[] lines = dataString.Replace("\r\n", "\n").Split(new char[] { '\n' }, SPLIT_OPTIONS_REMOVE_EMPTY_AND_TRIM);
            string currentSection = null;

            foreach (string line in lines)
            {
                if (line.StartsWith(";")) continue; // Line is a comment
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentSection = line.TrimStart('[').TrimEnd(']').Trim().ToLowerInvariant();
                    if (string.IsNullOrEmpty(currentSection)) currentSection = null;
                    continue;
                }
                if (!line.Contains('=')) continue;
                if (currentSection == null) continue;

                string[] parts = line.Split(new char[] { '=' }, 2, SPLIT_OPTIONS_REMOVE_EMPTY_AND_TRIM);
                if (parts.Length < 2) continue;

                string key = parts[0].ToLowerInvariant();
                if (string.IsNullOrEmpty(key)) continue;

                SetRawValue(currentSection, key, parts[1]);
            }
        }

        public T GetValue<T>(string sectionKey, string keyID, T defaultValue = default)
        {
            if (!ValueExists(sectionKey, keyID)) return defaultValue;
            return ConvertFromString<T>(GetRawValue(sectionKey, keyID));
        }

        public T[] GetValueArray<T>(string sectionKey, string keyID, params T[] defaultValues)
        {
            if (!ValueExists(sectionKey, keyID)) return defaultValues;

            Type valueType = typeof(T);

            string[] values = GetRawValue(sectionKey, keyID).Split(GetArraySeparator<T>(), SPLIT_OPTIONS_REMOVE_EMPTY_AND_TRIM);

            return (from string value in values select ConvertFromString<T>(value)).ToArray();
        }

        public Dictionary<string, T[]> GetValueStringArrayDictionary<T>(string sectionKey, string keyID)
        {
            Dictionary<string, T[]> dict = new(StringComparer.OrdinalIgnoreCase)
            {
                { "", GetValueArray<T>(sectionKey, keyID) }
            };

            string[] subKeys =
                (from subKey in GetAllKeysInSection(sectionKey)
                 where subKey.ToLowerInvariant().StartsWith($"{keyID.ToLowerInvariant()}.") &&
                 subKey.Length > keyID.Length + 1
                 select subKey.Split('.')[1].ToLowerInvariant()).ToArray();

            foreach (string subKey in subKeys)
            {
                if (dict.ContainsKey(subKey)) continue;
                dict.Add(subKey, GetValueArray<T>(sectionKey, $"{keyID}.{subKey}"));
            }

            return dict;
        }

        public Dictionary<string, T> GetValueStringDictionary<T>(string sectionKey, string keyID)
        {
            Dictionary<string, T> dict = new(StringComparer.OrdinalIgnoreCase)
            {
                { "", GetValue<T>(sectionKey, keyID) }
            };

            string[] subKeys =
                (from subKey in GetAllKeysInSection(sectionKey)
                 where subKey.ToLowerInvariant().StartsWith($"{keyID.ToLowerInvariant()}.") &&
                 subKey.Length > keyID.Length + 1
                 select subKey.Split('.')[1].ToLowerInvariant()).ToArray();

            foreach (string subKey in subKeys)
            {
                if (dict.ContainsKey(subKey)) continue;
                dict.Add(subKey, GetValue<T>(sectionKey, $"{keyID}.{subKey}"));
            }

            return dict;
        }

        public Dictionary<EnumType, T> GetValueEnumDictionary<T, EnumType>(string sectionKey, string keyID) where EnumType : struct
        {
            Dictionary<string, T> stringDict = GetValueStringDictionary<T>(sectionKey, keyID);

            Dictionary<EnumType, T> dict = new();
            foreach (string sKey in stringDict.Keys)
            {
                if (string.IsNullOrWhiteSpace(sKey)) continue;
                bool success = Enum.TryParse(sKey, true, out EnumType eKey);
                if (!success) continue;
                if (dict.ContainsKey(eKey)) continue;
                dict.Add(eKey, stringDict[sKey]);
            }

            return dict;
        }

        public T[] GetValueArrayEnumIndex<T, TIndex>(string sectionKey, string keyID) where TIndex : Enum
        {
            T[] values = new T[Enum.GetValues(typeof(TIndex)).Length];

            for (int i = 0; i < values.Length; i++)
            {
                string fullKey = $"{keyID}.{(TIndex)(object)i}";

                if (!ValueExists(sectionKey, fullKey))
                {
                    values[i] = default;
                    continue;
                }

                values[i] = GetValue<T>(sectionKey, keyID);
            }

            return values;
        }

        /// <summary>
        /// Get an array of comma-separated lower case strings.
        /// </summary>
        /// <param name="sectionKey">Section key</param>
        /// <param name="keyID">Value key</param>
        /// <param name="distinct">Should multiple instances of the same string be removed?</param>
        /// <returns>An array of strings</returns>
        public string[] GetValueArrayNormalizedStrings(string sectionKey, string keyID, bool distinct = false)
        {
            string valuesString = GetRawValue(sectionKey, keyID);
            if (string.IsNullOrEmpty(valuesString)) return Array.Empty<string>();

            IEnumerable<string> values = valuesString.Split(',', SPLIT_OPTIONS_REMOVE_EMPTY_AND_TRIM);

            if (!values.Any()) return Array.Empty<string>();
            values = (from string value in values select value.ToLowerInvariant());

            if (distinct)
                values = values.Distinct();

            return values.ToArray();
        }

        public T[][] GetValueArrayArrayEnumIndex<T, TIndex>(string sectionKey, string keyID) where TIndex : Enum
        {
            T[][] values = new T[Enum.GetValues(typeof(TIndex)).Length][];

            for (int i = 0; i < values.Length; i++)
            {
                string fullKey = $"{keyID}.{(TIndex)(object)i}";

                if (!ValueExists(sectionKey, fullKey))
                {
                    values[i] = default;
                    continue;
                }

                values[i] = GetValueArray<T>(sectionKey, fullKey);
            }

            return values;
        }

        public List<T> GetValueList<T>(string section, string key, params T[] defaultValues)
        {
            return new List<T>(GetValueArray(section, key, defaultValues));
        }

        public T GetValueAsFlags<T>(string section, string key) where T : Enum
        {
            T[] valueArray = GetValueArray<T>(section, key);
            int valueFlags = 0;
            foreach (T value in valueArray)
                valueFlags |= (int)(object)value;

            return (T)(object)valueFlags;
        }

        public Dictionary<TKey, TValue> GetValueDictionary<TKey, TValue>(string section, string key, params KeyValuePair<TKey, TValue>[] defaultPairs) where TKey : struct, Enum
        {
            if (!ValueExists(section, key))
                return new Dictionary<TKey, TValue>(defaultPairs);

            Dictionary<TKey, TValue> dict = new();

            foreach (TKey dictKey in Enum.GetValues(typeof(TKey)))
                dict.Add(dictKey, ConvertFromString<TValue>(GetRawValue(section, $"{key}.{dictKey}")));

            return dict;
        }

        public void SetValueDictionary<TKey, TValue>(string section, string key, Dictionary<TKey, TValue> value)
        {
            foreach (TKey dictKey in value.Keys)
                SetValue(section, $"{key}.{ConvertToString(dictKey)}", value[dictKey]);
        }

        public void SetValueDictionaryArray<TKey, TValue>(string section, string key, Dictionary<TKey, TValue[]> value)
        {
            foreach (TKey dictKey in value.Keys)
                SetValueArray(section, $"{key}.{ConvertToString(dictKey)}", value[dictKey]);
        }

        public void SetValue<T>(string section, string key, T value)
        {
            SetRawValue(section, key, ConvertToString(value));
        }

        protected virtual T ConvertFromString<T>(string valueString)
        {
            Type valueType = typeof(T);

            if (valueType.IsEnum) return (T)Enum.Parse(valueType, valueString, true);
            if (valueType == typeof(bool)) return (T)(object)Convert.ToBoolean(valueString, NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(byte)) return (T)(object)Convert.ToByte(valueString, NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(double)) return (T)(object)Convert.ToDouble(valueString, NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(float)) return (T)(object)Convert.ToSingle(valueString, NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(int)) return (T)(object)Convert.ToInt32(valueString, NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(long)) return (T)(object)Convert.ToInt64(valueString, NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(sbyte)) return (T)(object)Convert.ToSByte(valueString, NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(short)) return (T)(object)Convert.ToInt16(valueString, NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(string)) return (T)(object)valueString;
            if (valueType == typeof(uint)) return (T)(object)Convert.ToUInt32(valueString, NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(ushort)) return (T)(object)Convert.ToUInt16(valueString, NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(ulong)) return (T)(object)Convert.ToUInt64(valueString, NumberFormatInfo.InvariantInfo);

            throw new Exception($"Cannot read values of type {valueType.FullName} from INI file.");
        }

        protected virtual string ConvertToString<T>(T value)
        {
            Type valueType = typeof(T);
            object objectValue = value;

            if (valueType.IsEnum) return objectValue.ToString();
            if (valueType == typeof(bool)) return ((bool)objectValue).ToString(NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(byte)) return ((byte)objectValue).ToString(NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(double)) return ((double)objectValue).ToString(NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(float)) return ((float)objectValue).ToString(NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(int)) return ((int)objectValue).ToString(NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(long)) return ((long)objectValue).ToString(NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(sbyte)) return ((sbyte)objectValue).ToString(NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(short)) return ((short)objectValue).ToString(NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(string)) return objectValue.ToString(); // $"\"{value.ToString().Trim('\"')}\"";
            if (valueType == typeof(uint)) return ((uint)objectValue).ToString(NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(ushort)) return ((ushort)objectValue).ToString(NumberFormatInfo.InvariantInfo);
            if (valueType == typeof(ulong)) return ((ulong)objectValue).ToString(NumberFormatInfo.InvariantInfo);

            throw new Exception($"Cannot write values of type {valueType.FullName} to file.");
        }

        public Dictionary<TKey, TValue[]> GetValueDictionaryArray<TKey, TValue>(string section, string key) where TKey : struct, Enum
        {
            key = key.ToLowerInvariant();
            Dictionary<TKey, TValue[]> dict = new();

            foreach (TKey dictKey in Enum.GetValues(typeof(TKey)))
                dict.Add(dictKey, GetValueArray<TValue>(section, $"{key}.{dictKey}"));

            return dict;
        }

        public void SetValueArray<T>(string section, string key, params T[] values)
        {
            string arrayString = string.Join(
                GetArraySeparator<T>(),
                (from T value in values select ConvertToString(value)).ToArray());

            SetRawValue(section, key, arrayString);
        }

        protected virtual char GetArraySeparator<ElementType>()
        {
            if (typeof(ElementType) == typeof(string))
                return ';';

            return ',';
        }

        public void SaveToFile(string filePath, Encoding encoding = null)
        {
            string dataString = "";
            bool firstSection = true;

            foreach (string entryKey in GetAllSections())
            {
                if (firstSection) firstSection = false;
                else dataString += "\r\n";
                dataString += $"[{entryKey}]\r\n";

                foreach (string keyID in GetAllKeysInSection(entryKey))
                    dataString += $"{keyID}={GetRawValue(entryKey, keyID)}\r\n";
            }

            File.WriteAllText(filePath, dataString, encoding ?? Encoding.UTF8);
        }

        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return "0";
            key = KEY_NORMALIZATION_REGEX.Replace(key.ToLowerInvariant().Replace(" ", "-"), "-").Trim('-');
            while (key.Contains("--")) key = key.Replace("--", "-");
            return key;
        }

        public static T CreateFromRawString<T>(string dataString) where T : INIFile, new()
        {
            T dataFile = new();
            dataFile.ParseString(dataString);
            return dataFile;
        }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        public void Dispose()
        {
            Clear();
            GC.SuppressFinalize(this);
        }
    }
}
