using DerelictCore.ObsoleteScript.Constants;

namespace TestProject1;

public class UnitTest1 : TestBase
{
    [Theory]
    [InlineData("--> {nameof(NewMethod)}")]
    [InlineData("{nameof(OldMethod)} --> {nameof(NewMethod)}")]
    public Task BasicSubstitution(string script) =>
        RunAsync([nameof(BasicSubstitution)], script);

    [Theory]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("3")]
    public Task OverloadSubstitution(string variant) =>
        RunAsync(
            [nameof(OverloadSubstitution), variant],
            ScriptElements.OverloadsPrefix + "{`1`: `{0}NewMethod1({1})`, `2`: `{0}NewMethod1({1}).NewMethod2({2})`}");
}
