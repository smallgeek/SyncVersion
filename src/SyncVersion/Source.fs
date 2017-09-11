namespace Source
  open System.IO

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
    
