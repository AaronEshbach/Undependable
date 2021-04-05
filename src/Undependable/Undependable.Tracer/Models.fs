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

type ProjectSourceFile =
    {
        Name: string
        Path: string
    }

type Project =
    {
        Name: string
        RootFolder: string
        ProjectFile: string
        Type: ProjectType
        TargetFramework: string
        Version: string
        SourceFiles: ProjectSourceFile list
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