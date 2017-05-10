namespace CSharp.Formatting

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open System.Web

type SyntaxItem = {Token : SyntaxToken; Model : SemanticModel}

module Tokens =
    /// Identifies a language keyword
    let (|Keyword|_|) item =
        match item.Token.IsKeyword() with
        |true -> Some item.Token
        |false -> None

    /// Identifies a semicolon
    let (|Semicolon|_|) item =
        match item.Token.Kind() with
        |SyntaxKind.SemicolonToken -> Some item
        |_ -> None

    /// Identifies a string literal
    let (|StringLiteral|_|) item =
        match item.Token.Kind() with
        |SyntaxKind.StringLiteralToken -> Some item
        |_ -> None

    /// Identifies a character literal
    let (|CharacterLiteral|_|) item =
        match item.Token.Kind() with
        |SyntaxKind.CharacterLiteralToken -> Some item
        |_ -> None

    /// Identifies an identifier
    let (|Identifier|_|) item =
        match item.Token.Kind() with
        |SyntaxKind.IdentifierToken -> Some item
        |_ -> None
    
    /// Identifies an identifier name
    let (|Name|_|) item =
        match item.Token.Kind() with
        |SyntaxKind.IdentifierName -> Some item
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

    /// Identifies a reference directive
    let (|ReferenceDirective|_|) (token : SyntaxTrivia) =
        match token.Kind() with
        |SyntaxKind.ReferenceDirectiveTrivia -> Some token
        |_ -> None

    let private findParentKinds (token : SyntaxToken) =
        maybe {
                let! parent = Option.ofObj token.Parent
                let! pParent = Option.ofObj parent.Parent
                let! pParent = if pParent.Kind() = SyntaxKind.QualifiedName then Option.ofObj (pParent.Parent) else Some (pParent)
                return pParent.Kind(), parent.Kind()
            }

    /// Finds the type information, using the supplied semanticModel, for a given syntax token
    let findExpressionTypeInfo syntaxItem =
        match findParentKinds syntaxItem.Token with
        |Some(SyntaxKind.VariableDeclaration, SyntaxKind.VariableDeclarator) -> // Find the type information associated with a named variable
            let pParent = syntaxItem.Token.Parent.Parent :?> CSharp.Syntax.VariableDeclarationSyntax
            Option.ofObj <| (syntaxItem.Model.GetTypeInfo(pParent.Type).Type)
        |Some(SyntaxKind.ParameterList, SyntaxKind.Parameter) -> // Find the type information associated with a named parameter
            let pParent = syntaxItem.Token.Parent :?> CSharp.Syntax.ParameterSyntax
            Option.ofObj <| (syntaxItem.Model.GetTypeInfo(pParent.Type).Type)
        |Some(SyntaxKind.ArrayType, _) -> // Find the type information associated with an array
            Option.ofObj <| (syntaxItem.Model.GetTypeInfo(syntaxItem.Token.Parent.Parent).Type)
        |Some(_, SyntaxKind.ClassDeclaration) -> // Find the type information associated with a class declaration
            let clDecl = syntaxItem.Token.Parent :?> CSharp.Syntax.ClassDeclarationSyntax
            Some (syntaxItem.Model.GetDeclaredSymbol(clDecl) :> ITypeSymbol)
        |_ -> // Try to find type information from parent or grandparent syntax nodes
            let parentType = Option.ofObj <| (syntaxItem.Model.GetTypeInfo(syntaxItem.Token.Parent).Type)
            match parentType with
            |Some t -> Some t
            |None -> Option.ofObj <| (syntaxItem.Model.GetTypeInfo(syntaxItem.Token.Parent.Parent).Type)

    /// Finds the type information for syntax highlighting purposes, this drills into the type of arrays
    let private tryFindHighlightTypeInfo syntaxItem =
        match findParentKinds syntaxItem.Token with
        |Some(SyntaxKind.ForEachStatement, _)
        |Some(SyntaxKind.CatchDeclaration, _)
        |Some(SyntaxKind.SimpleBaseType, _)
        |Some(SyntaxKind.Attribute, _) 
        |Some(SyntaxKind.Parameter, _) 
        |Some(SyntaxKind.SimpleMemberAccessExpression, _)
        |Some(SyntaxKind.VariableDeclaration, _)
        |Some(SyntaxKind.TypeArgumentList, _)
        |Some(SyntaxKind.ArrayType, _) -> Option.ofObj <| (syntaxItem.Model.GetTypeInfo(syntaxItem.Token.Parent).Type)
        |Some(_, SyntaxKind.ClassDeclaration) -> 
            let clDecl = syntaxItem.Token.Parent :?> CSharp.Syntax.ClassDeclarationSyntax
            Some (syntaxItem.Model.GetDeclaredSymbol(clDecl) :> ITypeSymbol)
        |Some(SyntaxKind.ObjectCreationExpression, SyntaxKind.GenericName)
        |Some(SyntaxKind.ObjectCreationExpression, SyntaxKind.IdentifierName) -> Option.ofObj <| (syntaxItem.Model.GetTypeInfo(syntaxItem.Token.Parent.Parent).Type)
        |_ -> None

    let (|MethodIdentifier|PropertyIdentifier|IdentifierName|NamespaceIdentifier|TypeIdentifier|AttributeIdentifier|) syntaxItem =
        match findParentKinds syntaxItem.Token with
        |Some(SyntaxKind.Attribute, SyntaxKind.IdentifierName)           -> AttributeIdentifier (syntaxItem.Token)
        |Some(SyntaxKind.TypeArgumentList, SyntaxKind.IdentifierName)
        |Some(SyntaxKind.VariableDeclaration, SyntaxKind.IdentifierName) -> TypeIdentifier (syntaxItem.Token)
        |Some(_, SyntaxKind.IdentifierName) -> 
            match Option.ofObj <| syntaxItem.Model.GetSymbolInfo(syntaxItem.Token.Parent).Symbol with
            |Some symbol ->
                match symbol with
                | :? IMethodSymbol as methodSymbol          -> MethodIdentifier methodSymbol
                | :? IPropertySymbol as propSymbol          -> PropertyIdentifier propSymbol
                | :? INamespaceSymbol as namespaceSymbol    -> NamespaceIdentifier namespaceSymbol
                | :? INamedTypeSymbol                       -> TypeIdentifier (syntaxItem.Token)
                |_ -> IdentifierName (syntaxItem.Token)
            |None ->
                IdentifierName (syntaxItem.Token)
        |_ -> TypeIdentifier (syntaxItem.Token)
                
       
    /// Seperates type identifiers by identifying whether they are interfaces, classes, enums, vars or something else
    let (|InterfaceIdentifier|ClassIdentifier|StructIdentifier|EnumIdentifier|Other|) syntaxItem =
        let identifierTypeInfo = tryFindHighlightTypeInfo syntaxItem
        /// Splits the type infos into appropriate groups, determining whether the type is an interface, a class or an enum
        let splitTypeInfosByTypeGroup (tI : ITypeSymbol)   =
            match tI.TypeKind with
            |TypeKind.Interface -> InterfaceIdentifier tI
            |TypeKind.Class -> ClassIdentifier tI
            |TypeKind.Struct -> StructIdentifier tI
            |TypeKind.Enum -> EnumIdentifier tI
            |_ -> Other
        // Use other if no other type is valid
        Option.fold (fun _ tI -> splitTypeInfosByTypeGroup tI) Other identifierTypeInfo


