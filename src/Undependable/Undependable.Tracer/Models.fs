namespace Undependable.Tracer

type Assembly =
    {
        Name: string
        Version: string
        PublicKey: string option
        Culture: string option
        FullyQualifiedName: string
    }

type Package =
    {
        Name: string
        Version: string
        Assemblies: Assembly list
    }

type SourceFile =
    {
        Name: string
        Path: string
        References: Assembly list
    }

type ProjectType =
| NetFramework
| NetStandard
| NetCore

type PackageReference =
    {
        Name: string
        Version: string
    }

type AssemblyReference =
    {
        Name: string
        HintPath: string option
    }

type Project =
    {
        Name: string
        ProjectFile: string
        Type: ProjectType
        TargetFramework: string
        Version: string
        SourceFiles: string list
        PackageReferences: PackageReference list
        ProjectReferences: string list
        AssemblyReferences: AssemblyReference list
    }

type CompiledProject =
    {
        Name: string
        SourceFiles: SourceFile list
        References: Assembly list
    }