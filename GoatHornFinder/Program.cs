namespace GoatHornFinder;

using Raspite.Serializer.Tags;

class Program
{
    static void Main(string[] args)
    {
        var searchId = "minecraft:goat_horn";
        
        foreach (var regionFile in args)
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
}