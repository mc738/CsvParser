namespace CsvParser

module RecordBuilder =

    open System
    open System.Globalization
    open Microsoft.FSharp.Reflection

    module private TypeHelpers =
        let getName<'T> = typeof<'T>.FullName

        let typeName (t: Type) = t.FullName

        let boolName = getName<bool>

        let uByteName = getName<uint8>

        let uShortName = getName<uint16>

        let uIntName = getName<uint32>

        let uLongName = getName<uint64>

        let byteName = getName<byte>

        let shortName = getName<int16>

        let intName = getName<int>

        let longName = getName<int64>

        let floatName = getName<float>

        let doubleName = getName<double>

        let decimalName = getName<decimal>

        let charName = getName<char>

        let timestampName = getName<DateTime>

        let uuidName = getName<Guid>

        let stringName = getName<string>
            
    [<RequireQualifiedAccess>]
    type SupportedType =
        | Boolean
        | Byte
        | Char
        | Decimal
        | Double
        | Float
        | Int
        | Short
        | Long
        | String
        | DateTime
        | Guid
        
        static member FromName(name: String) =
            match name with
            | t when t = TypeHelpers.boolName -> SupportedType.Boolean
            | t when t = TypeHelpers.byteName -> SupportedType.Byte
            | t when t = TypeHelpers.charName -> SupportedType.Char
            | t when t = TypeHelpers.decimalName -> SupportedType.Decimal
            | t when t = TypeHelpers.doubleName -> SupportedType.Double
            | t when t = TypeHelpers.floatName -> SupportedType.Float
            | t when t = TypeHelpers.intName -> SupportedType.Int
            | t when t = TypeHelpers.shortName -> SupportedType.Short
            | t when t = TypeHelpers.longName -> SupportedType.Long
            | t when t = TypeHelpers.stringName -> SupportedType.String
            | t when t = TypeHelpers.timestampName -> SupportedType.DateTime
            | t when t = TypeHelpers.uuidName -> SupportedType.Guid
            | _ -> failwith $"Type `{name}` not supported."
    
    /// A helper function to try and get field value by index.
    let tryGetAtIndex (values: string array) (i: int) =
            match i >= 0 && i < values.Length with
            | true -> Some values.[i]
            | false -> None
    
    /// Create a record of type 'T from a list of strings
    let createRecord<'T> (values: string list) =
            
            // A helper function to get a field value by index.
            // Converts to an array for easier access by index,
            // however this could be skipped
            let getValue = values |> Array.ofList |> tryGetAtIndex

            // Get the generic type.
            let t = typeof<'T>

            // Create the values and box them.
            let values =
                // Get the properties from the type
                t.GetProperties()
                |> List.ofSeq
                // Use List.mapi to have access to the field index - to match up with the values list.
                |> List.mapi
                    (fun i pi ->
                        // Check if the property has a format attribute.
                        let format =
                            match Attribute.GetCustomAttribute(pi, typeof<CsvValueFormatAttribute>) with
                            | att when att <> null -> Some <| (att :?> CsvValueFormatAttribute).Format
                            | _ -> None

                        // Get the supported type.
                        let t = SupportedType.FromName(pi.PropertyType.FullName)

                        // Attempt to get the value,
                        // Then match on the supported type, parse and box.
                        match getValue i, t with
                        | Some v, SupportedType.Boolean -> bool.Parse v :> obj
                        | Some v, SupportedType.Byte -> Byte.Parse v :> obj
                        | Some v, SupportedType.Char -> v.[0] :> obj
                        | Some v, SupportedType.Decimal -> Decimal.Parse v :> obj
                        | Some v, SupportedType.Double -> Double.Parse v :> obj
                        | Some v, SupportedType.DateTime ->
                            match format with
                            | Some f -> DateTime.ParseExact(v, f, CultureInfo.InvariantCulture)
                            | None -> DateTime.Parse(v)
                            :> obj
                        | Some v, SupportedType.Float -> Double.Parse v :> obj
                        | Some v, SupportedType.Guid ->
                            match format with
                            | Some f -> Guid.ParseExact(v, f)
                            | None -> Guid.Parse(v)
                            :> obj
                        | Some v, SupportedType.Int -> Int32.Parse v :> obj
                        | Some v, SupportedType.Long -> Int64.Parse v :> obj
                        | Some v, SupportedType.Short -> Int16.Parse v :> obj
                        | Some v, SupportedType.String -> v :> obj
                        | None, _ -> failwith "Could not get value")

            // Create the record.
            // This will return an object.
            let o =
                FSharpValue.MakeRecord(t, values |> Array.ofList)

            // Downcast the newly created object back to type 'T
            o :?> 'T