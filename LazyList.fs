﻿namespace Original.Collections

open System.Collections.Generic
open System.Collections
open System
open Extension

type LazyList<'a>(x : 'a seq, y : 'a list) = 
    member this.Seq() = x
    member this.List() = y
    member this.Cons(value : 'a) = 
        let ls = value :: this.List()
        let seq = ls |> Seq.ofList
        LazyList(seq, ls)

module LazyList = 
    let makeLazyListFromSeq (seq : 'a seq) = 
        let ls = List.ofSeq seq
        LazyList(seq, ls)
    
    let map (mapping : 'a -> 'b) (list : LazyList<'a>) = 
        let seq = list.Seq() |> Seq.map mapping
        makeLazyListFromSeq seq
    
    let filter (predicate : 'a -> bool) (list : LazyList<'a>) = 
        let seq = list.Seq() |> Seq.filter predicate
        makeLazyListFromSeq seq
    
    let empty<'a> = LazyList<'a>(Seq.empty, List.empty)

    let toSeq (list : LazyList<'a>) =
        list.Seq() |> Seq.rev

    let fold (folder : 's -> 't -> 's) (state : 's) (list : LazyList<'t>) = 
        list
        |> toSeq
        |> Seq.fold folder state

    let foldBack (folder : 't -> 's -> 's) (list : LazyList<'t>) (state : 's) =
        list
        |> fold (fun s t -> folder t s) state