module File

type SourceFile = 
  | Assembly of path: string
  | Manifest of path: string
  | PList of path: string

