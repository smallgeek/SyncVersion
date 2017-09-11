namespace Source
  open System.IO

  type Solution = 
    static member GetProject(sln: string) =

      let lines = File.ReadAllLines sln

      seq {
        for line in lines do
          if line.StartsWith("Project(") then
            let field = line.Split(',')
            yield field.[1]
      }
    
