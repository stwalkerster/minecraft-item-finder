namespace GoatHornFinder;

public class AnvilFile
{
    private readonly string anvilFile;
    private readonly IEnumerable<Chunk> chunks;

    public AnvilFile(string anvilFile)
    {
        this.anvilFile = anvilFile;

        this.chunks = this.LoadChunks();
    }
    
    private IEnumerable<Chunk> LoadChunks()
    {
        var fileStream = File.OpenRead(this.anvilFile);

        var regionParts = Path.GetFileName(this.anvilFile).Split('.');
        var regionX = int.Parse(regionParts[1]);
        var regionZ = int.Parse(regionParts[2]);

        var offsetHeader = new byte[4096];
        var timestampHeader = new byte[4096];

        fileStream.Read(offsetHeader, 0, 4096);
        fileStream.Read(timestampHeader, 0, 4096);

        var list = new List<Chunk>();

        for (var i = 0; i < 1024; i++)
        {
            var tableOffset = i * 4;

            var offsetData = new byte[4];
            offsetHeader.AsSpan(tableOffset, 3).ToArray().CopyTo(offsetData, 1);
            var offset = BitConverter.ToInt32(offsetData.Reverse().ToArray());

            var sectorCount = (int)offsetHeader[tableOffset + 3];

            var lastModSeconds = BitConverter.ToInt32(timestampHeader.AsSpan(tableOffset, 4).ToArray().Reverse().ToArray());

            fileStream.Seek(offset * 4096, SeekOrigin.Begin);

            var dataBuffer = new byte[sectorCount * 4096];
            fileStream.Read(dataBuffer, 0, sectorCount * 4096);

            list.Add(new Chunk(offset, sectorCount, i, DateTime.UnixEpoch.AddSeconds(lastModSeconds), dataBuffer, regionX, regionZ));
        }

        fileStream.Close();
        return list.Where(x => x.Flag == string.Empty);
    }

    public IEnumerable<Chunk> GetChunks()
    {
        return this.chunks;
    }
}