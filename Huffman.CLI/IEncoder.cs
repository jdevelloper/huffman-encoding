namespace Huffman.CLI;

public interface IEncoder
{
    void Compress(BinaryReader reader, BinaryWriter writer);
    void Decompress(BinaryReader reader, BinaryWriter writer);
}
