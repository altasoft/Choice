using System.Runtime.CompilerServices;
using VerifyTests;

namespace AltaSoft.Choice.Generator.SnapshotTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
    }
}
