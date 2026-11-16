using DerelictCore.ObsoleteScript;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace TestProject1;

public class TestBase
{
    private static Task<string> GetTestCodeAsync(string name, string? suffix = null) =>
        File.ReadAllTextAsync(Path.Join("Samples", suffix == null ? $"{name}.csx" : $"{name}.{suffix}.csx"));

    protected static async Task RunAsync(string testName, string script)
    {
        var codeToPrepend = await GetTestCodeAsync("CodeToPrepend");
        var testCode = await GetTestCodeAsync(testName, "Input");
        var fixedCode = await GetTestCodeAsync(testName, "Expected");

        var test = new CSharpCodeFixTest<EmptyDiagnosticAnalyzer, ObsoleteScriptCodeFixProvider, DefaultVerifier>
        {
            TestCode = codeToPrepend + testCode.Replace("SCRIPT-HERE", script),
            FixedCode = codeToPrepend + fixedCode.Replace("SCRIPT-HERE", script),
        };

        await test.RunAsync();
    }
}
