﻿namespace OrderedCollection
open FSharpx.Collections
open FSharpx.Functional
open FSharpx.Functional.Prelude
open System.Linq
open Collections.Immutable
open System.Collections.Immutable

type SkipList<'a>(data : ImmutableList<ImmutableList<'a>>, comparer : 'a -> 'a -> int) =
    class 
        member x.Data = data
        member x.Comparer = comparer
    end


module SkipList =
    begin
        let randomLevel () =
            let rand = new System.Random() in
            let mutable level : int = 1 in 
            while rand.Next(0, 1) > 0 do 
                level <- level + 1
            done 
            level

        let emptyWith (comparer : 'a -> 'a -> int) = 
            SkipList(ImmutableList.empty, comparer)

        let isEmpty (list : SkipList<'a>) =
            list.Data |> ImmutableList.isEmpty

        let add (value : 'a) (list : SkipList<'a>) =
            let data = list.Data in 
            let comparer = list.Comparer in
            match list |> isEmpty with 
            | true ->
                SkipList(value 
                         |> ImmutableList.singleton 
                         |> ImmutableList.singleton, comparer)
            | false ->
                emptyWith comparer
                //let level = randomLevel() in 
                //let rec inner n d =
                //    match n = 
    end

//type SkipMap<'a, 'b>(list : PersistentVector<'a>, compare : 'a -> 'a -> bool) =
//    class
//        member x.Data = list
//        member x.Compare = compare
//    end

//module SkipMap =
//    begin 
//        let emptyWith compare =
//            SkipMap(List.empty, compare)

//        //let add (k : 'k) (v : 'v) (map : SkipMap<'k, 'v>) =
//        //    let data = map.Data in 
//        //    let compare = map.Compare
//        //    match data with 
//        //    | [] -> 
//    end

module Seq =
    begin
        let (|IsEmpty|_|) (set : 'a seq) =
            match set |> Seq.isEmpty with
            | true -> Some()
            | false -> None
    end

//[<CustomEquality; NoComparison>]
type OrderedSet<[<EqualityConditionalOn>] 'k when 'k : comparison>(first : 'k option, map : Map<'k, ('k option * 'k option)>, last : 'k option) =
    class
        member x.First = first
        member x.Map = map
        member x.Last = last
        override x.ToString() =
            map.ToString()
        //static member op_Equality (x : OrderedSet<'k>, y : OrderedSet<'k>) =
        //    x.First = y.First && x.Last = y.Last && x.Map = y.Map
        //static member op_Inequality (x : OrderedSet<'k>, y : OrderedSet<'k>) =
        //    x.First <> y.First || x.Last <> y.Last || x.Map = y.Map
        override x.GetHashCode() = hash x.Map
        override x.Equals(y : obj) =
            match y with
            | :? OrderedSet<'k> as y ->
                (x.First = y.First && x.Last = y.Last && x.Map = y.Map) || (x.Map |> Map.isEmpty && y.Map |> Map.isEmpty)
            | _ -> false
        interface System.IComparable
            with
                member this.CompareTo(o : obj) = 0
            end
    end
    
module OrderedSet =
    begin
        let isEmpty (set : OrderedSet<'k>) =
            set.Map |> Map.isEmpty

        let (|IsEmpty|_|) (set : OrderedSet<'k>) =
            match set |> isEmpty with
            | true -> Some()
            | false -> None

        let add (value : 'k) (set : OrderedSet<'k>) =
            match set with
            | IsEmpty ->
                let first = Some value in
                let last = Some value in
                let map = Map.empty |> Map.add value (None, None) in
                OrderedSet(first, map, last)
            | _ ->
                let first = set.First |> Option.get in
                let last = set.Last |> Option.get in
                let map = set.Map in
                match map |> Map.tryFind value with
                | Some (_, _) ->
                    set
                | None ->
                    let (prev, _) = map |> Map.find last in
                    let map =
                        map
                        |> Map.updateWith (fun (prev, _) -> Some (prev, Some value)) last
                        |> Map.add value (Some last, None)
                    in
                    OrderedSet(Some first, map, Some value)

        let contains (value : 'k) (set : OrderedSet<'k>) =
            set.Map |> Map.containsKey value

        let count (set : OrderedSet<'k>) =
            set.Map |> Map.count

        let empty<'k when 'k : comparison> : OrderedSet<'k> =
            OrderedSet(None, Map.empty, None)
            
        let fold (folder : 's -> 't -> 's) (state : 's) (set : OrderedSet<'t>) =
            match set with
            | IsEmpty -> state
            | _ ->
                let first = set.First |> Option.get in
                let last = set.Last |> Option.get in
                let rec inner s t =
                    let s = folder s t in
                    match t = last with
                    | true -> s
                    | false ->
                        let map = set.Map in
                        let (_, next) = map.[t] in
                        inner s (next |> Option.get)
                in
                inner state first

        let remove (value : 'k) (set : OrderedSet<'k>) =
            match set with
            | IsEmpty -> set
            | _ ->
                let first = set.First in
                let last = set.Last in
                let map = set.Map in
                match map |> Map.tryFind value with
                | Some (None, None) ->
                    empty
                | Some (None, Some next) ->
                    let map =
                        map
                        |> Map.remove value
                        |> Map.updateWith (fun (_, n) -> Some (None, n)) next
                    in
                    OrderedSet(Some next, map, last)
                | Some (Some prev, None) ->
                    let map =
                        map
                        |> Map.remove value
                        |> Map.updateWith (fun (p, _) -> Some (p, None)) prev
                    in
                    OrderedSet(set.First, map, Some prev)
                | Some (Some prev, Some next) ->
                    let map =
                        map
                        |> Map.remove value
                        |> Map.updateWith (fun (p, _) -> Some (p, Some next)) prev
                        |> Map.updateWith (fun (_, n) -> Some (Some prev, n)) next
                    in
                    OrderedSet(first, map, last)
                | None -> set

        let difference (set1 : OrderedSet<'k>) (set2 : OrderedSet<'k>) =
            fold (flip remove) set1 set2
            
        let exists (predicate : 'k -> bool) (set : OrderedSet<'k>) =
            set.Map |> Map.exists (fun k _ -> predicate k)
            
        let filter (predicate : 'k -> bool) (set : OrderedSet<'k>) =
            fold (fun s i ->
                match (predicate i) with
                | true -> s |> add i
                | false -> s)
                empty set
                
        let iter (action : 'k -> unit) (set : OrderedSet<'k>) =
            set
            |> fold (fun a b ->
                begin
                    action b;
                    a;
                end
                ) empty
            |> ignore

        let forall (predicate : 'k -> bool) (set : OrderedSet<'k>) =
            set.Map |> Map.forall (fun k _ -> predicate k)
            
        let intersect (set1 : OrderedSet<'k>) (set2 : OrderedSet<'k>) =
            set1
            |> filter (flip exists set2 << (=))
            
        let intersectMany (sets : OrderedSet<'k> seq) =
            sets
            |> Seq.reduce intersect
            
        let isSubset (set1 : OrderedSet<'k>) (set2 : OrderedSet<'k>) =
            set1 |> forall (flip exists set2 << (=))
            
        let isProperSubset (set1 : OrderedSet<'k>) (set2 : OrderedSet<'k>) =
            isSubset set1 set2 &&
            (set2 |> count) > (set1 |> count)
            
        let isSuperset (set1 : OrderedSet<'k>) (set2 : OrderedSet<'k>) =
            set2 |> forall (flip exists set1 << (=))
            
        let isProperSuperset (set1 : OrderedSet<'k>) (set2 : OrderedSet<'k>) =
            isSuperset set1 set2 &&
            (set2 |> count) < (set1 |> count)

        let map (mapping : 'a -> 'b) (set : OrderedSet<'a>) =
            fold (fun s i -> s |> add (mapping i)) empty set
            
        let maxElement (set : OrderedSet<'a>) =
            set.Map |> Map.keys |> Seq.max

        let minElement (set : OrderedSet<'a>) =
            set.Map |> Map.keys |> Seq.min
            
        let ofArray (array : 'k []) =
            Array.fold (flip add) empty array
            
        let ofList (elements : 'k list) =
            List.fold (flip add) empty elements

        let ofSeq (elements : 'k seq) =
            Seq.fold (flip add) empty elements

        let partition (predicate : 'k -> bool) (set : OrderedSet<'k>) =
            fold (fun (set1, set2) t ->
                match predicate t with
                | true -> (set1 |> add t, set2)
                | false -> (set1, set2 |> add t)
            ) (empty, empty) set
            
        let singleton (value : 'k) =
            empty |> add value

        let foldBack (folder : 't -> 's -> 's) (set : OrderedSet<'t>) (state : 's) =
            match set with
            | IsEmpty -> state
            | _ ->
                let first = set.First |> Option.get in
                let last = set.Last |> Option.get in
                let rec inner s t =
                    let s = folder t s in
                    match t = first with
                    | true -> s
                    | false ->
                        let map = set.Map in
                        let (prev, _) = map.[t] in
                        inner s prev.Value
                inner state last

        let toList (set : OrderedSet<'k>) =
            foldBack List.cons set []

        let toArray (set : OrderedSet<'k>) =
            set
            |> toList
            |> List.toArray

        let toSeq (set : OrderedSet<'k>) =
            set
            |> toList
            |> List.toSeq
            
        let union (set1 : OrderedSet<'k>) (set2 : OrderedSet<'k>) =
            set2
            |> fold (fun s i -> s |> add i) set1
            
        let unionMany (sets : OrderedSet<'k> seq) =
            match sets |> Seq.isEmpty with
            | true -> empty
            | false -> sets |> Seq.reduce union
    end
            