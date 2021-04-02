namespace Undependable.Tracer.FSharp

open FSharp.Compiler.SourceCodeServices
open Undependable.Tracer

module Compiler =
    let private sysLib name =
        if System.Environment.OSVersion.Platform = System.PlatformID.Win32NT then
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86) +
            sprintf @"\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\%s.dll" name
        else
            let sysDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()
            System.IO.Path.Combine(sysDir, sprintf "%s.dll" name)

    let getReferences (project: Project) =
        [ 
            yield sysLib "mscorlib"
            yield sysLib "System"
            yield sysLib "System.Core" 
            for assembly in project.AssemblyReferences do
                match assembly.HintPath with
                | Some path ->
                    yield path
                | None ->
                    yield sysLib assembly.Name
        ]

    let getArguments (project: Project) =
        [|
            yield "--simpleresolution"
            yield "--noframework"
            yield "--debug:full"
            yield "--define:DEBUG"
            yield "--optimize-"
            yield "--warn:3"
            yield "--fullpaths"
            yield "--flaterrors"
            yield "--target:library"
            for file in project.SourceFiles do
                yield file
            for r in getReferences project do
                  yield "-r:" + r 
        |]
