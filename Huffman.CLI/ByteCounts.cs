namespace Huffman.CLI;

internal static class ByteCounts
{
    /// <summary>
    /// MAX_CHAR is one more than typical to store a terminator in last position.
    /// We use short instread of byte to reeserve this slot for the terminator.
    /// </summary>
    private static readonly short MAX_CHAR = 1 << sizeof(byte) * 8;

    public static short MaxChar => MAX_CHAR;

    /// <summary>
    /// Builds and returns a count array from a source stream, counting the occurences of each character.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns>indexes are characters(ints), values are counts of those characters</returns>
    public static int[] Analyze(BinaryReader reader) {
        int[] counts = new int[MAX_CHAR + 1];
        while (!reader.EOF()) {
            byte b = reader.ReadByte();
            counts[b]++;
        }
        return counts;
    }

    /// <summary>
    /// Serializes count array into writer stream, delimiting start 
    /// and end with a value that is beyond any existing count or frequency.
    /// </summary>
    /// <param name="counts">indexes represent characters, values are counts of those characters</param>
    /// <param name="writer"></param>
    public static void Serialize(int[] counts, BinaryWriter writer) {
        int delimiter = counts.Max() + 1;

        // Announce delimiter by being first in stream.
        writer.Write(delimiter);

        foreach (short c in GetPossibleChars()) {
            if (counts[c] != 0) {
                writer.Write(counts[c]);
                writer.Write(c);
            }
        }

        // Now use delimiter
        writer.Write(delimiter);
    }

    public static int[] Deserialize(BinaryReader reader) {
        // First bits are a delimiter int.
        // Then pairs of count and character (represented as int and short),
        // End when we see delimiter again.
        try {
            int[] counts = new int[MAX_CHAR + 1];
            int delimiter = reader.ReadInt32();

            while (true) {
                int count = reader.ReadInt32();
                if (count == delimiter) {
                    return counts;
                }
                short character = reader.ReadInt16();
                counts[character] = count;
            }
        }
        catch (Exception ex) {
            throw new Exception("Could not deserialize counts.", ex);
        }
    }

    /// <summary>
    /// Enumerates all potential byte values ending with 
    /// the reserved value out side of byte range.
    /// </summary>
    public static IEnumerable<short> GetPossibleChars() {
        for (short i = 0; i <= MAX_CHAR; i++) {
            yield return i;
        }
    }
}
