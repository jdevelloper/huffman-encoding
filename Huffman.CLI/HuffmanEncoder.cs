using System.Diagnostics;
using System.Text;

namespace Huffman.CLI;

internal class HuffmanEncoder : IEncoder
{
    private record class Node(
        int Count,
        short Symbol = default,
        Node? Left = null,
        Node? Right = null);

    /// <summary>
    /// Compresses the given string using Huffman coding and returns the compressed result as a byte array.
    /// </summary>
    public void Compress(BinaryReader reader, BinaryWriter writer) {
        int[] counts = ByteCounts.Analyze(reader);
        AddTerminatorToCounts(counts);
        short terminator = GetTerminatorSymbol(counts);

        ByteCounts.Serialize(counts, writer);

        Node huffmanTree = BuildHuffmanTree(counts);
        string?[] codes = BuildCodes(huffmanTree, counts.Length);

        // Some debug-only code
        DebugWriteCodeArray(codes);
        DebugAssertUniquePrefixes(codes);

        using var bitWriter = new BitWriter(writer);
        bool sentTerminator = false;
        reader.Restart();

        while (!reader.EOF() || !sentTerminator) {
            short symbol = !reader.EOF() ? reader.ReadByte() : terminator;
            string codeString = codes[symbol] ?? throw new Exception($"Code string for ascii code {symbol} is null");

            foreach (char codeChar in codeString.ToCharArray()) {
                bitWriter.Write(codeChar is '1');
            }
            sentTerminator = symbol == terminator;
        }
    }

    /// <summary>
    /// Decompresses the given reader using Huffman coding
    /// and returns the decompressed result in the writer stream.
    /// </summary>
    public void Decompress(BinaryReader reader, BinaryWriter writer) {
        int[] counts = ByteCounts.Deserialize(reader);
        short terminator = GetTerminatorSymbol(counts);

        Node huffmanTree = BuildHuffmanTree(counts);

        // Displays a code table in debug mode.
        DebugWriteCodeArray(BuildCodes(huffmanTree, counts.Length));
        DebugAssertUniquePrefixes(BuildCodes(huffmanTree, counts.Length));

        BitReader bitReader = new(reader);
        Node? node = huffmanTree;

        foreach (bool bit in bitReader) {
            if (!bit) {
                node = node.Left;
            }
            else {
                node = node.Right;
            }
            if (node is null) {
                throw new NullReferenceException("Node is null");
            }
            if (IsLeaf(node)) {
                if (node.Symbol == terminator) {
                    return;
                }
                writer.Write((byte)node.Symbol);
                node = huffmanTree; // restart at root
            }
        }
    }

    /// <summary>
    /// The terminator is represented in the counts, and its code signals the end of the compressed data.
    /// </summary>
    private static void AddTerminatorToCounts(int[] counts) {
        // We will encode a 'special' terminating character
        // so that we can encode an end of data mark.
        // It must be outside the range of a byte; it is 256, the last possition in the array.
        short terminator = GetTerminatorSymbol(counts);
        ref int terminatorCount = ref counts[terminator];
        Debug.Assert(terminatorCount is 0, "The last spot in the counts array must be reserved for terminator");
        terminatorCount++;
    }

    private static short GetTerminatorSymbol(int[] counts) {
        // last position represents the terminator character
        return (short)(counts.Length - 1);
    }

    /// <summary>
    /// Builds a Huffman tree for the counts and returns it as a root Node object.
    /// </summary>
    /// <param name="counts">indexes are characters, values are counts of those characters</param>
    private static Node BuildHuffmanTree(int[] counts) {
        PriorityQueue<Node, int> queue = new();

        queue.EnqueueRange(
            from c in GetPossibleChars(counts)
            where counts[c] > 0
            select new Node(Count: counts[c], Symbol: c) into node
            select (node, node.Count)
        );

        // 2 alternate ways of doing the above:
        // var seedNodes = GetPossibleChars()
        //    .Select(c => new Node(Count: counts[c], Symbol: c))
        //    .Where(n => n.Count > 0)
        //    .Select(n => (n, n.Count));
        // queue.EnqueueRange(seedNodes);
        //
        // foreach (var c in GetPossibleChars()) {
        //    if (counts[c] > 0) {
        //        queue.Enqueue(new(counts[c], c), counts[c]);
        //    }
        //}

        while (queue.Count >= 2) {
            var l = queue.Dequeue();
            var r = queue.Dequeue();
            var internalNode = new Node(Count: l.Count + r.Count, Left: l, Right: r);
            queue.Enqueue(internalNode, internalNode.Count);
        }
        return queue.Dequeue();
    }

    /// <summary>
    /// Builds a lookup table for encoding a byte into the bit code. 
    /// For this intermediate representation, bits are strings like "1010". 
    /// If the string is null, then its corresponding byte is not present in the source.
    /// </summary>
    /// <param name="huffmanTree">The root node</param>
    /// <returns>indexes are characters(bytes), values are counts of those characters</returns>
    private static string?[] BuildCodes(Node huffmanTree, int size) {
        string?[] codes = new string?[size];

        void Traverse(Node n, string code) {
            if (n.Left is null) {
                codes[n.Symbol] = code;
            }
            else {
                Debug.Assert(n.Right is not null);
                Traverse(n.Left, code + "0");
                Traverse(n.Right, code + "1");
            }
        }

        Traverse(huffmanTree, "");
        return codes;
    }

    /// <summary>
    /// Enumerates all potential byte values ending with 
    /// the reserved value out side of byte range.
    /// </summary>
    private static IEnumerable<short> GetPossibleChars(int[] counts) {
        for (short i = 0; i < counts.Length; i++) {
            yield return i;
        }
    }

    private static bool IsLeaf(Node n) {
        return n.Left is null;
    }

    /// <summary>
    /// A debug method to display codex.
    /// Write to the debug window a list of pairs in this format: char:code\n 
    /// </summary>
    /// <param name="codes">Codes where indexes are keys and values are resulting codes.</param>
    [Conditional("DEBUG")]
    private static void DebugWriteCodeArray(string?[] codes) {
        short terminator = (short)(codes.Length - 1); // last position represents the terminator character
        StringBuilder sb = new();
        for (short i = 0; i < codes.Length; i++) {
            if (codes[i] is not null) {
                string displayChar = "0x" + i.ToString("X2");
                if (i == terminator) {
                    displayChar += " ";
                }
                else {
                    char c = (char)i;
                    if (!char.IsControl(c) && !char.IsWhiteSpace(c)) {
                        displayChar += " " + c.ToString();
                    }
                    else {
                        displayChar += "  ";
                    }
                }
                sb.AppendLine($"{displayChar}:{codes[i]}");
            }
        }
        Debug.WriteLine(sb.ToString());
    }

    /// <summary>
    /// A debug method to assert the Huffman invariate of prefix (free) codes,
    /// that if no codeword is a prefix of another one.
    /// </summary>
    /// <param name="codes">Codes where indexes are keys and values are resulting codes.</param>
    [Conditional("DEBUG")]
    private static void DebugAssertUniquePrefixes(string?[] codes) {
        HashSet<string> seen = new();
        for (int i = 0; i < codes.Length; i++) {
            string? code = codes[i];
            if (code is not null) {
                string prefix = "";
                foreach (char c in code) {
                    prefix += c;
                    if (seen.Contains(prefix)) {
                        Debug.Fail($"Assert fail: Prefix '{prefix}' of code '{code}' for ascii {i} matches and existing code.");
                    }
                }
                seen.Add(code);
            }
        }
    }
}
