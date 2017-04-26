# CSharp.Formatting
Tools for generating syntax highlighted C# documentation (written in F#)

## Project Status

Appveyor: ![Build Status](https://ci.appveyor.com/api/projects/status/github/TheInnerLight/CSharp.Formatting?branch=master&svg=true)

## Getting started

__1.__ Generate a C# script (.csx) file and include some code, e.g.

```csharp

using System;

Console.WriteLine("Hello World!");
```

__2.__ Generate html documentation using the script file and the `CSharp.Formatting` library.

C# Example

```csharp
using CSharp.Formatting;

var data = System.IO.File.ReadAllText(@"ExampleScript.csx");
var html = HtmlGenerator.htmlEncodeSource(data);
System.IO.File.WriteAllText("test.html", html)
```

F# example

```fsharp
open CSharp.Formatting

let data = System.IO.File.ReadAllText """ExampleScript.csx"""
let html = HtmlGenerator.htmlEncodeSource data
System.IO.File.WriteAllText("test.html", html)
```

__3.__ Review the result

```html
<html>
  <head>
    <link rel="stylesheet" type="text/css" href="highlight.css"/>
  </head>
  <body>
    <span class="keyword">using</span>&#160;<span class="other">System</span>;
    <br/>
    <br/>
    <span class="classIdentifier" title="System.Console">Console</span>.<span class="methodIdentifier" title="System.Console.WriteLine(string)">WriteLine</span>(<span class="literal">&quot;Hello&#160;World!&quot;</span>);
  </body>
</html>
```

---

## Usage

Include documentation within your `.csx` script file by using multiline comments of the form `/***` and `*/`:

```csharp
/***
This is documentation.
*/

Console.WriteLine("This is code.");
```

Standard multiline `/*`, `*/` and single line comments `//` will be higlighted like typical code comments in your IDE.

## Highlighting

The formatting that the syntax highlighting produces is controlled by a style sheet called `highlight.css`, here is an example:

```css
.documentation {
    font-family: Calibri;
    font-size: 20px;
}

body {
    background-color: white;
    color: black;
    font-family: Consolas,monaco,monospace;
    font-size: 15px;
}

.block {
    padding-left: 20px;
}

.keyword {
    color : blue;
}

.literal {
    color : OrangeRed;
}

.interfaceIdentifier {
    color : Orange;
    font-weight: bold;
}

.classIdentifier {
    color : DodgerBlue;
    font-weight: bold;
}

.structIdentifier {
    color : DodgerBlue;
    font-weight: bold;
}

.enumIdentifier {
    color : yellow;
    font-weight: bold;
}

.varIdentifier {
    color : blue;
}

.attribIdentifier {
    color : DarkViolet;
    font-weight: bold;
}

.methodIdentifier {

}

.slComment {
    color : green;
}
```

