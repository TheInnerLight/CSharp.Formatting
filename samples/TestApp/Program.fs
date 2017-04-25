// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open CSharp.Formatting

[<EntryPoint>]
let main argv = 
    let data = System.IO.File.ReadAllText """ExampleScript.csx"""
    let html = HtmlGenerator.htmlEncodeSource data

    System.IO.File.WriteAllText("test.html", html)

    printfn "%A" html
    0 // return an integer exit code
