open CsvParser

//let data = [ "Foo"; string 1; "Bar" ]

//let csv = data |> String.concat ","

let line = """Foo,1,Bar,"Hello, World! From ""CSV Parse"".",2"""

let result = Parsing.parseLine line

printfn $"{result}"

type Foo = { Bar: string; Baz: int }

let record = RecordBuilder.createRecord<Foo> [ "Hello, World!"; "42" ]

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
