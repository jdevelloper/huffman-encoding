using Huffman.CLI;
using Moq;
using System.IO.Abstractions.TestingHelpers;

namespace Huffman.Tests;

public class CommandLineParserTests
{
    readonly string _testDirectory;
    readonly string _textFileName;
    readonly string _compressedFileName;
    readonly string _existingOutputFileName;

    public CommandLineParserTests() {
        // FUTURE: The disc is not accessed at this time but will be with integration tests.
        var prjDir = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent!;
        _testDirectory = Path.Combine(prjDir.FullName, "test-files");
        _textFileName = Path.Combine(_testDirectory, "text.txt");
        _compressedFileName = Path.Combine(_testDirectory, "compressed.huf");
        _existingOutputFileName = Path.Combine(_testDirectory, "existing-overwrite.txt");
    }

    [Fact]
    public void CompressHandler_Throws_When_File_Exists_And_No_Overwrite() {
        // Arrange
        var mockEncoder = new Mock<IEncoder>();
        var mockFileSystem = new MockFileSystem();
        var sut = new CommandLineParser(mockEncoder.Object, mockFileSystem);
        mockFileSystem.AddFile(_existingOutputFileName, "");

        // Act & Assert
        var ex = Assert.Throws<Exception>(
            () => sut.CompressHandler(_textFileName, _existingOutputFileName, overwrite: false));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public void DecompressHandler_Throws_When_File_Exists_And_No_Overwrite() {
        // Arrange
        var mockEncoder = new Mock<IEncoder>();
        var mockFileSystem = new MockFileSystem();
        var sut = new CommandLineParser(mockEncoder.Object, mockFileSystem);
        mockFileSystem.AddFile(_existingOutputFileName, "");

        // Act & Assert
        var ex = Assert.Throws<Exception>(
            () => sut.DecompressHandler(_compressedFileName, _existingOutputFileName, overwrite: false));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public void CreateCommandLineParser_Returns_RootCommand_With_Subcommands() {
        // Arrange
        var mockEncoder = new Mock<IEncoder>();
        var mockFileSystem = new MockFileSystem();
        var sut = new CommandLineParser(mockEncoder.Object, mockFileSystem);

        // Act
        var result = sut.CreateCommandLineParser();

        // Assert
        Assert.Equal(2, result.Subcommands.Count);
        Assert.Equal("compress", result.Subcommands[0].Name);
        Assert.Equal("decompress", result.Subcommands[1].Name);
    }

    [Fact]
    public void CompressHandler_Creates_Output_File_And_Calls_Compress() {
        // Arrange
        var mockEncoder = new Mock<IEncoder>();
        var mockFileSystem = new MockFileSystem();
        var sut = new CommandLineParser(mockEncoder.Object, mockFileSystem);
        var inFileName = _textFileName;
        var outFileName = Path.Combine(_testDirectory, $"output-{Guid.NewGuid()}.huf");

        mockFileSystem.AddFile(inFileName, "");

        // Act
        sut.CompressHandler(inFileName, outFileName, overwrite: false);

        // Assert
        mockEncoder.Verify(x => x.Compress(It.IsAny<BinaryReader>(), It.IsAny<BinaryWriter>()));

        var newFile = mockFileSystem.GetFile(outFileName);
        Assert.NotNull(newFile);
    }

    [Fact]
    public void DecompressHandler_Creates_Output_File_And_Calls_Decompress() {
        // Arrange
        var mockEncoder = new Mock<IEncoder>();
        var mockFileSystem = new MockFileSystem();
        var sut = new CommandLineParser(mockEncoder.Object, mockFileSystem);
        var inFileName = _compressedFileName;
        var outFileName = Path.Combine(_testDirectory, $"output-{Guid.NewGuid()}.txt");

        mockFileSystem.AddFile(inFileName, "");

        // Act
        sut.DecompressHandler(inFileName, outFileName, overwrite: false);

        // Assert
        mockEncoder.Verify(x => x.Decompress(It.IsAny<BinaryReader>(), It.IsAny<BinaryWriter>()));

        var newFile = mockFileSystem.GetFile(outFileName);
        Assert.NotNull(newFile);
    }
}
