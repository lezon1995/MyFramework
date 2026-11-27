# Pinned local dependencies

`com.code-philosophy.obfuz-7b06f0fed867.tgz` contains the unmodified Obfuz
source from `focus-creative-games/obfuz` at commit
`7b06f0fed8675c31cbae9760a93e7deec0c0a05c`, the revision previously recorded in
this project's `packages-lock.json`. Its upstream license is included.

The archive uses the Unity Package Manager `package/` root layout and is
referenced by `Packages/manifest.json`. Keep the archive with the manifest
when committing or sharing this project.

This local dependency avoids the Git download failure observed when the
Intel Unity Package Manager invoked Apple's Git with ARM-only Command Line
Tools on this machine. It does not fix that machine-wide toolchain issue.
When moving back to a Git dependency, pin this same commit to avoid an
unintentional package upgrade.
