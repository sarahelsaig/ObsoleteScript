public class Program
{
    public void Run(OldService service)
    {
        {|CS0618:service.OldMethod()|};
    }
}
