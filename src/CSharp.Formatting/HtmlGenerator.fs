namespace CSharp.Formatting

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open System.Web
open Tokens

/// Additional string functions
module String =
    /// The empty string
    let empty =
        System.String.Empty
    /// Replace occurences of 'oldStr' with 'newStr' in the supplied string
    let replace (oldStr : string) (newStr : string) (str : string) =
        str.Replace (oldStr, newStr)

/// Functions for encoding code elements in HTML
module HtmlGenerator =
    /// encode a string as html with fixed whitespacing
    let htmlEncodeString (str : string) =
        str
        |> String.replace " " "\xA0"
        |> System.Web.HttpUtility.HtmlEncode
        |> String.replace "\n" "<br/>"

    /// create an HTML tag with supplied tag name, tag class, optional title and content
    let createTag tagName tagClass titleOption content =
        match titleOption with
        |Some title -> sprintf """<%s class="%s" title="%s">%s</%s>""" tagName tagClass title content tagName
        |None -> sprintf """<%s class="%s">%s</%s>""" tagName tagClass content tagName

    /// create an HTML span tag
    let createSpan = createTag "span"

    /// create an HTML div tag
    let createDiv = createTag "div"

    /// html encode the string representation of a syntax token
    let htmlEncodeTokenText (token : SyntaxToken) =
        htmlEncodeString (token.ToString())

    /// encode a C# keyword as html, grabbing type information if applicable
    let htmlEncodeKeyword (semanticModel :SemanticModel) (token : SyntaxToken) =
        let typeInfoStr = 
            Tokens.findExpressionTypeInfo semanticModel token 
            |> Option.map (fun ti -> htmlEncodeString <| ti.ToDisplayString())
        createSpan "keyword" typeInfoStr

    /// encode a C# identifier as html, grabbing type information if applicable
    let htmlEncodeIdentifier (semanticModel :SemanticModel) (token : SyntaxToken) =
        let typeInfoStr = 
            Tokens.findExpressionTypeInfo semanticModel token 
            |> Option.map (fun ti -> htmlEncodeString <| ti.ToDisplayString())
        match (token, semanticModel) with
        |InterfaceIdentifier _ -> createSpan "interfaceIdentifier" typeInfoStr
        |ClassIdentifier _ -> createSpan "classIdentifier" typeInfoStr
        |EnumIdentifier _ -> createSpan "enumIdentifier" typeInfoStr
        |VarIdentifier _ -> createSpan "varIdentifier" typeInfoStr
        |MethodIdentifier mInfo -> createSpan "methodIdentifier" (Some <| mInfo.ToDisplayString())
        |LocalVar _ -> createSpan "localVar" (Option.map (fun str -> token.ToString() + " : " + str) typeInfoStr) 
        |_ -> createSpan "other" typeInfoStr

    /// encode C# trivia (comments/whitespace etc) as html
    let htmlEncodeTrivia (syntaxTrivia : SyntaxTrivia) =
        let formatFunction = 
            match syntaxTrivia with
            |SingleLineComment _ -> createSpan "slComment" None
            |MultiLineComment _ -> createSpan "slComment" None
            |_ -> sprintf """%s"""
        formatFunction (htmlEncodeString <| syntaxTrivia.ToFullString())
        
    /// encode an arbitrary C# syntax token as html
    let htmlEncodeSyntaxToken (semanticModel :SemanticModel) (token : SyntaxToken) =
        let leadingTrivia = Seq.fold (fun acc st -> acc + htmlEncodeTrivia st) String.empty token.LeadingTrivia
        let trailingTrivia = Seq.fold (fun acc st -> acc + htmlEncodeTrivia st) String.empty token.TrailingTrivia
        let formatFunction =   
            match token with
            |Keyword _ -> htmlEncodeKeyword semanticModel token
            |Identifier _ -> htmlEncodeIdentifier semanticModel token
            |StringLiteral _ -> createSpan "literal" None
            |Name _ -> createSpan "test" None
            |_ -> sprintf """%s"""
        leadingTrivia + formatFunction (htmlEncodeTokenText token) + trailingTrivia

    /// walk a supplied syntax node, encoding all tokens contained as html and concatenate the results
    let htmlEncodeNode (semanticModel :SemanticModel) (node : SyntaxNode)  =
        (node.DescendantTokens()) |> Seq.fold (fun html node -> html + (htmlEncodeSyntaxToken semanticModel node)) String.empty
        
    /// Encode some supplied source code as html
    let htmlEncodeSource (source : string) =
        let compilation = CSharp.Scripting.CSharpScript.Create(source).GetCompilation()
        let syntaxTree = Seq.head compilation.SyntaxTrees
        let semanticModel = compilation.GetSemanticModel(syntaxTree, true)
        let content = htmlEncodeNode semanticModel (syntaxTree.GetRoot())
        sprintf """<html><head><link rel="stylesheet" type="text/css" href="highlight.css"/></head><body>%s</body></html>""" content
        


