# Switching MOTMaster and DAQ to "SDK-style" project files

This explains a change made to two project files in this repo: `MOTMaster/MOTMaster.csproj`
and `DAQ/DAQ.csproj`. If you've never heard the term "SDK-style" before, this doc is for
you — no prior MSBuild knowledge assumed.

## What's a `.csproj` file, anyway?

Every C# project (like `MOTMaster` or `DAQ`) has one `.csproj` file that tells the build
tool (MSBuild) three things: which `.cs` source files belong to the project, which other
libraries it needs (references), and how to build it for each configuration (CaF, AlF,
EDM, and so on — this repo has one build configuration per experiment).

There are two *formats* a `.csproj` file can be written in. Both do the same job; they're
just different ways of writing it down, a bit like the difference between an old, verbose
recipe that spells out every single step and a modern one that assumes you know the
basics and just lists what's different about this dish.

- **Old-style** (what these two files used to be, and what most other projects in this
  repo still are): every single source file is listed by name — `<Compile Include="Foo.cs" />`
  — one line per file. The file typically runs to 500-900 lines.
- **SDK-style** (what `MOTMaster.csproj`/`DAQ.csproj` are now): starts with
  `<Project Sdk="Microsoft.NET.Sdk">` and doesn't list source files individually — it
  just says "compile every `.cs` file in this folder," with a short list of exceptions.
  The same project shrinks to well under 200 lines.

Nothing about what the *code* does changes. This is purely about which dialect the
project's own "table of contents" file is written in.

## Why bother?

Modern .NET tooling — the `dotnet` command-line tool, and things built on top of it like
Visual Studio Code's Testing panel — only fully understands SDK-style projects. Old-style
projects mostly still work with it, but not always, and this repo hit real, concrete
problems from it this session:

- `MoleculeMOTMasterScripts.csproj` (a different, unconverted project) can't be built
  with the plain `dotnet build` command at all — it fails outright.
- The new automated test suite for MOTMaster's experiment scripts
  (`MOTMaster.ScriptTests`, see `.claude/skills/motmaster-controller/SKILL.md` if you're
  curious) had to avoid the normal, direct way of depending on MOTMaster and DAQ, and
  instead reference their already-built `.exe`/`.dll` files directly — a workaround
  needed specifically because MOTMaster and DAQ were old-style.

Converting `MOTMaster.csproj` and `DAQ.csproj` to SDK-style removes that friction at the
source: standard `dotnet build`/`dotnet test` commands now work directly against them,
and VS Code's Testing panel can discover and run tests normally.

## What actually changed

**The goal throughout was: change the file format, not the behavior.** Every experiment's
build configuration (CaF, AlF, EDM, Buffer, and all the others) still produces the exact
same `.exe`/`.dll` in the exact same output folder as before. If you build under your
experiment's configuration the way you always have, nothing should look different.

That said, a few small, deliberate things changed along the way — all listed here rather
than buried in a commit diff:

- **DAQ now targets .NET Framework 4.6.1 instead of 4.5.2** (to match MOTMaster, which
  was already on 4.6.1). This was forced, not optional: one of DAQ's resource files
  needed a component that simply isn't available for anything older than 4.6.1. 4.6.1 is
  a strict superset of 4.5.2 — everything that worked before still works, plus more.
- **A handful of old, unused backup files are now explicitly excluded from the build**
  (things like `Controller - Copy.cs`, `PatternBuilder32 - Copy.cs` — leftover duplicate
  copies of real files, never part of the actual program). Under the old file format,
  simply not listing them was enough to keep them out. The new format compiles
  everything in the folder by default, so these had to be named explicitly as
  exclusions. They're untouched on disk — nothing was deleted, just newly told to stay
  out of the build, exactly as before.
- **One real, tiny bug was fixed**: `MOTMaster.csproj`'s CaF configuration only enabled
  DDS support when building for "AnyCPU" — not for the x64 or x86 build variants of the
  same CaF configuration. It now works the same way on all three. (Nobody appeared to
  build CaF as x64/x86 in practice, but it was inconsistent, so it's fixed.)
- **One other pre-existing oddity was found and deliberately left alone**: `DAQ.csproj`'s
  AlF configuration turns on a compiler switch named `CaF` instead of `AlF` — almost
  certainly a copy/paste leftover from whoever set up the AlF configuration originally.
  This wasn't touched, since fixing behavior wasn't the goal of this change — it's
  flagged here so whoever owns the AlF experiment configuration can decide whether it
  matters and fix it deliberately.
- **The old-style `packages.config` file is gone**, replaced by `<PackageReference>`
  entries directly in the `.csproj` (the modern way of saying "this project needs NuGet
  package X, version Y"). Same packages, same versions, different bookkeeping format.

## Where this lives right now

This was done on a separate git branch (`sdk-style-motmaster-daq`), specifically so it
could be built and tested in isolation before touching the branch anyone else is working
from. See the commit(s) on that branch for the exact diffs, and ask if you want a hand
merging it once you're happy with it.

## How to check it still works

Two things were used to confirm nothing broke, and are safe for anyone to re-run:

1. Build MOTMaster the normal way for your configuration, e.g.:
   ```
   MSBuild.exe MOTMaster\MOTMaster.csproj /p:Configuration=CaF /p:Platform=AnyCPU
   ```
   should produce `MOTMaster\bin\CaF\MOTMaster.exe`, same as always.
2. `dotnet test MOTMaster.ScriptTests.slnf` from the repo root runs the automated script
   checks and reports pass/fail per experiment script — a green run here (aside from
   pre-existing, known failures unrelated to this change) means MOTMaster can still
   compile and run every script the same way it always could.
