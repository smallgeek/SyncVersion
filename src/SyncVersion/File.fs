module File

open System.Text
open Ude

type SourceFile = 
  | Assembly of path: string
  | Manifest of path: string
  | PList of path: string

let detectEncoding path =
  use fs = System.IO.File.OpenRead(path)

  let detector = Ude.CharsetDetector()
  detector.Feed(fs)
  detector.DataEnd()

  if detector.Charset <> null then
    Encoding.GetEncoding(detector.Charset)
  else
    Encoding.Default