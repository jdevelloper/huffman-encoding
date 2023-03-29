using System.CommandLine;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Huffman.Tests")]

namespace Huffman.CLI;

class Program
{
    static async Task<int> Main(string[] args) {
        CommandLineParser clp = new(new HuffmanEncoder(), new FileSystem());
        var rootCommand = clp.CreateCommandLineParser();
        return await rootCommand.InvokeAsync(args);
    }
}