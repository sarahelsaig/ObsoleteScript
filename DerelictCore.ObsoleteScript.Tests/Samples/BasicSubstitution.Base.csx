public class OldService
{
    [Obsolete]
    [ObsoleteScript($"SCRIPT-HERE")]
    public void OldMethod() { }
    public void NewMethod() { }
}
