using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KvjBoardGames.OnlineFunctions
{
    public static class ByteTools
    {
        public static void WriteBytes(byte[] to, int startIndex, byte[] from)
        {
            Array.Copy(from, 0, to, startIndex, from.Length);
        }

        public static void WriteUint16(byte[] to, int startIndex, ushort toWrite)
        {
            WriteBytes(to, startIndex, BitConverter.GetBytes(toWrite));
        }

        public static void WriteBool(byte[] to, int startIndex, bool toWrite)
        {
            WriteBytes(to, startIndex, BitConverter.GetBytes(toWrite));
        }

        public static byte[] ReadBytes(byte[] from, int startIndex, int length)
        {
            byte[] b = new byte[length];
            Array.Copy(from, startIndex, b, 0, length);
            return b;
        }

        public static ushort ReadUint16(byte[] from, int startIndex)
        {
            return BitConverter.ToUInt16(from, startIndex);
        }

        public static bool ReadBool(byte[] from, int startIndex)
        {
            return BitConverter.ToBoolean(from, startIndex);
        }
    }
}
