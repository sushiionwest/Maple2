using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Context;
using Maple2.Model.Metadata;

namespace Maple2.Database.Storage;

public class FunctionCubeMetadataStorage : MetadataStorage<int, FunctionCubeMetadata> {
    protected Dictionary<int, FunctionCubeMetadata> FunctionCubeMetadata;

    public FunctionCubeMetadataStorage(MetadataContext context) : base(context, capacity: 500) {
        FunctionCubeMetadata = LoadAll();
    }

    public override void InvalidateCache() {
        base.InvalidateCache();
        FunctionCubeMetadata = LoadAll();
    }

    private Dictionary<int, FunctionCubeMetadata> LoadAll() {
        lock (Context) {
            return Context.FunctionCubeMetadata.ToDictionary(functionCube => functionCube.Id);
        }
    }

    public bool TryGet(int id, [NotNullWhen(true)] out FunctionCubeMetadata? functionCube) {
        return FunctionCubeMetadata.TryGetValue(id, out functionCube);
    }
}
