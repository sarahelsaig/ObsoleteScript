public partial class OldService
{
    public void Run()
    {
        {|CS0612:OldMethod("foo", "bar")|};
    }
}
