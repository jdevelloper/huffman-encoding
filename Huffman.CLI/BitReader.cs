using System.Collections;

namespace Huffman.CLI;

internal class BitReader : IEnumerable<bool>
{
    private readonly BinaryReader _reader;

    public BitReader(BinaryReader reader) {
        if (!BitConverter.IsLittleEndian) {
            throw new NotSupportedException("This is not built for big-endian systems.");
            // Future: Can handle with BinaryPrimitives.ReverseEndianness(myByte)
        }

        _reader = reader;
    }

    public IEnumerator<bool> GetEnumerator() {
        while (!_reader.EOF()) {
            byte source = _reader.ReadByte();
            for (int bitPosition = 0; bitPosition < 8; bitPosition++) {
                byte mask = 1;
                mask <<= bitPosition;
                yield return (source & mask) == mask;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}
