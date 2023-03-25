using System.CommandLine;
using System.IO.Abstractions;
using System.Text;

namespace Huffman.CLI;

public class CommandLineParser
{
    private const string DEFAULT_ENCODED_EXTENSION = ".huf";
    private readonly IHuffmanCoder _huffmanCoder;
    private readonly IFileSystem _fileSystem;

    public CommandLineParser(IHuffmanCoder huffmanCoder, IFileSystem fileSystem) {
        _huffmanCoder = huffmanCoder;
        _fileSystem = fileSystem;
    }

    public RootCommand CreateCommandLineParser() {
        Option<string?> inputOption = new(
            name: "--input",
            description: "The input file",
            parseArgument: arg => {
                string? filePath = arg.Tokens.Single().Value;
                if (!File.Exists(filePath)) {
                    arg.ErrorMessage = $"{arg.Argument.Name} {filePath} does not exist";
                    return null;
                }
                return filePath;
            }) { IsRequired = true };

        inputOption.AddAlias("-i");

        Option<string?> outputOption = new(
            name: "--output",
            description: "The ouput file",
            parseArgument: arg => {
                string? filePath = arg.Tokens.Single().Value;
                if (filePath is null) {
                    return null;
                }
                return filePath;
            }) { IsRequired = false };

        outputOption.AddAlias("-o");

        Option<bool> overwriteOption = new(
            "--overwrite",
            getDefaultValue: () => false,
            description: "Overwrite the output file if it already exists");

        Command compressSubcommand = new("compress", "Compress the source.") {
            inputOption, outputOption, overwriteOption
        };
        Command decompressSubcommand = new("decompress", "Decompress and return the source.") {
            inputOption, outputOption, overwriteOption
        };

        compressSubcommand.SetHandler(CompressHandler, inputOption, outputOption, overwriteOption);
        decompressSubcommand.SetHandler(DecompressHandler, inputOption, outputOption, overwriteOption);

        RootCommand rootCommand = new("Huffman CLI");
        rootCommand.AddCommand(compressSubcommand);
        rootCommand.AddCommand(decompressSubcommand);
        return rootCommand;
    }

    public void CompressHandler(string? inFileName, string? outFileName, bool overwrite) {
        outFileName ??= inFileName! + DEFAULT_ENCODED_EXTENSION;

        var outFile = _fileSystem.FileInfo.New(outFileName);
        if (outFile.Exists) {
            if (!overwrite) {
                throw new Exception($"Output '${outFileName} already exists. Try overwrite flag?");
            }
            outFile.Delete();
        }
        CompressFile(
            _fileSystem.FileInfo.New(inFileName!),
            _fileSystem.FileInfo.New(outFileName));
    }

    public void DecompressHandler(string? inFileName, string? outFileName, bool overwrite) {
        if (outFileName is null) {
            // Create default out file name
            outFileName = inFileName!.Replace(".huf", "");
            if (outFileName == inFileName) {
                throw new Exception("Could not infer a proper output file.");
            }
        }

        var outFile = _fileSystem.FileInfo.New(outFileName);
        if (outFile.Exists) {
            if (!overwrite) {
                throw new Exception($"Output '${outFileName} already exists. Try overwrite flag?");
            }
            outFile.Delete();
        }

        DecompressFile(
            _fileSystem.FileInfo.New(inFileName!),
            outFile);
    }

    private void CompressFile(IFileInfo inFile, IFileInfo outFile) {
        using var inFileStream = inFile.OpenRead();
        using var outFileStream = outFile.OpenWrite();
        using var reader = new BinaryReader(inFileStream); // need to read char by char
        using var writer = new BinaryWriter(outFileStream, Encoding.UTF8, false);
        _huffmanCoder.Compress(reader, writer);
    }

    private void DecompressFile(IFileInfo inFile, IFileInfo outFile) {
        using var inFileStream = inFile.OpenRead();
        using var outFileStream = outFile.OpenWrite();
        using var reader = new BinaryReader(inFileStream);
        using var writer = new BinaryWriter(outFileStream);
        _huffmanCoder.Decompress(reader, writer);
    }
}

