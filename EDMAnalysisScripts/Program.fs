// this stub exists to stop compile errors. This project is not
// used as a compiled program, but rather exists so that the f#
// scripts can be edited with Visual Studio. It can also be used for
// scripted debugging.

open SonOfSirCachealot

let bs = new BlockStore()

let q = new BlockStoreQuery()
q.BlockIDs <- [| 1; 3; 4; 5 |]
q.BlockQuery <- new BlockStoreBlockQuery()
let dq = new BlockStoreDetectorQuery()
dq.Detector <- "top"
dq.Channels <- [| "E.B"; "DB"; "RF1F" |]
q.BlockQuery.DetectorQueries <- [| dq |]

ignore(bs.processQuery(q))

