using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace NovaLauncher.Models.Controls
{
    public class NewVersion
    {
        public string _version;
        public NewVersion(string filePath)
        {
            byte[] FilesBytes = File.ReadAllBytes(filePath);
            byte[] MainSearch = AssetConverter("2B 00 2B 00 46 00 6F 00 72 00 74 00 6E 00 69 00 74 00 65 00 2B 00 52 00 65 00 6C 00 65 00 61 00 73 00 65 00 2D 00");

            int Offset = IndexOfOffset(FilesBytes, MainSearch, 0) + 38;
            List<byte> Version = new() { FilesBytes[Offset], FilesBytes[Offset + 2], FilesBytes[Offset + 6], FilesBytes[Offset + 8] };

            bool olderthan10 = false;

            if (Version[1] > 58)
            {
                byte[] hashBytes;
                using (var md5 = MD5.Create())
                using (var stream = File.OpenRead(filePath))
                {
                    hashBytes = md5.ComputeHash(stream);
                }

                string hashString = BitConverter.ToString(hashBytes).Replace("-", "");

                if (hashString.Equals("426971FE40C306DC97C8C7C26170157B"))
                {
                    _version = "1.7.2";
                }
                else
                {
                    MessageBox.Show($"Unknown version");
                    _version = "Failed finding a valid version";
                }

                return;
            }

            if (FilesBytes[Offset + 10] != 0)
                olderthan10 = true;

            if (olderthan10)
            {
                Version = new();
                Version.Add(FilesBytes[Offset]);
                Version.Add(FilesBytes[Offset + 4]);

                if (FilesBytes[Offset + 6] > 47)
                    Version.Add(FilesBytes[Offset + 6]);

                Version.Insert(1, 0x2E);
            }
            else
                Version.Insert(2, 0x2E);

            _version = Encoding.ASCII.GetString(FromHex(BitConverter.ToString(Version.ToArray())));
        }





        public static byte[] FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }

        public static int IndexOfOffset(byte[] array, byte[] sequence, int offset = 0)
        {
            for (int i = offset; i < array.Length - sequence.Length; i++)
            {
                if (array[i] != sequence[0]) continue;
                bool found = true;
                for (int j = 1; j < sequence.Length; j++)
                {
                    if (array[i + j] != sequence[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                    return i;
            }
            return -1;
        }

        public static int IndexOfSequence(byte[] buffer, byte[] pattern)
        {
            //CREDIT: https://stackoverflow.com/a/31107925
            int i = Array.IndexOf(buffer, pattern[0], 0);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                byte[] segment = new byte[pattern.Length];
                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual(pattern))
                    return i;
                i = Array.IndexOf(buffer, pattern[0], i + 1);
            }
            return -1;
        }

        public static byte[] AssetConverter(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return data;
        }
    }
}
