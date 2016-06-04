namespace CSharp.Formatting

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open System.Web

module Tokens =
    /// Identifies a language keyword
    let (|Keyword|_|) (token : SyntaxToken) =
        match token.IsKeyword() with
        |true -> Some token
        |false -> None

    /// Identifies a semicolon
    let (|Semicolon|_|) (token : SyntaxToken) =
        match token.Kind() with
        |SyntaxKind.SemicolonToken -> Some token
        |_ -> None

    /// Identifies a string literal
    let (|StringLiteral|_|) (token : SyntaxToken) =
        match token.Kind() with
        |SyntaxKind.StringLiteralToken
        |SyntaxKind.CharacterLiteralToken -> Some token
        |_ -> None

    /// Identifies an identifier
    let (|Identifier|_|) (token : SyntaxToken) =
        match token.Kind() with
        |SyntaxKind.IdentifierToken -> Some token
        |_ -> None
    
    /// Identifies an identifier name
    let (|Name|_|) (token : SyntaxToken) =
        match token.Kind() with
        |SyntaxKind.IdentifierName -> Some token
        |_ -> None

    /// Identifies a single line comment
    let (|SingleLineComment|_|) (token : SyntaxTrivia) =
        match token.Kind() with
        |SyntaxKind.SingleLineCommentTrivia -> Some token
        |_ -> None

    /// Identifies a multi-line comment
    let (|MultiLineComment|_|) (token : SyntaxTrivia) =
        match token.Kind() with
        |SyntaxKind.MultiLineCommentTrivia -> Some token
        |_ -> None

    let private findParentKinds (token : SyntaxToken) =
        maybe {
                let! parent = Option.ofObj token.Parent
                let! pParent = Option.ofObj parent.Parent
                return pParent.Kind(), parent.Kind()
            }

    /// Finds the type information, using the supplied semanticModel, for a given syntax token
    let findExpressionTypeInfo (semanticModel : SemanticModel) (token : SyntaxToken) =
        match findParentKinds token with
        |Some(SyntaxKind.VariableDeclaration, SyntaxKind.VariableDeclarator) -> // Find the type information associated with a named variable
            let pParent = token.Parent.Parent :?> CSharp.Syntax.VariableDeclarationSyntax
            Option.ofObj <| (semanticModel.GetTypeInfo(pParent.Type).Type)
        |Some(SyntaxKind.ParameterList, SyntaxKind.Parameter) -> // Find the type information associated with a named parameter
            let pParent = token.Parent :?> CSharp.Syntax.ParameterSyntax
            Option.ofObj <| (semanticModel.GetTypeInfo(pParent.Type).Type)
        |Some(SyntaxKind.ArrayType, _) -> // Find the type information associated with an array
            Option.ofObj <| (semanticModel.GetTypeInfo(token.Parent.Parent).Type)
        |_ -> // Try to find type information from parent or grandparent syntax nodes
            let parentType = Option.ofObj <| (semanticModel.GetTypeInfo(token.Parent).Type)
            match parentType with
            |Some t -> Some t
            |None -> Option.ofObj <| (semanticModel.GetTypeInfo(token.Parent.Parent).Type)

    /// Finds the type information for syntax highlighting purposes, this drills into the type of arrays
    let private tryFindHighlightTypeInfo (semanticModel : SemanticModel) (token : SyntaxToken) =
        match findParentKinds token with
        |Some(SyntaxKind.Attribute, _) 
        |Some(SyntaxKind.Parameter, _) 
        |Some(SyntaxKind.SimpleMemberAccessExpression, _)
        |Some(SyntaxKind.VariableDeclaration, _)
        |Some(SyntaxKind.TypeArgumentList, _)
        |Some(SyntaxKind.ArrayType, _) -> Option.ofObj <| (semanticModel.GetTypeInfo(token.Parent).Type)
        |Some(SyntaxKind.ObjectCreationExpression, SyntaxKind.GenericName)
        |Some(SyntaxKind.ObjectCreationExpression, SyntaxKind.IdentifierName) -> Option.ofObj <| (semanticModel.GetTypeInfo(token.Parent.Parent).Type)
        |_ -> None
        
    /// Seperates type identifiers by identifying whether they are interfaces, classes, enums, vars or something else
    let (|InterfaceIdentifier|ClassIdentifier|EnumIdentifier|VarIdentifier|Other|) (token : SyntaxToken, semanticModel : SemanticModel) =
        let identifierTypeInfo = tryFindHighlightTypeInfo semanticModel token
        /// Splits the type infos into appropriate groups, determining whether the type is an interface, a class or an enum
        let splitTypeInfosByTypeGroup (tI : ITypeSymbol)   =
            match tI.TypeKind with
            |TypeKind.Interface | TypeKind.Class | TypeKind.Enum when token.ValueText.ToUpperInvariant() = "VAR" ->  VarIdentifier tI
            |TypeKind.Interface -> InterfaceIdentifier tI
            |TypeKind.Class -> ClassIdentifier tI
            |TypeKind.Enum -> EnumIdentifier tI
            |_ -> Other
        // Use other if no other type is valid
        Option.fold (fun _ tI -> splitTypeInfosByTypeGroup tI) Other identifierTypeInfo

    /// Identifies a local variable
    let (|LocalVar|_|) (token : SyntaxToken, semanticModel : SemanticModel) =
        Option.ofObj <| semanticModel.GetDeclaredSymbol(token.Parent)
        |> Option.filter (fun tI -> tI.Kind = SymbolKind.Local)

    /// Identifies a method
    let (|MethodIdentifier|_|) (token : SyntaxToken, semanticModel : SemanticModel) =
        let symbol =  Option.ofObj <| semanticModel.GetSymbolInfo(token.Parent).Symbol
        symbol |> Option.bind (function
            | :? IMethodSymbol as symbol -> Some symbol
            |_ -> None)


