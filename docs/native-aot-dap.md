# NativeAOT DAP server

The DAP server can be published with NativeAOT using the net8.0 or net10.0
assets. The server package supplies the linker roots needed by DryIoc and DAP
handler discovery. Rooting these assemblies increases the native binary size;
it does not imply that every use of reflection by a user-defined handler is
safe. Custom DTOs, converters and dynamically discovered handlers still need
their own source-generated JSON metadata and linker preservation.

The smoke test covers initialize, attach with a UTF-8 extension property,
stopped events, source breakpoints, threads, stack frames, scopes, variables,
evaluation, stepping and invalid-request responses. This is a protocol test; it
does not attach to a CLR debuggee. Run it on a supported NativeAOT host with:

```sh
dotnet publish test/Dap.NativeAot.Smoke/Dap.NativeAot.Smoke.csproj -c Release -r <host-rid>
test/Dap.NativeAot.Smoke/bin/Release/net10.0/<host-rid>/publish/Dap.NativeAot.Smoke
```

The test project references the source projects, so it explicitly supplies the
same linker roots that the NuGet package supplies through `buildTransitive`.
`verify-package.sh <package-directory> <version> <host-rid>` instead verifies a
fresh package consumer without explicit roots.

To exercise Android ARM64, install the .NET 11 Android workload, an Android SDK,
and an NDK, start an ARM64 emulator or connect a device, and run:

```sh
DOTNET=/path/to/dotnet ANDROID_HOME=/path/to/sdk ANDROID_NDK_HOME=/path/to/ndk \
  ANDROID_SERIAL=<adb-serial> test/Dap.NativeAot.Smoke/run-android-arm64.sh
```

The Android script builds an isolated temporary NativeAOT shared library. A
small native test loader calls its exported C# entry point; that loader is not
part of a debug agent or a distribution. The script checks each result in
logcat and removes its temporary device and host files.

The `Publish lextudio DAP packages` workflow is manually dispatched. Each run
uses its GitHub run ID and attempt to produce a unique prerelease version,
runs the DAP/JSON-RPC suites, verifies an actual NativeAOT package consumer,
and publishes the five-package DAP dependency closure to GitHub Packages.
The workflow does not run the Android smoke test; run that separately before
shipping a version intended for Android.
