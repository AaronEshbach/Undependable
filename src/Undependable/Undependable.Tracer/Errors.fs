namespace Undependable.Tracer

type CompilerError =
| SyntaxError of string
| ProjectCompilerError of string
| CheckingAborted
| UnresolvedReferenceError of (string * string)
| MultipleCompilerErrors of CompilerError list
| UnexpectedCompilerError of exn

type DependencyTracerError =
| UnknownTargetFramework of string
| UnsupportedProjectFileFormat of string
| CompilerErrorParsingSourceFile of (string * CompilerError)
| UnexpectedDependencyTracerError of exn