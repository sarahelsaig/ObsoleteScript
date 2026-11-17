public partial class OldService
{
    [Obsolete]
    [ObsoleteScript("SCRIPT-HERE")]
    public void OldMethod(string foo, string bar = "") { }
    public OldService NewMethod1(string foo) { return this; }
    public OldService NewMethod2(string bar) { return this; }
}
