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
    public Task ChoiceTypeShouldGenerateDocumentationCorrectly_ForArrayInChoice()
    {
        const string source =
            """
            using System;
            using System.Xml;
            using System.Xml.Schema;
            using System.Xml.Serialization;
            using AltaSoft.Choice;
            
            namespace TestNamespace
            {
                [Choice]
                public sealed partial class ArrayInTypeChoice
                {
                    public partial string? StringChoice { get; set; }
            
                    public partial AccountId[]? Accounts { get; set; }
                }
            
                public sealed record AccountId(int Id);
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

    [Fact]
    public Task ChoiceTypeShouldGenerateWithXmlTagNamespace()
    {
        const string source =
            """
            using System;
            using System.Xml;
            using System.Xml.Schema;
            using System.Xml.Serialization;
            using AltaSoft.Choice;

            namespace TestNamespace
            {
                [Choice]
                public sealed partial class XmlNamespaceChoice
                {
                    /// <summary>
                    /// <para>Specifies the code with namespace.</para>
                    /// </summary>
                    [XmlTag("Cd", Namespace = "urn:test:code")]
                    public partial string? Code { get; set; }

                    /// <summary>
                    /// <para>Specifies the proprietary value with namespace.</para>
                    /// </summary>
                    [XmlTag("Prtry", Namespace = "urn:test:proprietary")]
                    public partial string? Proprietary { get; set; }
                }
            }
            """;

        return TestHelper.Verify(source, (_, x, _) =>
        {
            Assert.Single(x);
        });
    }

    [Fact]
    public Task ChoiceTypeShouldGenerateWithMixedXmlTagAndXmlElement()
    {
        const string source =
            """
            using System;
            using System.Xml;
            using System.Xml.Schema;
            using System.Xml.Serialization;
            using AltaSoft.Choice;

            namespace TestNamespace
            {
                [Choice]
                public sealed partial class MixedAttributeChoice
                {
                    /// <summary>
                    /// <para>Code with XmlTag and namespace.</para>
                    /// </summary>
                    [XmlTag("Cd", Namespace = "urn:test:code")]
                    public partial string? Code { get; set; }

                    /// <summary>
                    /// <para>Proprietary with standard XmlElement.</para>
                    /// </summary>
                    [XmlElement("Prtry")]
                    public partial string? Proprietary { get; set; }

                    /// <summary>
                    /// <para>Amount with XmlTag but no namespace.</para>
                    /// </summary>
                    [XmlTag("Amt")]
                    public partial decimal? Amount { get; set; }
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
            var (diagnostics, output, driver) = TestHelpers.GetGeneratedOutput<ChoiceGenerator>(source, assemblies);

            Assert.Empty(diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error));
            additionalChecks?.Invoke(diagnostics, output, driver);

            return Verifier.Verify(driver).UseDirectory("Snapshots");
        }
    }
}
