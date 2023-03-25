using Huffman.CLI;
using System.Collections;

namespace Huffman.Tests;

public class BitReaderTests
{
    [Fact]
    public void BitReader_Enumerates_Bits_Correctly() {
        // Arrange
        byte[] testData = { 0xFF, 0x00, 0x0F };
        var stream = new MemoryStream(testData);
        var reader = new BinaryReader(stream);
        var bitReader = new BitReader(reader);
        var expectedBits = new BitArray(testData).Cast<bool>();

        // Act
        var actualBits = bitReader.ToList();

        // Assert
        Assert.Equal(expectedBits, actualBits);
    }

    [Fact]
    public void BitReader_Enumerates_Bits_Correctly_From_Middle() {
        // Arrange
        byte[] testData = { 0xFF, 0x00, 0x0F, 0xF0 };
        int skipBytes = testData.Length / 2;
        var stream = new MemoryStream(testData);
        var reader = new BinaryReader(stream);
        var bitReader = new BitReader(reader);
        var expectedBits = new BitArray(testData).Cast<bool>().Skip(skipBytes * 8);
        for (int i = 0; i < skipBytes; i++) {
            reader.ReadByte();
        }

        // Act
        var actualBits = bitReader.ToList();

        // Assert
        Assert.Equal(expectedBits, actualBits);
    }
}
