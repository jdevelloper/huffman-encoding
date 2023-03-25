using Huffman.CLI;

namespace Tests;

public class StreamExtensionsTests
{
    [Fact]
    public void EOF_BinaryReader_Detects_End() {
        // Arrange
        byte[] data = { 1, 2, 3 };
        using MemoryStream stream = new(data);
        using BinaryReader reader = new(stream);

        // Act
        Assert.False(reader.EOF());

        // Read all the data from the stream
        reader.ReadByte();
        reader.ReadByte();
        reader.ReadByte();

        // Test that EOF returns true after reading all the data
        Assert.True(reader.EOF());
    }

    [Fact]
    public void Restart_StreamReader_Restarts() {
        // Arrange
        byte[] data = { 1, 2, 3 };
        MemoryStream stream = new(data);
        StreamReader reader = new(stream);
        reader.Read();

        // Act
        reader.Restart();

        // Assert
        int firstByte = reader.Read();
        Assert.Equal(firstByte, data[0]);
    }

    [Fact]
    public void Restart_BinaryReader_Restarts() {
        // Arrange
        byte[] data = { 1, 2, 3 };
        MemoryStream stream = new(data);
        BinaryReader reader = new(stream);
        reader.Read();

        // Act
        reader.Restart();

        // Assert
        int firstByte = reader.Read();
        Assert.Equal(firstByte, data[0]);
    }
}