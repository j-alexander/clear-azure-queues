namespace System

[<AutoOpen>]
module Shared =

    let (|Null|NotNull|) x =
        if obj.ReferenceEquals(null, x) then Null
        else NotNull x

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Option =

        let ofNull = function
            | Null -> None
            | NotNull x -> Some x

        let getValueOr x = function
            | None -> x
            | Some x -> x