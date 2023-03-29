using Huffman.CLI;

namespace Huffman.Tests;

public class ByteCountsTests
{
    [Fact]
    public void Analyze_Counts_Bytes_Correctly() {
        // Arrange
        byte[] bytes = new byte[] { 1, 2, 3, 4, 4, 4 };
        var stream = new MemoryStream(bytes);
        var reader = new BinaryReader(stream);

        // Act
        int[] counts = ByteCounts.Analyze(reader);

        // Assert
        Assert.Equal(1, counts[1]);
        Assert.Equal(1, counts[2]);
        Assert.Equal(1, counts[3]);
        Assert.Equal(3, counts[4]);
        Assert.Equal(0, counts[5]);
    }

    [Fact]
    public void MaxChar_Allows_For_Terminator() {
        Assert.Equal(byte.MaxValue + 1, ByteCounts.MaxChar);
        Assert.Equal(256, ByteCounts.MaxChar);
    }

    [Fact]
    public void Round_Trip_Matches_Counts_And_Delimiter() {
        // Arrange
        int[] counts = new int[ByteCounts.MaxChar + 1];
        counts[0] = 4;
        counts[1] = 1;
        counts[2] = 1;
        counts[3] = 1;
        counts[4] = 3;
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);

        // Act
        ByteCounts.Serialize(counts, writer);
        stream.Seek(0, SeekOrigin.Begin);
        var reader = new BinaryReader(stream);

        int delimiter = reader.ReadInt32();
        stream.Seek(0, SeekOrigin.Begin);
        int[] deserializedCounts = ByteCounts.Deserialize(reader);

        stream.Seek(-sizeof(int), SeekOrigin.End);
        int finalDelimiter = reader.ReadInt32();

        // Assert

        // delimiter must be outside range of occuring counts
        Assert.InRange(delimiter, counts.Max() + 1, short.MaxValue);
        Assert.Equal(5, delimiter);
        Assert.Equal(finalDelimiter, delimiter);

        Assert.Equal(counts, deserializedCounts);
    }

    [Fact]
    public void Deserialize_Does_Not_Read_Too_Far() {
        // Arrange
        int[] counts = new int[ByteCounts.MaxChar + 1];
        counts[0] = 4;
        counts[1] = 1;
        counts[2] = 1;
        counts[3] = 1;
        counts[4] = 3;
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);

        ByteCounts.Serialize(counts, writer);
        writer.Write("-------Some junk at the end------");

        stream.Seek(0, SeekOrigin.Begin);
        var reader = new BinaryReader(stream);

        int[] deserializedCounts = ByteCounts.Deserialize(reader);

        // Assert
        Assert.Equal(counts, deserializedCounts);
    }

    [Fact]
    public void GetPossibleChars_Returns_All_Possible_Byte_Values() {
        // Arrange
        var expectedValues = Enumerable
            .Range(0, ByteCounts.MaxChar + 1)
            .Select(x => (short)x);

        // Act
        var actualValues = ByteCounts.GetPossibleChars();

        // Assert
        Assert.Equal(expectedValues, actualValues);
    }
}
