module blockstore

// Initialises the analysis environment - most of the functions that do the real work
// are in different scripts that are pulled in by this one.

#I @"..\SonOfSirCachealot\bin\EDMAnalysis"
#r "SonOfSirCachealot.dll"

open SonOfSirCachealot

let bs = new BlockStore()




