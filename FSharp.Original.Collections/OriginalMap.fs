﻿namespace Original.Collections
open System.Collections.Generic
open Extension

type OriginalMap<'a, 'b  when 'a : comparison>(x : 'a seq, y : Map<'a, 'b>) =
    member this.Seq() = x
    member this.Map() = y
    
module OriginalMap =
    let add (key : 'a) (value : 'b) (set : OriginalMap<'a, 'b>) =
        let (a, b) = (set.Seq(), set.Map())
        OriginalMap(Seq.append a (Seq.singleton key), b.Add(key, value))

    let containsKey (key : 'a) (table : OriginalMap<'a, 'b>) =
        Map.containsKey key (table.Map())

    let empty<'a, 'b  when 'a : comparison and 'b : comparison> =
        OriginalMap<'a, 'b>(Seq.empty, Map.empty)

    let exists (predicate : 'a -> 'b -> bool) (table : OriginalMap<'a, 'b>) =
        Map.exists predicate (table.Map())

    let filter (predicate : 'a -> 'b -> bool) (table : OriginalMap<'a, 'b>) =
        let seq, map = table.Seq(), table.Map()
        OriginalMap(Seq.filter (fun x -> predicate x map.[x]) seq, Map.filter predicate map)

    let find (key : 'a) (table : OriginalMap<'a, 'b>) =
        Map.find key (table.Map())

    let findKey (predicate : 'a -> 'b -> bool) (table : OriginalMap<'a, 'b>) =
        Map.findKey predicate (table.Map())

    let fold (folder : 's -> 'a -> 'b -> 's) (state : 's) (table : OriginalMap<'a, 'b>) =
        let seq, map = table.Seq(), table.Map()
        Seq.fold (fun s x -> folder state x map.[x]) state seq

    let foldBack (folder : 'a -> 'b -> 's -> 's) (table : OriginalMap<'a, 'b>) (state : 's) =
        let seq, map = table.Seq(), table.Map()
        Seq.foldBack (fun x s -> folder x map.[x] state) seq state

    let forall (predicate : 'a -> 'b -> bool) (table : OriginalMap<'a, 'b>) =
        Map.forall predicate (table.Map())

    let isEmpty (table : OriginalMap<'a, 'b>) =
        Map.isEmpty (table.Map())

    let iter (action : 'a -> 'b -> unit) (table : OriginalMap<'a, 'b>) =
        Map.iter action (table.Map())

    let map (mapping : 'a -> 'b -> 'c) (table : OriginalMap<'a, 'b>) =
        let seq, map = table.Seq(), table.Map()
        let map = Map.map mapping map
        OriginalMap(seq, map)

    let ofArray (elements : ('a * 'b) []) =
        let seq = Seq.ofArray elements
        let map = Map.ofSeq seq
        OriginalMap(Seq.map (fun (k, _) -> k) seq, map)

    let ofList (elements : ('a * 'b) list) =
        let seq = Seq.ofList elements
        let map = Map.ofSeq seq
        OriginalMap(Seq.map (fun (k, _) -> k) seq, map)
        
    let ofSeq (elements : ('a * 'b) seq) =
        let seq = Seq.map (fun (k, _) -> k) elements
        let map = Map.ofSeq elements
        OriginalMap(seq, map)

    let partition (predicate : 'a -> 'b -> bool) (table : OriginalMap<'a, 'b>) =
        let seq, map = table.Seq(), table.Map()
        let seq1, seq2 = Seq.partition (fun x -> predicate x map.[x]) seq
        let map1, map2 = Map.partition predicate map
        OriginalMap(seq1, map1), OriginalMap(seq2, map2)

    let pick (chooser : 'a -> 'b -> 'c option) (table : OriginalMap<'a, 'b>) =
        let seq, map = table.Seq(), table.Map()
        Seq.pick (fun x -> chooser x map.[x]) seq

    let remove (key : 'a) (table : OriginalMap<'a, 'b>) =
        let seq, map = table.Seq() |> Seq.filter ((=) key), table.Map() |> Map.remove key
        OriginalMap(seq, map)

    let toSeq (table : OriginalMap<'a, 'b>) =
        table.Seq()

    let toArray (table : OriginalMap<'a, 'b>) =
        table |> toSeq |> Array.ofSeq
        
    let toList (table : OriginalMap<'a, 'b>) =
        table |> toSeq |> List.ofSeq

    let tryFind (key : 'a) (table : OriginalMap<'a, 'b>) =
        Map.tryFind key (table.Map())

    let tryFindKey (predicate : 'a -> 'b -> bool) (table : OriginalMap<'a, 'b>) =
        Map.tryFindKey predicate (table.Map())

    let tryPick (chooser : 'a -> 'b -> 'c option) (table : OriginalMap<'a, 'b>) =
        let seq, map = toSeq table, table.Map()
        Seq.tryPick (fun x -> chooser x map.[x]) seq