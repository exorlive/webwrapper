using System.Diagnostics.CodeAnalysis;
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

[assembly: SuppressMessage("Performance", "CA1822:Mark members as static")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles")]
[assembly: SuppressMessage("Style", "IDE0065:Misplaced using directive", Justification = "Third party library.", Scope = "namespace", Target = "~N:Microsoft.Shell")]
[assembly: SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "Third party library.", Scope = "member", Target = "~M:Microsoft.Shell.NativeMethods.CommandLineToArgvW(System.String)~System.String[]")]
