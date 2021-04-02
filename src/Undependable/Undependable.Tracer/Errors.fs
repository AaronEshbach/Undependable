namespace Undependable.Tracer

type DependencyTracerError =
| UnknownTargetFramework of string
| UnsupportedProjectFileFormat of string
| UnexpectedDependencyTracerError of exn