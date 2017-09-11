module Versioning
  open System.IO
  open System.Text.RegularExpressions
  open System.Xml.Linq
  open System.Xml.XPath
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

  let private updateAssembly(sourceFile: string) (version: Version) (replacing: Replacing) =
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

  let private updateManifest(sourceFile: string) (version: Version) =
    let ns = "http://schemas.android.com/apk/res/android"

    let versionCodeAttributeName = XName.Get("versionCode", ns)
    let versionNameAttributeName = XName.Get("versionName", ns)

    let doc = XDocument.Load(sourceFile)

    let fileVersion = System.Version(version.File)
    let code = fileVersion.Major * 10000 + fileVersion.Minor * 100 + fileVersion.Build

    doc.Root.SetAttributeValue(versionCodeAttributeName, code)
    doc.Root.SetAttributeValue(versionNameAttributeName, version.Assembly)
    doc.Save(sourceFile)

  let update(sourceFile: SourceFile) (version: Version) =
    
    match sourceFile with
    | Assembly path ->
        let ext = Path.GetExtension(path).ToLowerInvariant()
        match ext with
        | ".cs" -> updateAssembly path version csReplacing
        | ".fs" -> updateAssembly path version fsReplacing
        | ".vb" -> updateAssembly path version vbReplacing
        | _ -> ()

    | Manifest path -> updateManifest path version
    | PList path -> ()
    

