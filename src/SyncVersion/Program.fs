open Cli

[<EntryPoint>]
let main argv = 
  let result = parseArguments argv

  printfn "%s" result.Version
  printfn "%s" result.Solution

  let projects = Solution.getProject result.Solution

  projects 
  |> Seq.collect (fun proj -> Project.getVersionFile proj)
  |> Seq.iter (fun source -> Versioning.update source result.Version)

  0
