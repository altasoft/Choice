using System.Runtime.CompilerServices;
using DiffEngine;
using VerifyTests;

namespace AltaSoft.Choice.Generator.SnapshotTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        DiffTools.UseOrder(DiffTool.VisualStudioCode, DiffTool.VisualStudio, DiffTool.AraxisMerge, DiffTool.BeyondCompare);
        VerifySourceGenerators.Initialize();
    }
}
