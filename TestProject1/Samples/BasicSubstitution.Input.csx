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
