namespace Undependable.Tracer

open CurryOn
open CurryOn.IO
open CurryOn.Xml
open System.IO

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Project =
    let load (projectFile: string) =
        asyncResult {
            let file = projectFile |> FileInfo
            let rootDirectory = file.Directory.FullName
            let! xml = projectFile |> XDocument.load

            let propertyGroup = xml |> XDocument.tryElement "PropertyGroup"

            match propertyGroup with
            | Some element ->
                let name = element |> XElement.tryElement "Name" |> Option.map XElement.value |> Option.defaultValue (File.name projectFile)
                let! targetFramework = element |> XElement.tryElement "TargetFramework" |> Result.ofOption (UnknownTargetFramework "") |> Result.map XElement.value
                let projectVersion = element |> XElement.tryElement "Version" |> Option.map XElement.value |> Option.defaultValue "1.0.0"
                let itemGroups = xml |> XDocument.elements "ItemGroup"

                let sourceFiles =
                    itemGroups
                    |> Seq.collect (XElement.descendents "Compile")
                    |> Seq.choose (XElement.tryAttribute "Include")
                    |> Seq.map XAttribute.value
                    |> Seq.map (fun name -> { Name = name; Path = sprintf "%s\\%s" rootDirectory name})

                let projectReferences =
                    itemGroups
                    |> Seq.collect (XElement.descendents "ProjectReference")
                    |> Seq.choose (XElement.tryAttribute "Include")
                    |> Seq.map XAttribute.value

                let packageReferences =
                    itemGroups
                    |> Seq.collect (XElement.descendents "PackageReference")
                    |> Seq.choose (fun element -> 
                        element 
                        |> XElement.tryAttribute "Include" 
                        |> Option.bind (fun name -> element |> XElement.tryAttribute "Version" |> Option.map (fun version -> name, version)))
                    |> Seq.map (fun (name, version) -> { Name = XAttribute.value name; Version = XAttribute.value version })
                
                let assemblyReferences =
                    itemGroups
                    |> Seq.collect (XElement.descendents "Reference")
                    |> Seq.choose (fun element ->
                        let name = element |> XElement.tryAttribute "Include" |> Option.map XAttribute.value
                        let hintPath = element |> XElement.tryElement "HintPath" |> Option.map XElement.value
                        name |> Option.map (fun assembly -> { Name = assembly; HintPath = hintPath } ))
                
                let! projectType =
                    match xml.Root |> XElement.tryAttribute "Sdk" with
                    | Some _ ->
                        match targetFramework with
                        | Like "netcore*" -> Ok NetCore
                        | Like "netstandard*" -> Ok NetStandard
                        | other -> Error <| UnknownTargetFramework other
                    | None ->
                        Ok NetFramework

                return
                    {
                        Name = name
                        Type = projectType
                        TargetFramework = targetFramework
                        RootFolder = rootDirectory
                        ProjectFile = projectFile
                        Version = projectVersion
                        SourceFiles = sourceFiles |> Seq.toList
                        PackageReferences = packageReferences |> Seq.toList
                        ProjectReferences = projectReferences |> Seq.toList
                        AssemblyReferences = assemblyReferences |> Seq.toList
                    }
            | None ->
                return! Error <| UnsupportedProjectFileFormat projectFile
        }