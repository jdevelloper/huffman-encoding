using System.CommandLine;
using System.IO.Abstractions;

namespace Huffman.CLI;

class Program
{
    static async Task<int> Main(string[] args) {
        CommandLineParser clp = new(new HuffmanCoder(), new FileSystem());
        var rootCommand = clp.CreateCommandLineParser();
        return await rootCommand.InvokeAsync(args);
    }
}