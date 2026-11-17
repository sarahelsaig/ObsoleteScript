using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class ObsoleteScriptAttribute : Attribute
{
    public string Script { get; set; }

    public ObsoleteScriptAttribute(string script) => Script = script;
}

