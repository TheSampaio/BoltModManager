using System.IO.Compression;
using System.Text;
using Bolt.Infrastructure.Archives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpCompress.Common;
using SharpCompress.Writers.SevenZip;

namespace Bolt.Tests;

[TestClass]
public sealed class ArchiveReaderTests
{
    private static readonly byte[] Content = "Bolt archive test"u8.ToArray();
    private static readonly string[] ExpectedExtensions = [".zip", ".7z", ".rar"];

    [TestMethod]
    public void SupportedExtensionsContainZipSevenZipAndRar()
    {
        var reader = new ArchiveReader();

        CollectionAssert.AreEquivalent(
            ExpectedExtensions,
            reader.SupportedExtensions.ToArray());
    }

    [TestMethod]
    [DataRow("zip")]
    [DataRow("7z")]
    [DataRow("rar")]
    public void ExtractSupportedArchiveExtractsItsFile(string format)
    {
        using var directory = new TestDirectory();
        var archivePath = directory.GetPath($"sample.{format}");
        var destination = directory.GetPath("extracted");

        CreateArchive(archivePath, format, "Data/file.txt", Content);

        var reader = new ArchiveReader();
        var entries = reader.ListEntries(archivePath);
        var extracted = reader.Extract(archivePath, destination);

        Assert.HasCount(1, entries);
        Assert.HasCount(1, extracted);
        Assert.AreEqual(Content.Length, entries[0].Length);
        CollectionAssert.AreEqual(Content, File.ReadAllBytes(directory.GetPath("extracted", "Data", "file.txt")));
    }

    [TestMethod]
    public void ExtractEntryOutsideDestinationIsSkipped()
    {
        using var directory = new TestDirectory();
        var archivePath = directory.GetPath("unsafe.zip");

        using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
        {
            using var stream = archive.CreateEntry("../outside.txt").Open();
            stream.Write(Content);
        }

        var reader = new ArchiveReader();
        var extracted = reader.Extract(archivePath, directory.GetPath("extracted"));

        Assert.IsEmpty(extracted);
        Assert.IsFalse(File.Exists(directory.GetPath("outside.txt")));
    }

    private static void CreateArchive(string path, string format, string entryName, byte[] content)
    {
        switch (format)
        {
            case "zip":
                using (var archive = ZipFile.Open(path, ZipArchiveMode.Create))
                using (var stream = archive.CreateEntry(entryName).Open())
                    stream.Write(content);
                break;

            case "7z":
                using (var output = File.Create(path))
                using (var writer = new SevenZipWriter(output, new SevenZipWriterOptions(CompressionType.LZMA2)))
                using (var input = new MemoryStream(content))
                    writer.Write(entryName, input, modificationTime: null);
                break;

            case "rar":
                CreateStoredRar(path, entryName.Replace('/', '\\'), content);
                break;

            default:
                Assert.Fail($"Unsupported test format: {format}");
                break;
        }
    }

    /// <summary>Creates a minimal RAR 4 archive containing one uncompressed file.</summary>
    private static void CreateStoredRar(string path, string entryName, byte[] content)
    {
        using var output = File.Create(path);
        using var writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen: false);

        writer.Write(new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00 });
        WriteRarHeader(writer, 0x73, 0, new byte[6]);

        var name = Encoding.UTF8.GetBytes(entryName);
        using var bodyStream = new MemoryStream();
        using (var body = new BinaryWriter(bodyStream, Encoding.UTF8, leaveOpen: true))
        {
            body.Write((uint)content.Length);
            body.Write((uint)content.Length);
            body.Write((byte)2);
            body.Write(CalculateCrc32(content));
            body.Write(0u);
            body.Write((byte)20);
            body.Write((byte)0x30);
            body.Write((ushort)name.Length);
            body.Write(0x20u);
            body.Write(name);
        }

        WriteRarHeader(writer, 0x74, 0x8000, bodyStream.ToArray());
        writer.Write(content);
        WriteRarHeader(writer, 0x7B, 0, []);
    }

    private static void WriteRarHeader(BinaryWriter writer, byte type, ushort flags, byte[] body)
    {
        var size = checked((ushort)(7 + body.Length));
        using var checksumStream = new MemoryStream();

        using (var checksumWriter = new BinaryWriter(checksumStream, Encoding.UTF8, leaveOpen: true))
        {
            checksumWriter.Write(type);
            checksumWriter.Write(flags);
            checksumWriter.Write(size);
            checksumWriter.Write(body);
        }

        writer.Write((ushort)CalculateCrc32(checksumStream.ToArray()));
        writer.Write(checksumStream.ToArray());
    }

    private static uint CalculateCrc32(ReadOnlySpan<byte> data)
    {
        var crc = uint.MaxValue;

        foreach (var value in data)
        {
            crc ^= value;

            for (var bit = 0; bit < 8; bit++)
                crc = (crc >> 1) ^ (0xEDB88320u & (uint)-(int)(crc & 1));
        }

        return ~crc;
    }
}
