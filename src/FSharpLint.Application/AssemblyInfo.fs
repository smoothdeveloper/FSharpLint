﻿namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSharpLint")>]
[<assembly: AssemblyProductAttribute("FSharpLint")>]
[<assembly: AssemblyDescriptionAttribute("Lint tool for F#.")>]
[<assembly: AssemblyVersionAttribute("0.1.14")>]
[<assembly: AssemblyFileVersionAttribute("0.1.14")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.1.14"
