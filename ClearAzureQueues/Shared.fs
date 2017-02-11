namespace System

[<AutoOpen>]
module Shared =

    let (|Null|NotNull|) x =
        if obj.ReferenceEquals(null, x) then Null
        else NotNull x

    let ifNullOr x fn = function
        | Null -> x
        | NotNull x -> fn x

    let ifNull fn = ifNullOr () fn

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Option =

        let ofNull = function
            | Null -> None
            | NotNull x -> Some x

        let iterNone action = function
            | None -> action()
            | Some _ -> ()

        let getValueOr x = function
            | None -> x
            | Some x -> x