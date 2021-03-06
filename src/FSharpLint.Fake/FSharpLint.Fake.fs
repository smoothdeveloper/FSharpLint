﻿// FSharpLint, a linter for F#.
// Copyright (C) 2016 archaeron
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

module FSharpLint.Fake

open System
open Fake
open FSharpLint.Application
open FSharpLint.Application.FSharpLintWorker

let private printException (e:Exception) =
    "Exception Message:" + e.Message + "Exception Stack Trace:" + e.StackTrace
    |> Console.WriteLine

let private failedToParseFileError (file:string) parseException =
    printfn "%A" file
    printException parseException

/// the default only prints something if FSharpLint found a lint in a file
let private defaultProgress (progress:ProjectProgress) = 
    match progress with
    | Failed(filename, e) ->
        failedToParseFileError filename e
    | _ -> ()

let private defaultErrorReceived (error:LintWarning.Warning) =
    let formattedError = LintWarning.getWarningWithLocation error.Range error.Input
    error.Info + System.Environment.NewLine + formattedError
    |> Console.WriteLine
    
[<NoEquality; NoComparison>]
type LintOptions =
    { Progress: ProjectProgress -> unit
      ErrorReceived: LintWarning.Warning -> unit
      FailBuildIfAnyWarnings: bool }

let defaultLintOptions =
    { Progress = defaultProgress
      ErrorReceived = defaultErrorReceived
      FailBuildIfAnyWarnings = false }

/// Runs FSharpLint on a project.
/// ## Parameters
/// 
///  - `setParams ` - Function used to manipulate the default FSharpLint value.
///  - `projectFile` - The project file of the project you want to lint
/// 
/// ## Sample usage
///
///     Target "Lint" (fun _ ->
///         FSharpLint (fun o -> { o with ErrorReceived = System.Action<LintWarning.Error>(customErrorFunction) }) projectFile
///     )
let FSharpLint (setParams: LintOptions->LintOptions) (projectFile: string) =
    let parameters = defaultLintOptions |> setParams

    traceStartTask "FSharpLint" projectFile

    let numberOfWarnings, numberOfFiles = ref 0, ref 0
    
    let errorReceived error = 
        incr numberOfWarnings
        parameters.ErrorReceived error

    let parserProgress (progress:ProjectProgress) =
        match progress with
        | ReachedEnd(_) -> incr numberOfFiles
        | _ -> ()

        parameters.Progress progress

    let options = 
        { ReceivedWarning = Some(errorReceived)
          FinishEarly = None
          Configuration = None }

    match FSharpLintWorker.RunLint(projectFile, options, Some(parserProgress)) with
    | Success when parameters.FailBuildIfAnyWarnings && !numberOfWarnings > 0 ->
        failwithf "Linted %s and failed the build as warnings were found. Linted %d files and found %d warnings." projectFile !numberOfFiles !numberOfWarnings
    | Success ->
        tracefn "Successfully linted %s. Linted %d files and found %d warnings." projectFile !numberOfFiles !numberOfWarnings
    | Failure(message) ->
        sprintf "Failed to lint %s. Failed with: %s" projectFile message |> traceError

    traceEndTask "FSharpLint" projectFile