using Huffman.CLI;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Huffman.Tests;

public class HuffmanTests
{
    private readonly ITestOutputHelper _output;
    private readonly string _helloText = "hello world";
    private readonly byte[] _helloCompressedSnapshot = new byte[] {
        4, 0, 0, 0, 1, 0, 0, 0, 32, 0, 1, 0, 0, 0, 100, 0, 1, 0, 0,
        0, 101, 0, 1, 0, 0, 0, 104, 0, 3, 0, 0, 0, 108, 0, 2, 0, 0,
        0, 111, 0, 1, 0, 0, 0, 114, 0, 1, 0, 0, 0, 119, 0, 1, 0, 0,
        0, 0, 1, 4, 0, 0, 0, 105, 101, 142, 242, 23, };

    public HuffmanTests(ITestOutputHelper output) {
        _output = output;
    }

    [Fact]
    public void Compress_Returns_Correct_Result() {
        // Arrange
        var sut = new HuffmanCoder();
        var input = new BinaryReader(new MemoryStream(Encoding.UTF8.GetBytes(_helloText)));
        var expectedBytes = _helloCompressedSnapshot;
        var resultStream = new MemoryStream();
        var writer = new BinaryWriter(resultStream);

        // Act
        sut.Compress(input, writer);
        var got = resultStream.ToArray();

        // Assert
        try {
            Assert.Equal(expectedBytes, got);
        }
        catch (XunitException) {
            _output.WriteLine($"WARNING: {nameof(_helloCompressedSnapshot)} did not match. Update it to the below?");
            _output.WriteLine(ByteArrayToString(got));
            throw;
        }
    }

    [Fact]
    public void Decompress_Returns_Correct_Result() {
        // Arrange
        var sut = new HuffmanCoder();
        var input = new BinaryReader(new MemoryStream(_helloCompressedSnapshot));
        var expectedText = _helloText;
        var resultStream = new MemoryStream();
        var writer = new BinaryWriter(resultStream);

        // Act
        sut.Decompress(input, writer);
        writer.Flush();
        var got = Encoding.UTF8.GetString(resultStream.ToArray());

        // Assert
        Assert.Equal(expectedText, got);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("aa")]
    [InlineData("abb")]
    [InlineData("aab")]
    [InlineData(@"hello 

world")]
    [MemberData(nameof(GetFullRangeOfBytes))]
    public void Round_Trip_Matches(string text) {
        RoundTripMatches(text);
    }

    [Fact]
    public void Round_Trip_Matches_And_Compresses_Large_Data() {
        const float expectedCompressionPercent = 0.50f;
        const int numLines = 100;

        StringBuilder sb = new();
        foreach (var _ in Enumerable.Range(1, numLines)) {
            sb.AppendLine(_helloText);
        }

        RoundTripMatches(sb.ToString(), expectedCompressionPercent);
    }

    public static IEnumerable<object[]> GetFullRangeOfBytes() {
        {// visible characters
            var text = "";
            for (var i = 0; i < 256; i++) {
                char c = (char)i;
                if (c is >= ' ' and <= 'z') {
                    text += c;
                }
            }
            yield return new object[] { text };
        }
        {// all possible values
            var text = "";
            for (var i = 0; i < 256; i++) {
                text += (char)i;
            }
            yield return new object[] { text };
        }
    }

    private void RoundTripMatches(string sourceText, float? expectedCompressionThreshold = null) {
        // Arrange
        byte[] intermediateCompressResult;
        byte[] decompressResult;
        int sourceLength;
        int compressedLength;

        // Act
        {
            var sut = new HuffmanCoder();
            var sourceBytes = Encoding.UTF8.GetBytes(sourceText);
            sourceLength = sourceBytes.Length;
            var reader = new BinaryReader(new MemoryStream(sourceBytes));
            var resultStream = new MemoryStream();
            var writer = new BinaryWriter(resultStream);
            sut.Compress(reader, writer);
            intermediateCompressResult = resultStream.ToArray();
            compressedLength = intermediateCompressResult.Length;
        }
        {
            var sut = new HuffmanCoder();
            var reader = new BinaryReader(new MemoryStream(intermediateCompressResult));
            var resultStream = new MemoryStream();
            var writer = new BinaryWriter(resultStream);
            sut.Decompress(reader, writer);
            decompressResult = resultStream.ToArray();
        }

        // Assert
        string resultText = Encoding.UTF8.GetString(decompressResult);
        Assert.Equal(sourceText, resultText);

        if (expectedCompressionThreshold is not null) {
            float compressionFactor = (float)compressedLength / sourceLength;
            try {
                Assert.InRange(compressionFactor, 0, expectedCompressionThreshold.Value);
            }
            catch {
                _output.WriteLine($"Source length     : {sourceLength}");
                _output.WriteLine($"Compressed length : {compressedLength}");
                _output.WriteLine($"Compression       : {compressionFactor:p}");
                throw;
            }
        }
    }

    private static string ByteArrayToString(byte[] bytes) {
        var sb = new StringBuilder("new byte[] { ");
        foreach (var b in bytes) {
            sb.Append(b + ", ");
        }
        sb.Append('}');
        return sb.ToString();
    }
}
