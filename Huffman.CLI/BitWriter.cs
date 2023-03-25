namespace Huffman.CLI;

public class BitWriter : IDisposable
{
    private int _bufferedCount = 0;
    private byte _bufferBits = default;
    private readonly BinaryWriter _writer;
    private bool _disposed;

    public BitWriter(BinaryWriter writer) {
        if (!BitConverter.IsLittleEndian) {
            throw new NotSupportedException("This is not built for big-endian systems.");
            // Future: Can handle with BinaryPrimitives.ReverseEndianness(myByte)
        }

        _writer = writer;
    }

    public void Write(bool bit) {
        if (bit) {
            _bufferBits |= (byte)(1 << _bufferedCount);
        }
        _bufferedCount++;

        if (_bufferedCount == 8) {
            _writer.Write(_bufferBits);
            _bufferBits = default;
            _bufferedCount = default;
        }
    }

    /// <summary>
    /// Warning: This will flush the byte buffer, potentiallly completing the buffer with zeros.
    /// </summary>
    protected void Flush() {
        if (_bufferedCount > 0) {
            _writer.Write(_bufferBits);
            _bufferBits = default;
            _bufferedCount = default;
        }
        _writer.Flush();
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                Flush();
                _writer.Dispose();

            }
            _disposed = true;
        }
    }
}
