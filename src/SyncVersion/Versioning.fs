module Versioning
  open System.IO
  open System.Text.RegularExpressions
  open Cli

  let csAssemblyVersionPattern = "^\[assembly: AssemblyVersion\(\"(?<version>.*?)\"\)\]$"
  let csAssemblyVersionFormat = "[assembly: AssemblyVersion(\"{0}\")]"
  let csAssemblyFileVersionPattern = "^\[assembly: AssemblyFileVersion\(\"(?<version>.*?)\"\)\]$"
  let csAssemblyFileVersionFormat = "[assembly: AssemblyFileVersion(\"{0}\")]"

  let fsAssemblyVersionPattern = "^\[\<assembly: AssemblyVersion\(\"(?<version>.*?)\"\)\>\]$"
  let fsAssemblyVersionFormat = "[<assembly: AssemblyVersion(\"{0}\")>]"
  let fsAssemblyFileVersionPattern = "^\[\<assembly: AssemblyFileVersion\(\"(?<version>.*?)\"\)\>\]$"
  let fsAssemblyFileVersionFormat = "[<assembly: AssemblyFileVersion(\"{0}\")>]"

  let vbAssemblyVersionPattern = "^\<Assembly: AssemblyVersion\(\"(?<version>.*?)\"\)\>$"
  let vbAssemblyVersionFormat = "<Assembly: AssemblyVersion(\"{0}\")>"
  let vbAssemblyFileVersionPattern = "^\<Assembly: AssemblyFileVersion\(\"(?<version>.*?)\"\)\>$"
  let vbAssemblyFileVersionFormat = "<Assembly: AssemblyFileVersion(\"{0}\")>"

  let private updateCore(sourceFile: string) (version: Version) (pattern: string) (replaceFormat: string) (filePattern: string) (replaceFileFormat: string) =
    let regexMatch s pattern (version: string) format =
      let m = Regex.Match(s, pattern)
      if m.Success then
        System.String.Format(format, version)
      else
        s

    let lines = File.ReadAllLines(sourceFile)
    let newLines =
      lines 
      |> Seq.map (fun s -> regexMatch s pattern version.Assembly replaceFormat)
      |> Seq.map (fun s -> regexMatch s filePattern version.File replaceFormat)
    
    File.WriteAllLines(sourceFile, newLines)

    ()

  let update(sourceFile: string) (version: Version) =
    
    let update' = updateCore sourceFile version
    let ext = Path.GetExtension(sourceFile).ToLowerInvariant()
    
    match ext with
    | ".cs" -> update' csAssemblyVersionPattern csAssemblyVersionFormat csAssemblyFileVersionPattern csAssemblyFileVersionFormat
    | ".fs" -> update' fsAssemblyVersionPattern fsAssemblyVersionFormat fsAssemblyFileVersionPattern fsAssemblyFileVersionFormat
    | ".vb" -> update' vbAssemblyVersionPattern vbAssemblyVersionFormat vbAssemblyFileVersionPattern vbAssemblyFileVersionFormat
    | _ -> ()
