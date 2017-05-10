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
    /// Gets a string without `start` characters from the start and `end'` from the end
    let withoutStartEnd start end' (str : string) =
        str.Substring(start, str.Length-start-end')

module TypeSymbol =
    let toDisplayString (typeSymbol : ITypeSymbol) =
        typeSymbol.ToDisplayString()

type Whitespace =
    |NormalWS
    |FixedWS

/// Functions for encoding code elements in HTML
module HtmlGenerator =

    let private encodeWhitespace whitespace str =
        match whitespace with
        |NormalWS -> str
        |FixedWS -> String.replace " " "\xA0" str

    /// encode a string as html with non-fixed whitespacing
    let htmlEncodeString whitespace (str : string) =
        str
        |> encodeWhitespace whitespace
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
        htmlEncodeString FixedWS (token.ToString())

    /// encode a C# keyword as html, grabbing type information if applicable
    let htmlEncodeKeyword syntaxItem =
        let typeInfoStr = 
            Tokens.findExpressionTypeInfo syntaxItem
            |> Option.map (htmlEncodeString FixedWS << TypeSymbol.toDisplayString)
        createSpan "keyword" typeInfoStr

    /// encode a C# identifier as html, grabbing type information if applicable
    let htmlEncodeIdentifier syntaxItem =
        let typeInfoStr = 
            Tokens.findExpressionTypeInfo syntaxItem 
            |> Option.map (htmlEncodeString FixedWS << TypeSymbol.toDisplayString)
        match syntaxItem with
        |MethodIdentifier mInfo -> createSpan "methodIdentifier" (Some <| mInfo.ToDisplayString())
        |PropertyIdentifier pInfo -> createSpan "propertyIdentifier" (Some <| pInfo.ToDisplayString())
        |NamespaceIdentifier nInfo -> createSpan "namespaceIdentifier" (Some <| "Namespace: "  + nInfo.ToDisplayString())
        |AttributeIdentifier _ -> createSpan "attribIdentifier" typeInfoStr
        |IdentifierName _ -> createSpan "identifierName" (Option.map (fun str -> syntaxItem.Token.ToString() + " : " + str) typeInfoStr) 
        |TypeIdentifier _ ->
            match syntaxItem with
            |InterfaceIdentifier _ -> createSpan "interfaceIdentifier" (typeInfoStr |> Option.map (fun type' -> "Interface: " + type'))
            |ClassIdentifier _ -> createSpan "classIdentifier" (typeInfoStr |> Option.map (fun type' -> "Class: " + type'))
            |StructIdentifier _ -> createSpan "structIdentifier" (typeInfoStr |> Option.map (fun type' -> "Struct: " + type'))
            |EnumIdentifier _ -> createSpan "enumIdentifier" (typeInfoStr |> Option.map (fun type' -> "Enum: " + type'))
            |_ -> createSpan "other" typeInfoStr
        

    /// encode C# trivia (comments/whitespace etc) as html
    let htmlEncodeTrivia (syntaxTrivia : SyntaxTrivia) =
        let formatFunction = 
            match syntaxTrivia with
            |MultiLineComment _ -> 
                match syntaxTrivia.ToFullString().StartsWith("/***") with
                |false -> createSpan "slComment" None << htmlEncodeString FixedWS
                |true -> createSpan "documentation" None << htmlEncodeString NormalWS << String.withoutStartEnd 4 2
            |SingleLineComment _ -> createSpan "slComment" None << htmlEncodeString FixedWS
            |ReferenceDirective _ -> fun _ -> ""
            |_ -> sprintf """%s""" << htmlEncodeString FixedWS
        let syntaxTrivia = syntaxTrivia.ToFullString()
        formatFunction syntaxTrivia
        
    /// encode an arbitrary C# syntax token as html
    let htmlEncodeSyntaxToken syntaxItem =
        let leadingTrivia = Seq.fold (fun acc st -> acc + htmlEncodeTrivia st) String.empty syntaxItem.Token.LeadingTrivia
        let trailingTrivia = Seq.fold (fun acc st -> acc + htmlEncodeTrivia st) String.empty syntaxItem.Token.TrailingTrivia
        let formatFunction =   
            match syntaxItem with
            |Keyword _ -> htmlEncodeKeyword syntaxItem
            |Identifier _ -> htmlEncodeIdentifier syntaxItem
            |StringLiteral _ -> createSpan "literal" None
            |CharacterLiteral _ -> createSpan "charLiteral" None
            |_ -> sprintf """%s"""
        leadingTrivia + formatFunction (htmlEncodeTokenText syntaxItem.Token) + trailingTrivia

    /// walk a supplied syntax node, encoding all tokens contained as html and concatenate the results
    let htmlEncodeNode (semanticModel :SemanticModel) (node : SyntaxNode)  =
        (node.DescendantTokens()) |> Seq.fold (fun html node -> html + (htmlEncodeSyntaxToken {Token = node; Model = semanticModel})) String.empty
        
    /// Encode some supplied source code as html
    [<CompiledName("HtmlEncodeSource")>]
    let htmlEncodeSource (source : string) =
        let compilation = CSharp.Scripting.CSharpScript.Create(source).GetCompilation()
        let syntaxTree = Seq.head compilation.SyntaxTrees
        let semanticModel = compilation.GetSemanticModel(syntaxTree, true)
        let content = htmlEncodeNode semanticModel (syntaxTree.GetRoot())
        sprintf """<html><head><link rel="stylesheet" type="text/css" href="highlight.css"/></head><body>%s</body></html>""" content
        


