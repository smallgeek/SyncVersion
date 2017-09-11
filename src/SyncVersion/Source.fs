namespace Source
  open System.IO
  open System.Xml.Linq
  open System.Xml.XPath

  type Solution = 
    static member GetProject(solutionFile: string) =

      let solutionDirectory = Path.GetDirectoryName solutionFile
      let lines = File.ReadAllLines solutionFile

      seq {
        for line in lines do
          if line.StartsWith "Project(" then
            let field = line.Split ','

            let projectPath = field.[1].Replace('"'.ToString(), "").Trim()
            let ext = Path.GetExtension(projectPath).ToLowerInvariant()

            if ext = ".csproj" || ext = ".vbproj" || ext = ".fsproj" then
              yield Path.Combine(solutionDirectory, projectPath) |> Path.GetFullPath
      }
    
  type Project =
    static member GetVersionFile(proj: string) =

      let ns = "http://schemas.microsoft.com/developer/msbuild/2003"
      let doc = XDocument.Load proj

      let project = doc.Element <| XName.Get("Project", ns)
      let groups = project.Elements <| XName.Get("ItemGroup", ns) 

      let files =
        groups 
        |> Seq.collect (fun g -> g.Elements <| XName.Get("Compile", ns))
        |> Seq.map (fun elm -> elm.Attribute <| XName.Get("Include"))
        |> Seq.filter (fun attr -> Path.GetFileNameWithoutExtension(attr.Value) = "AssemblyInfo")
        |> Seq.map (fun attr -> Path.Combine(proj, attr.Value))

      files
