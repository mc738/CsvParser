namespace CsvParser

module Parsing =

    open System    
    open System.IO    
    open System.Text
        
    /// A basic function to read all lines in a file,
    /// with catch all error handling.
    let loadFile (path: string) =
        try
            File.ReadLines path |> Ok
        with
        | exn -> Error exn.Message
        
    let inBounds (input: string) i = i >= 0 && i < input.Length

    let getChar (input: string) i =
        match inBounds input i with
        | true -> Some input.[i]
        | false -> None

    let readUntilChar (input: string) (c: Char) (start: int) (sb: StringBuilder) =
        let rec read i (sb: StringBuilder) =
            match getChar input i with
            // If the current char is ", check if the next one is. If some it is a delimited ".
            // In that case just add a " to the string builder.
            | Some r when r = '"' && getChar input (i + 1) = Some '"' -> read (i + 2) <| sb.Append('"')
            // If the current char is c, return the accumulated string and the current index.
            | Some r when r = c -> Some <| (sb.ToString(), i)
            // If the current index does return a character add it to the string builder
            // and index the index by one.
            | Some r -> read (i + 1) <| sb.Append(r)
            // If the current index does not return a character, return the accumulated string
            // and the next index. This will be out of bounds but will be handled later.
            | None -> Some <| (sb.ToString(), i + 1)
        
        read start sb
        
    let parseLine (input: string) =
        // Use a StringBuilder as a bit of an optimization. 
        let sb = StringBuilder()

        let rec readBlock (i, sb: StringBuilder, acc: string list) =

            match getChar input i with
            // If the block starts with ", it is an double quote enclosed block.
            // Read until an non delimited ".
            | Some c when c = '"' ->
                match readUntilChar input '"' (i + 1) sb with
                | Some (r, i) ->
                    // i + 2 to skip end " and ,
                    readBlock (i + 2, sb.Clear(), acc @ [ r ])
                | None -> acc
            // If not, simply read until ,
            | Some _ ->
                match readUntilChar input ',' i sb with
                // i + 1 to skip the ,
                | Some (r, i) -> readBlock (i + 1, sb.Clear(), acc @ [ r ])
                | None -> acc
            // If no character return, simply return the accumulated strings.
            // This is where the i + 1 in readUntilChar last branch is effectively handled. 
            | None -> acc

        readBlock (0, sb, [])