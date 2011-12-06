#load "init.fsx"

open SonOfSirCachealot

let bs = blockstore.bs

let q = new BlockStoreQuery()

q.Detector = "top"
q.blockIDs = [| 1; 2; 3 |]

bs.processQuery(q)