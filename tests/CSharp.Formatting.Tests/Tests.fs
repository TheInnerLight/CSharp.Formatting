namespace CSharp.Formatting.Tests

open FsCheck
open FsCheck.Xunit
open CSharp.Formatting

type ``Html Generator Tests``() =
    [<Property>]
    member __.``Given a tag name, tag class and empty content a matching html tag is created`` (tagName : NonEmptyString, tagClass : NonEmptyString) =
        let html = HtmlGenerator.createTag tagName.Get tagClass.Get None (System.String.Empty)
        html = sprintf """<%s class="%s"></%s>""" tagName.Get tagClass.Get tagName.Get

    [<Property>]
    member __.``Given a tag name, tag class and content a matching html tag is created`` (tagName : NonEmptyString, tagClass : NonEmptyString, content : NonNull<string>) =
        let html = HtmlGenerator.createTag tagName.Get tagClass.Get None (content.Get)
        html = sprintf """<%s class="%s">%s</%s>""" tagName.Get tagClass.Get content.Get tagName.Get

    [<Property>]
    member __.``Given a tag name, tag class, title and content a matching html tag is created`` (tagName : NonEmptyString, tagClass : NonEmptyString, tagTitle : NonEmptyString, content : NonNull<string>) =
        let html = HtmlGenerator.createTag tagName.Get tagClass.Get (Some tagTitle.Get) (content.Get)
        html = sprintf """<%s class="%s" title="%s">%s</%s>""" tagName.Get tagClass.Get tagTitle.Get content.Get tagName.Get