namespace CSharp.Formatting

type MaybeBuilder() =
    member this.Bind(x, f) = Option.bind f x
    member this.Return x = Some x
    member this.ReturnFrom x : 'a Option = x

[<AutoOpen>]
module Maybe =
    let maybe = MaybeBuilder()
