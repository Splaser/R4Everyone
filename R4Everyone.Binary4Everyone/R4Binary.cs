using System.Text;

namespace R4Everyone.Binary4Everyone;

public static class R4Binary
{
    public static Encoding CurrentEncoding { get; set; } = Encoding.UTF8;

    public static void SeekAlign4WithExtraBlock(Stream stream)
    {
        var pos = stream.Position;
        var aligned = (pos + 3) & ~3;
        if (pos % 4 == 0) aligned += 4;
        stream.Seek(aligned, SeekOrigin.Begin);
    }

    public static (string Title, string? Description) ReadItemMeta(BinaryReader reader, bool game = false)
    {
        var titleBytes = new List<byte>();
        byte bx;
        while ((bx = reader.ReadByte()) != 0)
            titleBytes.Add(bx);

        if (game)
        {
            reader.BaseStream.Seek((4 - reader.BaseStream.Position % 4) % 4, SeekOrigin.Current);
            return (CurrentEncoding.GetString(titleBytes.ToArray()), null);
        }

        var descriptionBytes = new List<byte>();
        byte by;
        while ((by = reader.ReadByte()) != 0)
            descriptionBytes.Add(by);

        reader.BaseStream.Seek((4 - reader.BaseStream.Position % 4) % 4, SeekOrigin.Current);

        return (
            CurrentEncoding.GetString(titleBytes.ToArray()),
            CurrentEncoding.GetString(descriptionBytes.ToArray())
        );
    }

    public static List<byte[]> ReadCheatCodes(BinaryReader reader, ulong numChunks)
    {
        if (numChunks == 0) return [];

        var cheatCodes = new List<byte[]>((int)Math.Min(numChunks, 1024));
        for (ulong i = 0; i < numChunks; i++)
        {
            var cheatCode = reader.ReadBytes(4);
            if (cheatCode.Length < 4)
                throw new InvalidDataException("Incomplete cheat code found.");
            cheatCodes.Add(cheatCode);
        }

        return cheatCodes;
    }

    public static void WriteTitleAndDesc(BinaryWriter writer, string title, string description)
    {
        writer.Write(CurrentEncoding.GetBytes(title));
        writer.Write((byte)0x00);
        writer.Write(CurrentEncoding.GetBytes(description));
        SeekAlign4WithExtraBlock(writer.BaseStream);
    }
}