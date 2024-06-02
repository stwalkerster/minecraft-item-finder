namespace GoatHornFinder;

using System.Diagnostics;
using System.IO.Compression;
using Raspite.Serializer;
using Raspite.Serializer.Tags;

[DebuggerDisplay("R: [{RegionX},{RegionZ}] [{RegionChunkX},{RegionChunkZ}] | x={ChunkX}, z={ChunkZ}{Flag}")]
public class Chunk
{
    public Chunk(int offset, int length, int regionIndex, DateTime lastModified, byte[] dataBuffer, int regionX, int regionZ)
    {
        this.Offset = offset;
        this.RegionIndex = regionIndex;
        this.LastModified = lastModified;
        this.RegionX = regionX;
        this.RegionZ = regionZ;
        this.Length = length;

        if (this.Offset == 0 && this.Length == 0)
        {
            this.Data = [];
            return;
        }
        
        var dataLength = BitConverter.ToInt32(dataBuffer.AsSpan(0, 4).ToArray().Reverse().ToArray());
        this.CompressionType = (CompressionType)dataBuffer[4];

        this.Data = dataBuffer.AsSpan(5, dataLength - 1).ToArray();
    }

    public byte[] Data { get; set; }

    public CompressionType CompressionType { get; set; }

    public int RegionIndex { get; }
    
    public int Offset { get; }
    public int Length { get; }
    
    public int RegionX { get; }
    public int RegionZ { get; }

    public async Task<CompoundTag> GetRootTag()
    {
        Stream dataStream = new MemoryStream(this.Data);
        
        if (this.CompressionType == CompressionType.ZLib)
        {
            dataStream = new ZLibStream(dataStream, CompressionMode.Decompress);
        }
        
        return await BinaryTagSerializer.DeserializeAsync<CompoundTag>(dataStream);
    }

    public CompoundTag RootTag => this.GetRootTag().GetAwaiter().GetResult();

    public DateTime LastModified { get; }

    public int RegionChunkX => this.RegionIndex % 32;
    public int RegionChunkZ => (this.RegionIndex - this.RegionChunkX) / 32;

    public int ChunkX => this.RegionX * 32 + this.RegionChunkX;
    public int ChunkZ => this.RegionZ * 32 + this.RegionChunkZ;
    
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string Flag => Offset == 0 && Length == 0 ? " [MISSING]" : "";
}