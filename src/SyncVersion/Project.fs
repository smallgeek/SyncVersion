module Project
  open System.IO
  open System.Xml.Linq
  open System.Xml.XPath

  let getVersionFile(projectFile: string) =

    let projectDirectory = Path.GetDirectoryName projectFile

    let ns = "http://schemas.microsoft.com/developer/msbuild/2003"
    let doc = XDocument.Load projectFile

    let project = doc.Element <| XName.Get("Project", ns)

    let assemblies =
      let groups = project.Elements <| XName.Get("ItemGroup", ns) 
      groups 
      |> Seq.collect (fun g -> g.Elements <| XName.Get("Compile", ns))
      |> Seq.map (fun elm -> elm.Attribute <| XName.Get("Include"))
      |> Seq.filter (fun attr -> Path.GetFileNameWithoutExtension(attr.Value) = "AssemblyInfo")
      |> Seq.map (fun attr -> Path.Combine(projectDirectory, attr.Value))

    let manifests =
      let groups = project.Elements <| XName.Get("PropertyGroup", ns) 
      groups 
      |> Seq.collect (fun g -> g.Elements <| XName.Get("AndroidManifest", ns))
      |> Seq.map (fun elm -> Path.Combine(projectDirectory, elm.Value))

    Seq.append assemblies manifests
