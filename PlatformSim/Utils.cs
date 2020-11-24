using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PlatformSim {
    public static class Utils {
        //http://www.codeproject.com/Articles/36747/Quick-and-Dirty-HexDump-of-a-Byte-Array
        public static string HexDump(this byte[] bytes, ulong baseAddress = 0, int bytesPerLine = 16) {
            if (bytes == null)
                return "<null>";

            var bytesLength = bytes.Length;

            var HexChars = "0123456789ABCDEF".ToCharArray();

            var firstHexColumn = 16 // 16 characters for the address
                                 + 3; // 3 spaces

            var firstCharColumn = firstHexColumn + bytesPerLine * 3 // - 2 digit for the hexadecimal value and 1 space
                                                 + (bytesPerLine - 1) / 8 // - 1 extra space every 8 characters from the 9th
                                                 + 2; // 2 spaces 

            var lineLength = firstCharColumn + bytesPerLine // - characters to show the ascii value
                                             + Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

            var line = (new String(' ', lineLength - 2) + Environment.NewLine + Environment.NewLine).ToCharArray();
            var expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            var result = new StringBuilder(expectedLines * lineLength);

            for (var i = 0; i < bytesLength; i += bytesPerLine) {
                var addr = baseAddress + (uint) i;

                for (var p = 0; p < 16; p++) {
                    line[p] = HexChars[(addr >> (60 - (p * 4))) & 0xF];
                }

                var hexColumn = firstHexColumn;
                var charColumn = firstCharColumn;

                for (var j = 0; j < bytesPerLine; j++) {
                    if (j > 0 && (j & 7) == 0)
                        hexColumn++;
                    if (i + j >= bytesLength) {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else {
                        var b = bytes[i + j];
                        line[hexColumn] = HexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];
                        line[charColumn] = asciiSymbol(b);
                    }

                    hexColumn += 3;
                    charColumn++;
                }

                result.Append(line);
            }

            return result.ToString();
        }

        static char asciiSymbol(byte val) {
            if (val < 32)
                return '.'; // Non-printable ASCII
            if (val < 127)
                return (char) val; // Normal ASCII

            // Handle the hole in Latin-1
            if (val == 127)
                return '.';
            if (val < 0x90)
                return "€.‚ƒ„…†‡ˆ‰Š‹Œ.Ž."[val & 0xF];
            if (val < 0xA0)
                return ".‘’“”•–—˜™š›œ.žŸ"[val & 0xF];
            if (val == 0xAD)
                return '.'; // Soft hyphen: this symbol is zero-width even in monospace fonts

            return (char) val; // Normal Latin-1
        }

        public static string ToString<X,Y>(this Dictionary<X,Y> dict) {
            var b = new StringBuilder();

            foreach (var kv in dict) {
                if (b.Length > 0) {
                    b.Append(", ");
                }

                b.Append($"{kv.Key}: {kv.Value}");
            }
            
            return b.ToString();
        }
    }
}