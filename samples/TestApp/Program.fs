// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open CSharp.Formatting

[<EntryPoint>]
let main argv = 
    let data = System.IO.File.ReadAllText """ExampleScript.csx"""
    let data2 = System.IO.File.ReadAllText """ExampleScript2.csx"""
    let data3 = System.IO.File.ReadAllText """ExampleScript3.csx"""
    let html = HtmlGenerator.htmlEncodeSource data
    let html2 = HtmlGenerator.htmlEncodeSource data2
    let html3 = HtmlGenerator.htmlEncodeSource data3

    System.IO.File.WriteAllText("test.html", html)
    System.IO.File.WriteAllText("test2.html", html2)
    System.IO.File.WriteAllText("test3.html", html3)

    //printfn "%A" html
    0 // return an integer exit code
