namespace Huffman.CLI;

public interface IHuffmanCoder
{
    void Compress(BinaryReader reader, BinaryWriter writer);
    void Decompress(BinaryReader reader, BinaryWriter writer);
}
