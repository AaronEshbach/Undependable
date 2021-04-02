namespace Undependable.Tracer

open CurryOn

type ICompiler =
    abstract member CompileProject : Project -> AsyncResult<CompiledProject, CompilerError>