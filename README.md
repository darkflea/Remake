# \# ReMake

# 

# ReMake is a modern, .NET‑based host for the Premake build system — designed to be fully compatible with existing Premake scripts while exploring new ideas in tooling, introspection, and developer experience.

# 

# Built on top of \*\*KeraLua\*\*, ReMake embeds a fast, lightweight Lua runtime that preserves Premake’s scripting model while enabling deep integration with .NET. This foundation allows ReMake to offer:

# 

# \- A clean, idiomatic C# execution pipeline  

# \- Rich introspection and reflection capabilities  

# \- Full compatibility with existing Premake5 Lua scripts  

# \- A platform for experimenting with new workflows, help systems, and extension models  

# 

# ReMake aims to honour Premake’s philosophy while pushing the ecosystem forward with thoughtful architecture, maintainability, and developer joy.

# 

# \---

# 

# \## Changes

# 

# \### ReMake no longer uses `premake\_main.lua`

# 

# ReMake no longer uses \*\*`premake\_main.lua`\*\* as its entry point.  

# All existing Premake scripts continue to work exactly as before — nothing changes for users or for your project’s `premake5.lua`.

# 

# \---

# 

# \## Why the change?

# 

# \### Cleaner architecture  

# ReMake now drives Premake from a modern, idiomatic .NET execution pipeline instead of relying on Premake’s legacy bootstrap script.

# 

# \### Better control over startup  

# Removing `premake\_main.lua` allows ReMake to manage initialization, error handling, and help interception directly in C#, without patching or overriding upstream Lua files.

# 

# \### Improved introspection  

# With KeraLua hosting the Lua runtime, ReMake can load modules, inspect the DSL, and integrate with .NET far earlier and more cleanly than the old bootstrap allowed.

# 

# You can see this in \*\*`PremakeHost.cs` → `Execute`\*\* — ReMake now loads everything up to the point where the command line is processed.  

# This enables features like intercepting `--help` and rendering help output through \*\*Spectre.Console\*\* if desired.

# 

# \### Full compatibility preserved  

# The Premake DSL, APIs, and behaviour remain unchanged. ReMake simply replaces the bootstrap layer, not the scripting model.

# 

# \---

# 

# \## In short

# 

# \*\*The Lua you write stays the same — the engine running it is now cleaner, faster, and easier to extend.\*\*

# 

# Spectre.Console support can be added easily — see `ReMakeHelp.cs` for an example of how help output can be styled and structured.



