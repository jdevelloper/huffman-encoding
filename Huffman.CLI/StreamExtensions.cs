namespace Huffman.CLI;

public static class StreamExtensions
{
    /// <summary>
    /// EOF indicates the end of the stream.
    /// </summary>
    public static bool EOF(this BinaryReader binaryReader) {
        Stream bs = binaryReader.BaseStream;
        return bs.Position == bs.Length;
    }

    /// <summary>
    /// Restarts the stream from the begining, it discards any data that has been buffered by the StreamReader.
    /// </summary>
    /// <param name="reader"></param>
    public static void Restart(this StreamReader reader) {
        reader.DiscardBufferedData();
        reader.BaseStream.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
    /// Restarts the stream from the begining, it discards any data that has been buffered by the StreamReader.
    /// </summary>
    /// <param name="reader"></param>
    public static void Restart(this BinaryReader reader) {
        reader.BaseStream.Seek(0, SeekOrigin.Begin);
    }
}
