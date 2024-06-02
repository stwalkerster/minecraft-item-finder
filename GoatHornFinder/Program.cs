namespace GoatHornFinder;

using Raspite.Serializer.Tags;

class Program
{
    static void Main(string[] args)
    {
        var ignoreTags = new[] { "x", "y", "z", "id", "keepPacked" };
        var ignoreEntities = new[]
        {
            "minecraft:jukebox",
            "minecraft:lectern",
            "minecraft:end_portal",
            "minecraft:decorated_pot", // FIXME: this probably needs to be checked later, but it's NBT is "item" instead of "Items" 
        };
        
        foreach (var regionFile in args)
        {
            var region = new AnvilFile(regionFile);
            foreach (var chunk in region.GetChunks())
            {
                try
                {
                    var blockEntities = chunk.RootTag.First<Raspite.Serializer.Tags.ListTag>("block_entities");

                    foreach (CompoundTag entity in blockEntities.Children)
                    {
                        var id = entity.First<Raspite.Serializer.Tags.StringTag>("id").Value;
                        if (ignoreEntities.Contains(id))
                        {
                            continue;
                        }
                        
                        var x = entity.First<Raspite.Serializer.Tags.IntegerTag>("x").Value;
                        var y = entity.First<Raspite.Serializer.Tags.IntegerTag>("y").Value;
                        var z = entity.First<Raspite.Serializer.Tags.IntegerTag>("z").Value;
                        
                        Console.WriteLine($"({x}, {y}, {z}): {id}");
                        
                        // Dump tags
                        foreach (var se in entity.Children.Where(x => !ignoreTags.Contains(x.Name)).Select(x => "  - " + x.Name))
                        {
                            Console.WriteLine(se);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}