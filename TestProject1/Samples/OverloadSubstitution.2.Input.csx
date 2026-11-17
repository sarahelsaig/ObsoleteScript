public class Program
{
    public void Run(OldService service)
    {
        {|CS0612:service.OldMethod("foo")|};
    }
}
