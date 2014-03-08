﻿(*
    FSharpLint, a linter for F#.
    Copyright (C) 2014 Matthew Mcveigh

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*)

module TestRuleBase

open NUnit.Framework
open System.Linq
open Microsoft.FSharp.Compiler.Range
open FSharpLint.Framework.Ast

[<AbstractClass>]
type TestRuleBase(visitor) =
    let errorRanges = System.Collections.Generic.List<range * string>()

    let postError (range:range) error =
        errorRanges.Add(range, error)

    member this.Parse input = parseInput input [visitor postError]

    member this.ErrorExistsAt(startLine, startColumn) =
        errorRanges.Any(fun (r, _) -> r.StartLine = startLine && r.StartColumn = startColumn)

    member this.ErrorsAt(startLine, startColumn) =
        errorRanges.Where(fun (r:range, _) -> r.StartLine = startLine && r.StartColumn = startColumn)

    member this.ErrorWithMessageExistsAt(message, startLine, startColumn) =
        (this.ErrorsAt(startLine, startColumn)).Any(fun (_, e) -> e = message)

    [<SetUp>]
    member this.SetUp() = 
        errorRanges.Clear()