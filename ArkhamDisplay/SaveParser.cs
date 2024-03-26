using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ArkhamDisplay
{
    public class SaveParser
    {
        protected string m_filePath;
        protected int m_id;
        protected string m_rawData;
        protected DateTime m_LastWriteTime;

        public SaveParser(string filePath, int id)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new NullReferenceException("Invalid file name!");
            }

            if (!Directory.Exists(filePath))
            {
                throw new FileNotFoundException("Could not find save file path at [" + filePath + "]!", filePath);
            }

            if (id < 0 || id > 3)
            {
                throw new ArgumentOutOfRangeException("SaveParser::id", "SaveParser ID value was invalid (must be between 0 and 3), value was " + id.ToString());
            }

            m_filePath = filePath;
            m_id = id;
            m_rawData = string.Empty;
            m_LastWriteTime = DateTime.MinValue;
        }

        protected string Decompress()
        {
            string finalResult = "";
            if (!File.Exists(GetFile()))
            {
                return finalResult;
            }

            byte[] buffer = File.ReadAllBytes(GetFile());

            int[] offsets = ExtractSaveOffsets(buffer);

            var reader = new BinaryReader(new MemoryStream(buffer));

            for (int i = 0; i < offsets.Length; i++)
            {
                reader.BaseStream.Seek(offsets[i], SeekOrigin.Begin);
                //- Header: 0x9e2a83c1 (2653586369)
                uint header = Get32(reader);
                uint unknown = Get32(reader, true); // + 4
                uint compressedSize = Get32(reader); // + 8
                uint decompressedSize = Get32(reader, true); // + 12

                reader.BaseStream.Position += 8;

                byte[] compressedBuffer = new byte[compressedSize];
                reader.Read(compressedBuffer, 0, (int)compressedSize);
                //var compressedBuffer = reader.ReadBytes((int)compressedSize);

                byte[] decompressed = new byte[decompressedSize];
                MiniLZO.MiniLZO.Decompress(compressedBuffer, decompressed);

                finalResult += Encoding.ASCII.GetString(decompressed);
            }

            return finalResult;
        }

        private static uint Get32(BinaryReader reader, bool skip = false)
        {
            long pos = reader.BaseStream.Position;

            uint a1 = reader.ReadUInt32();
            if (!skip)
            {
                /*var a2 = */
                reader.ReadUInt32();
            }
            uint a3 = reader.ReadUInt32();
            uint a4 = reader.ReadUInt32();

            // get32
            uint val = (a3 << 16) | a1 | (a4 << 24);

            // myswap32
            uint ret = ((val & 0xFF00) << 8) | ((val & 0xFF0000) >> 8) | (val >> 24) | (val << 24);

            reader.BaseStream.Position = pos + 4;

            return ret;
        }

        private static int[] ExtractSaveOffsets(byte[] buffer)
        {
            List<int> offsets = [];

            for (int i = 0; i < buffer.Length - 4; i++)
            {
                if (buffer[i] != 0x9E)
                {
                    continue;
                }

                if (buffer[i + 1] != 0x2A)
                {
                    continue;
                }

                if (buffer[i + 2] != 0x83)
                {
                    continue;
                }

                if (buffer[i + 3] != 0xC1)
                {
                    continue;
                }

                offsets.Add(i);
            }

            return offsets.ToArray();
        }

        public virtual bool Update()
        {
            var lastWriteTime = File.GetLastWriteTimeUtc(GetFile());
            if (lastWriteTime == m_LastWriteTime)
            {
                return false; //No need to update, file hasn't been written to since last check
            }

            m_LastWriteTime = lastWriteTime;

            lock (m_rawData)
            {
                m_rawData = Decompress();
            }

            return true;
        }

        public virtual bool HasKey(Entry entry, int requiredMatches) => HasKey(entry.id, requiredMatches) || HasKey(entry.alternateID, requiredMatches);

        public virtual bool HasKey(string key, int requiredMatches)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            return HasKeyCustomRegex(@"\b" + key + @"\b", requiredMatches);
        }

        public virtual bool HasKeyCustomRegex(string regex, int requiredMatches)
        {
            if (string.IsNullOrWhiteSpace(regex))
            {
                return false;
            }

            var rx = new Regex(regex);
            var collectibleMatches = rx.Matches(m_rawData);
            return collectibleMatches.Count >= requiredMatches;
        }

        public virtual string GetMatch(string regex)
        {
            var rx = new Regex(regex);
            var match = rx.Match(m_rawData);
            return match.ToString();
        }

        public virtual string GetLastMatch(string regex)
        {
            var rx = new Regex(regex);
            var matches = rx.Matches(m_rawData);

            if (matches.Count > 0)
            {
                return matches[matches.Count - 1].ToString();
            }

            return "";
        }

        protected virtual string GetFile() => System.IO.Path.Combine(m_filePath, "Save" + m_id.ToString() + ".sgd");

        public int GetID() => m_id;
    }
}