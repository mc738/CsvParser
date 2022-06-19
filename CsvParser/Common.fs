namespace CsvParser

[<AutoOpen>]
module Common =

    open System
    
    /// An attribute to specify a specific format for DateTime and Guid fields in CSV.
    type CsvValueFormatAttribute(format: string) =

        inherit Attribute()

        member att.Format = format

