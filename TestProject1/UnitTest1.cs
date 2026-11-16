namespace TestProject1;

public class UnitTest1 : TestBase
{
    [Theory]
    [InlineData("--> {nameof(NewMethod)}")]
    [InlineData("{nameof(OldMethod)} --> {nameof(NewMethod)}")]
    public Task BasicSubstitution(string script)
    {
        return RunAsync(nameof(BasicSubstitution), script);
    }
}
