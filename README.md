# ObsoleteScript

Code fix provider for Obsolete methods. This projects lets you create programmatic migration instructions that consuming code can use as Code Fix (light bulb icon).

This project adds a code fix provider that for `CS0612` and `CS0618` warnings, if the referenced member has the `[ObsoleteScript]` attribute. Currently supported:

- public and protected methods 
