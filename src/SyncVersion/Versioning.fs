module Versioning
  open System.IO
  open System.Text.RegularExpressions
  open Cli
  open File

  type Replacing = {
    AssemblyVersionPattern : string
    AssemblyVersionFormat : string
    FileVersionPattern : string
    FileVersionFormat : string
  }

  let csReplacing = {
    AssemblyVersionPattern = "^\[assembly: AssemblyVersion\(\"(?<version>.*?)\"\)\]$"
    AssemblyVersionFormat = "[assembly: AssemblyVersion(\"{0}\")]"
    FileVersionPattern = "^\[assembly: AssemblyFileVersion\(\"(?<version>.*?)\"\)\]$"
    FileVersionFormat = "[assembly: AssemblyFileVersion(\"{0}\")]"
  }

  let fsReplacing = {
    AssemblyVersionPattern = "^\[\<assembly: AssemblyVersion\(\"(?<version>.*?)\"\)\>\]$"
    AssemblyVersionFormat = "[<assembly: AssemblyVersion(\"{0}\")>]"
    FileVersionPattern = "^\[\<assembly: AssemblyFileVersion\(\"(?<version>.*?)\"\)\>\]$"
    FileVersionFormat = "[<assembly: AssemblyFileVersion(\"{0}\")>]"
  }

  let vbReplacing = {
    AssemblyVersionPattern = "^\<Assembly: AssemblyVersion\(\"(?<version>.*?)\"\)\>$"
    AssemblyVersionFormat = "<Assembly: AssemblyVersion(\"{0}\")>"
    FileVersionPattern = "^\<Assembly: AssemblyFileVersion\(\"(?<version>.*?)\"\)\>$"
    FileVersionFormat = "<Assembly: AssemblyFileVersion(\"{0}\")>"
  }

  let private updateCore(sourceFile: string) (version: Version) (replacing: Replacing) =
    let regexMatch s pattern (version: string) format =
      let m = Regex.Match(s, pattern)
      if m.Success then
        System.String.Format(format, version)
      else
        s

    let lines = File.ReadAllLines(sourceFile)
    let newLines =
      lines 
      |> Seq.map (fun s -> regexMatch s replacing.AssemblyVersionPattern version.Assembly replacing.AssemblyVersionFormat)
      |> Seq.map (fun s -> regexMatch s replacing.FileVersionPattern version.File replacing.FileVersionFormat)
    
    File.WriteAllLines(sourceFile, newLines)

    ()

  let update(sourceFile: SourceFile) (version: Version) =
    
    match sourceFile with
    | Assembly path ->
        let ext = Path.GetExtension(path).ToLowerInvariant()
        match ext with
        | ".cs" -> updateCore path version csReplacing
        | ".fs" -> updateCore path version fsReplacing
        | ".vb" -> updateCore path version vbReplacing
        | _ -> ()

    | Manifest path -> ()
    | PList path -> ()
    

