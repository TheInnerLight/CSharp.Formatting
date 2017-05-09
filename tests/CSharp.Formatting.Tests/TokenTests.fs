namespace CSharp.Formatting.Tests

open FsCheck
open FsCheck.Xunit
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open CSharp.Formatting
open CSharp.Formatting.Tokens

type TokenGenerator() =
    static member SyntaxKind =
        Gen.elements (System.Enum.GetValues(typeof<SyntaxKind>) |> Seq.cast<SyntaxKind>)
        |> Gen.suchThat (SyntaxFacts.IsAnyToken)
        |> Arb.fromGen

type ``Token Tests``() =
    [<Property(Arbitrary = [|typeof<TokenGenerator>|])>]
    member __.``Given arbitrary token, |Keyword| pattern matches for keywords but not for other tokens`` (kind : SyntaxKind) =
        let synToken = SyntaxFactory.Token(kind)
        let synItem = {Token = synToken; Model = Unchecked.defaultof<_>}
        let isKeyword = function Keyword _ -> true | _ -> false
        isKeyword synItem = synToken.IsKeyword()

    [<Property(Arbitrary = [|typeof<TokenGenerator>|])>]
    member __.``Given arbitrary token, |Semicolon| pattern matches for semicolons but not for other tokens`` (kind : SyntaxKind) =
        let synToken = SyntaxFactory.Token(kind)
        let synItem = {Token = synToken; Model = Unchecked.defaultof<_>}
        let isSemicolon = function Semicolon _ -> true | _ -> false
        isSemicolon synItem = (kind = SyntaxKind.SemicolonToken)

    [<Property(Arbitrary = [|typeof<TokenGenerator>|])>]
    member __.``Given arbitrary token, |Identifier| pattern matches for identifier tokens but not for other tokens`` (kind : SyntaxKind) =
        let synToken = SyntaxFactory.Token(kind)
        let synItem = {Token = synToken; Model = Unchecked.defaultof<_>}
        let isIdentifier = function Identifier _ -> true | _ -> false
        isIdentifier synItem = (kind = SyntaxKind.IdentifierToken)

    [<Property(Arbitrary = [|typeof<TokenGenerator>|])>]
    member __.``Given arbitrary token, |Name| pattern matches for identifier names but not for other tokens`` (kind : SyntaxKind) =
        let synToken = SyntaxFactory.Token(kind)
        let synItem = {Token = synToken; Model = Unchecked.defaultof<_>}
        let isIdentifierName = function Name _ -> true | _ -> false
        isIdentifierName synItem = (kind = SyntaxKind.IdentifierName)

    [<Property(Arbitrary = [|typeof<TokenGenerator>|])>]
    member __.``Given arbitrary token, |StringLiteral| pattern matches for string literals but not for other tokens`` (kind : SyntaxKind) =
        let synToken = SyntaxFactory.Token(kind)
        let synItem = {Token = synToken; Model = Unchecked.defaultof<_>}
        let isStringLiteral = function StringLiteral _ -> true | _ -> false
        isStringLiteral synItem = (kind = SyntaxKind.StringLiteralToken)

    [<Property(Arbitrary = [|typeof<TokenGenerator>|])>]
    member __.``Given arbitrary token, |CharacterLiteral| pattern matches for character literals but not for other tokens`` (kind : SyntaxKind) =
        let synToken = SyntaxFactory.Token(kind)
        let synItem = {Token = synToken; Model = Unchecked.defaultof<_>}
        let isCharLiteral = function CharacterLiteral _ -> true | _ -> false
        isCharLiteral synItem = (kind = SyntaxKind.CharacterLiteralToken)

