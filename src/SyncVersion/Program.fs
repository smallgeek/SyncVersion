open Argu
open System

type Arguments =
  | [<Mandatory>] Version of string
  | [<Mandatory>] Solution of string
with
  interface IArgParserTemplate with
    member x.Usage =
      match x with
      | Version _ -> "specify a version string."
      | Solution _ -> "specify a solution file path."

type Config = {
  Version: string
  Solution: string
}

let parseArguments (argv: string[]) =
  let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
  let parser = ArgumentParser.Create<Arguments>(errorHandler = errorHandler)
  let results = parser.Parse argv

  { Version = results.GetResult <@ Version @>; Solution = results.GetResult <@ Solution @> }

[<EntryPoint>]
let main argv = 
  let result = parseArguments argv

  printfn "%s" result.Version
  printfn "%s" result.Solution

  0
