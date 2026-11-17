public class OldService
{
    [Obsolete("This will be removed.")]
    [ObsoleteScript($"SCRIPT-HERE")]
    public void OldMethod() { }
    public void NewMethod() { }
}
