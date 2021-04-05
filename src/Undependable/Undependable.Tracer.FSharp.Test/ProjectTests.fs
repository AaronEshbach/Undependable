namespace Undependable.Tracer.FSharp.Test

open CurryOn
open CurryOn.UnitTest
open Undependable.Tracer
open Undependable.Tracer.FSharp
open Microsoft.VisualStudio.TestTools.UnitTesting

[<TestClass>]
type ProjectTests () =

    [<TestMethod>]
    member __.``.NET Core F# Project should compile`` () =
        async {
            let projectFile = @"C:\repos\Undependable\src\Undependable\Undependable.Tracer.FSharp.Test\Undependable.Tracer.FSharp.Test.fsproj"
            let! loadResult = projectFile |> Project.load |> AsyncResult.toAsync
            
            match loadResult with
            | Ok project ->
                let! compileResult = project |> Project.compile |> AsyncResult.toAsync
                match compileResult with
                | Ok compiledProject ->
                    compiledProject.References.Length |> Assert.greaterThan project.PackageReferences.Length
                | Error error ->                    
                    Assert.failf "Failed to compile project: %A" error
            | Error error ->
                Assert.failf "Failed to load project: %A" error
        } |> Async.RunSynchronously
        
