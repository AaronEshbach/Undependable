namespace Undependable.Tracer.FSharp

open CurryOn
open CurryOn.IO
open FSharp.Compiler.SourceCodeServices
open FSharp.Compiler.Text
open Undependable.Tracer
open System.IO
open System.Reflection

module Project =
    let private getReferencedAssembly (assembly: FSharpAssembly) =
        let assemblyName = AssemblyName(assembly.QualifiedName)
        { 
            Name = assembly.SimpleName 
            Version = assemblyName.Version.ToString()
            PublicKey = assemblyName.GetPublicKey() |> Option.ofObj |> Option.map Base64.toString
            Culture = assemblyName.CultureName |> Option.ofObj
            FullyQualifiedName = assembly.QualifiedName
        }

    let compileFile (compiler: FSharpChecker) (projectOptions: FSharpProjectOptions) (fileName: string) =
        asyncResult {
            let! sourceCode = fileName |> File.readAllText
            let! (parseResults, checkResults) = compiler.ParseAndCheckFileInProject(fileName, 1, SourceText.ofString sourceCode, projectOptions)
            if parseResults.ParseHadErrors then
                return! Error <| SyntaxError (parseResults.Errors |> Seq.map (fun error -> error.Message) |> String.join "\r\n")
            else
                match checkResults with
                | FSharpCheckFileAnswer.Aborted ->
                    return! Error CheckingAborted
                | FSharpCheckFileAnswer.Succeeded results ->
                    match results.Errors with
                    | [||] ->
                        let dependencies =
                            results.OpenDeclarations
                            |> Seq.collect (fun reference -> reference.Types)
                            |> Seq.map (fun fsType -> fsType.TypeDefinition.Assembly)
                            |> Seq.map getReferencedAssembly

                        return 
                            { 
                                Name = fileName 
                                Path = (FileInfo fileName).FullName
                                References = dependencies |> Seq.toList
                            }
                    | errors ->                        
                        return! Error <| SyntaxError (errors |> Seq.map (fun error -> error.Message) |> String.join "\r\n")
        }

    let compile (project: Project) =
        asyncResult {
            let compiler = FSharpChecker.Create()
            let arguments = Compiler.getArguments project
            let projectOptions = compiler.GetProjectOptionsFromCommandLineArgs(project.ProjectFile, arguments)
            
            let! fileResults = 
                project.SourceFiles
                |> List.map (compileFile compiler projectOptions)
                |> AsyncResult.Parallel
                
            return 
                {
                    Name = project.Name
                    SourceFiles = fileResults
                    References = fileResults |> List.collect (fun file -> file.References) |> List.distinctBy (fun assembly -> assembly.FullyQualifiedName)
                }
        }