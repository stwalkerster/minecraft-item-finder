namespace GoatHornFinder;

using Mono.Options;
using Raspite.Serializer.Tags;

class Program
{
    static void Main(string[] args)
    {
        var searchId = "minecraft:goat_horn";
        
        var isRegion = true;
        var options = new OptionSet
        {
            { "r|region", "Use region file parsing mode", x => isRegion = true },
            { "e|entity", "Use entity file parsing mode", x => isRegion = false },
            { "search=", "Specify the item type to look for", x => searchId = x }
        };

        var extra = options.Parse(args);

        if (extra.Count == 0)
        {
            var help = new[]
            {
                "Usage: GoatHornFinder [-r|-e] [--search ID] <file> [file...]",
                "",
                "   -r, --region : Use to indicate supplied MCA files are region files (./region/*.mca, default)",
                "   -e, --entity : Use to indicate supplied MCA files are entity files (./entities/*.mca)",
                "       --search : Change the searched-for item to the provided ID. (default 'minecraft:goat_horn')",
                "",
                "Specify one or more MCA files to search for the relevant item."
            };
            foreach (var h in help)
            {
                Console.WriteLine(h);
            }
            return;
        }
        
        if (isRegion)
        {
            SearchRegion(extra, searchId);
        }
        else
        {
            SearchEntities(extra, searchId);
        }
    }

    private static void SearchRegion(IEnumerable<string> files, string searchId)
    {
        foreach (var regionFile in files)
        {
            var region = new AnvilFile(regionFile);
            foreach (var chunk in region.GetChunks())
            {
                var blockEntities = chunk.RootTag.First<ListTag>("block_entities");

                foreach (var tag in blockEntities.Children)
                {
                    if (tag is not CompoundTag entity)
                    {
                        continue;
                    }

                    var id = entity.First<StringTag>("id").Value;
                    var x = entity.First<IntegerTag>("x").Value;
                    var y = entity.First<IntegerTag>("y").Value;
                    var z = entity.First<IntegerTag>("z").Value;

                    if (id == "minecraft:decorated_pot")
                    {
                        var item = entity.First<CompoundTag>("item");
                        var itemId = item.First<StringTag>("id").Value;
                        if (itemId == searchId)
                        {
                            var count = item.First<SignedByteTag>("Count").Value;
                            if (count > 0)
                            {
                                Console.WriteLine($"Found {searchId} in {id} at ({x}, {y}, {z})");
                            }
                        }
                    }

                    if (!entity.Children.Select(x => x.Name).Contains("Items"))
                    {
                        continue;
                    }
                            
                    var items = entity.First<ListTag>("Items");

                    foreach (var itemTag in items.Children)
                    {
                        if (itemTag is not CompoundTag item)
                        {
                            continue;
                        }

                        var itemId = item.First<StringTag>("id").Value;
                        if (itemId != searchId)
                        {
                            continue;
                        }

                        var count = item.First<SignedByteTag>("Count").Value;
                        if (count > 0)
                        {
                            Console.WriteLine($"Found {searchId} in {id} at ({x}, {y}, {z})");
                            break;
                        }
                    }
                }
            }
        }
    }
    
    private static void SearchEntities(IEnumerable<string> files, string searchId)
    {
        foreach (var regionFile in files)
        {
            var region = new AnvilFile(regionFile);
            foreach (var chunk in region.GetChunks())
            {
                var entities = chunk.RootTag.First<ListTag>("Entities");
                
                foreach (var tag in entities.Children)
                {
                    if (tag is not CompoundTag entity)
                    {
                        continue;
                    }
                
                    var id = entity.First<StringTag>("id").Value;

                    var pos = entity.First<ListTag>("Pos");
                    var x = ((DoubleTag)pos.Children[0]).Value;
                    var y = ((DoubleTag)pos.Children[1]).Value;
                    var z = ((DoubleTag)pos.Children[2]).Value;
                    
                    if (!entity.Children.Select(x => x.Name).Contains("Items"))
                    {
                        continue;
                    }
                            
                    var items = entity.First<ListTag>("Items");
                    
                    foreach (var itemTag in items.Children)
                    {
                        if (itemTag is not CompoundTag item)
                        {
                            continue;
                        }
                    
                        var itemId = item.First<StringTag>("id").Value;
                        if (itemId != searchId)
                        {
                            continue;
                        }
                    
                        var count = item.First<SignedByteTag>("Count").Value;
                        if (count > 0)
                        {
                            Console.WriteLine($"Found {searchId} in {id} at ({x}, {y}, {z})");
                            break;
                        }
                    }
                }
            }
        }
    }
}