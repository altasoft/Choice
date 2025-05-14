using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using VerifyXunit;
using Xunit;

namespace AltaSoft.Choice.Generator.SnapshotTests;

public class ChoiceGeneratorTest
{

    [Fact]
    public Task ChoiceTypeShouldGenerateAllMethodsAndCompileCorrectly()
    {
        const string source =
            """
            using System;
            using System.Xml;
            using System.Xml.Schema;
            using System.Xml.Serialization;
            using AltaSoft.Choice;
            using TestNamespace.OtherNamespace;
                              
            namespace TestNamespace
            {
                [Choice]
                public sealed partial class Authorisation1Choice
                {
                    /// <summary>
                    /// <para>Specifies the authorisation, in a coded form.</para>
                    /// </summary>
                    [XmlElement("Cd")]
                                     
                    public partial Authorisation1Code? Code { get; set; }
                                       
                    /// <summary>
                    /// <para>Specifies the authorisation, in a free text form.</para>
                    /// </summary>
                    [XmlElement("Prtry")]
                                     
                    public partial string? Proprietary { get; set; }
                                  
                }
            }
                           
            namespace TestNamespace.OtherNamespace
            {
                public enum Authorisation1Code
                {
                    One,
                    Two
                }
            }
            """;

        return TestHelper.Verify(source, (_, x, _) =>
        {
            Assert.Single(x);
        });
    }

    [Fact]
    public Task ChoiceTypeShouldNotGenerateImplicitMethodsAndCompileCorrectly()
    {
        const string source =
            """
            using System;
            using System.Xml;
            using System.Xml.Schema;
            using System.Xml.Serialization;
            using AltaSoft.Choice;
            using TestNamespace.OtherNamespace;
                              
            namespace TestNamespace
            {
                [Choice]
                public sealed partial class Authorisation1Choice
                {
                                  
                    /// <summary>
                    /// <para>Specifies the authorisation, in a coded form.</para>
                    /// </summary>
                    [XmlElement("Cd")]
                                     
                    public partial string? Code { get; set; }
                                       
                    /// <summary>
                    /// <para>Specifies the authorisation, in a free text form.</para>
                    /// </summary>
                    [XmlElement("Prtry")]
                                     
                    public partial Authorisation1Code? Proprietary { get; set; }
                }
            }
            
            namespace TestNamespace.OtherNamespace
            {
                public enum Authorisation1Code
                {
                    One,
                    Two
                }
            }
            """;

        return TestHelper.Verify(source, (_, x, _) =>
        {
            Assert.Single(x);
        });
    }

    public static class TestHelper
    {
        internal static Task Verify(string source, Action<ImmutableArray<Diagnostic>, List<string>, GeneratorDriver>? additionalChecks = null)
        {
            List<Assembly> assemblies = [typeof(XmlElementAttribute).Assembly, typeof(JsonSerializer).Assembly];
            var (diagnostics, output, driver) = TestHelpers.GetGeneratedOutput<AltaSoft.Choice.Generator.ChoiceGenerator>(source, assemblies);

            Assert.Empty(diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error));
            additionalChecks?.Invoke(diagnostics, output, driver);

            return Verifier.Verify(driver).UseDirectory("Snapshots");
        }
    }
}
