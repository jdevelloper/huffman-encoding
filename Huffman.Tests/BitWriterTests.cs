using Huffman.CLI;
using Moq;

namespace Huffman.Tests;
public class BitWriterTests
{
    [Fact]
    public void Write_Writes_1_Bit_After_Dispose() {
        // Arrange
        var streamMock = new Mock<BinaryWriter>();

        // Act
        using (var sut = new BitWriter(streamMock.Object)) {
            sut.Write(true);
        }

        // Assert
        streamMock.Verify(s => s.Write(It.IsAny<byte>()), Times.Once);
        streamMock.Verify(s => s.Write((byte)0b00000001), Times.Once);
    }

    [Fact]
    public void Write_Writes_0_Bit_After_Dispose() {
        // Arrange
        var streamMock = new Mock<BinaryWriter>();

        // Act
        using (var sut = new BitWriter(streamMock.Object)) {
            sut.Write(false);
        }

        // Assert
        streamMock.Verify(s => s.Write(It.IsAny<byte>()), Times.Once);
        streamMock.Verify(s => s.Write((byte)0b00000000), Times.Once);
    }

    [Fact]
    public void Write_Writes_1111_After_Dispose() {
        // Arrange
        var streamMock = new Mock<BinaryWriter>();

        // Act
        using (var sut = new BitWriter(streamMock.Object)) {
            sut.Write(true);
            sut.Write(true);
            sut.Write(true);
            sut.Write(true);
        }

        // Assert
        streamMock.Verify(s => s.Write(It.IsAny<byte>()), Times.Once);
        streamMock.Verify(s => s.Write((byte)0b00001111), Times.Once);
    }

    [Fact]
    public void Write_Writes_10000011_Before_Dispose() {
        // Arrange
        var streamMock = new Mock<BinaryWriter>();
        using var sut = new BitWriter(streamMock.Object);

        // Act
        sut.Write(true);//0
        sut.Write(true);//1
        sut.Write(false);//2
        sut.Write(false);//3
        sut.Write(false);//4
        sut.Write(false);//5
        sut.Write(false);//6
        sut.Write(true);//7

        // Assert
        streamMock.Verify(s => s.Write(It.IsAny<byte>()), Times.Once);
        streamMock.Verify(s => s.Write((byte)0b10000011), Times.Once);
    }

    [Fact]
    public void Write_Writes_2_Bytes() {
        // Arrange
        var streamMock = new Mock<BinaryWriter>();
        using var sut = new BitWriter(streamMock.Object);

        // Act
        sut.Write(false);//0
        sut.Write(false);//1
        sut.Write(false);//2
        sut.Write(false);//3
        sut.Write(false);//4
        sut.Write(false);//5
        sut.Write(false);//6
        sut.Write(false);//7

        sut.Write(true);//0
        sut.Write(true);//1
        sut.Write(true);//2
        sut.Write(true);//3
        sut.Write(true);//4
        sut.Write(true);//5
        sut.Write(true);//6
        sut.Write(true);//7

        // Assert
        streamMock.Verify(s => s.Write(It.IsAny<byte>()), Times.Exactly(2));
    }

    [Fact]
    public void Write_Writes_1_Plus_Bytes_With_Dispose() {
        // Arrange
        var streamMock = new Mock<BinaryWriter>();

        // Act
        using (var sut = new BitWriter(streamMock.Object)) {
            sut.Write(false);//0
            sut.Write(false);//1
            sut.Write(false);//2
            sut.Write(false);//3
            sut.Write(false);//4
            sut.Write(false);//5
            sut.Write(false);//6
            sut.Write(false);//7

            sut.Write(false);//0 
        }
        // Assert
        streamMock.Verify(s => s.Write(It.IsAny<byte>()), Times.Exactly(2));
    }

    [Fact]
    public void Dispose_Writes_Nothing_If_Nothing_Is_Buffered() {
        // Arrange
        var streamMock = new Mock<BinaryWriter>();

        // Act
        using (var sut = new BitWriter(streamMock.Object)) {
        }

        // Assert
        streamMock.Verify(s => s.Write(It.IsAny<byte>()), Times.Never);
    }
}
