using DerelictCore.ObsoleteScript;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace TestProject1;

public class UnitTest1
{
    private const string CodeToPrepend = """
using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class ObsoleteScriptAttribute : Attribute
{
    public string Script { get; set; }

    public ObsoleteScriptAttribute(string script) => Script = script;
}


""";
    
    [Fact]
    public async Task Test1()
    {
        var script = "{nameof(OldMethod)} --> {nameof(NewMethod)}";
        
        var testCode = """
public class OldService
{
    [Obsolete]
    [ObsoleteScript($"SCRIPT-HERE")]
    public void OldMethod() { }
    public void NewMethod() { }
}

public class Program
{
    public void Run(OldService service)
    {
        {|CS0612:service.OldMethod()|};
    }
}
""";

        var fixedCode = """
public class OldService
{
    [Obsolete]
    [ObsoleteScript($"SCRIPT-HERE")]
    public void OldMethod() { }
    public void NewMethod() { }
}

public class Program
{
    public void Run(OldService service)
    {
        service.NewMethod();
    }
}
""";

        await RunAsync(testCode, fixedCode, script);
    }

    private static Task RunAsync(string testCode, string fixedCode, string script)
    {
        var test = new CSharpCodeFixTest<EmptyDiagnosticAnalyzer, ObsoleteScriptCodeFixProvider, DefaultVerifier>
        {
            TestCode = CodeToPrepend + testCode.Replace("SCRIPT-HERE", script),
            FixedCode = CodeToPrepend + fixedCode.Replace("SCRIPT-HERE", script),
        };

        return test.RunAsync();
    }
}
