namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("CSharp.Formatting")>]
[<assembly: AssemblyProductAttribute("CSharp.Formatting")>]
[<assembly: AssemblyDescriptionAttribute("Tools for generating C# web documentation")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
    let [<Literal>] InformationalVersion = "1.0"
