# ObsoleteScript

Code fix provider for Obsolete methods. This projects lets you create programmatic migration instructions that consuming code can use as Code Fix (light bulb icon).

This project adds a code fix provider that for `CS0612` and `CS0618` warnings, if the referenced member has the `[ObsoleteScript]` attribute. Current supported attribute usage:

- public and protected methods 

## Features

To use the features of this analyzer, add `[ObsoleteScript("SCRIPT-HERE")]` to the member with the `[Obsolete]` attribute. Possible values:

- Substitute name: `"${nameof(OldName)} --> ${nameof(NewName)}"` or `"-->${nameof(NewName)}"` for short.
  The most basic expression. This special case lets you do very basic substitution. Use this in conjunction with `[Obsolete($"Use {nameof(NewName)} instead.")]` 
- Overload-count: ``"overloads: {`1`: `{0}NewMethod1({1})`, `2`: `{0}NewMethod1({1}).NewMethod2({2})`}"``
  When the script starts with "overloads:", the rest is decoded as JSON (except the backtick characters are first converted to quotes, so you can easily use strings without entering escape sequence hell). The key is the number of parameters, the value is a template string. The first parameter of the template is the part of the expression preceding the obsolete member's name. The rest are the arguments used. 
