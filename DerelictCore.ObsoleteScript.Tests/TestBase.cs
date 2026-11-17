using DerelictCore.ObsoleteScript;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace TestProject1;

public class TestBase
{
    private static Task<string> GetTestCodeAsync(string name, string? suffix = null)
    {
        var path = Path.Join("Samples", suffix == null ? $"{name}.csx" : $"{name}.{suffix}.csx");
        return File.Exists(path) ? File.ReadAllTextAsync(path) : Task.FromResult(string.Empty);
    }

    protected static async Task RunAsync(string[] testName, string script = "")
    {
        var joinedTestName = string.Join('.', testName);
        var codeToPrepend =
            await GetTestCodeAsync("CodeToPrepend") +
            await GetTestCodeAsync(testName[0], "Base") +
            (testName.Length > 1 ? await GetTestCodeAsync(joinedTestName, "Base") : string.Empty);
        var testCode = await GetTestCodeAsync(joinedTestName, "Input");
        var fixedCode = await GetTestCodeAsync(joinedTestName, "Expected");

        var test = new CSharpCodeFixTest<EmptyDiagnosticAnalyzer, ObsoleteScriptCodeFixProvider, DefaultVerifier>
        {
            TestCode = (codeToPrepend + testCode).Replace("SCRIPT-HERE", script),
            FixedCode = (codeToPrepend + fixedCode).Replace("SCRIPT-HERE", script),
        };

        await test.RunAsync();
    }
}
