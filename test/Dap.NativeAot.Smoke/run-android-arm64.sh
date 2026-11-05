#!/usr/bin/env bash
set -euo pipefail

# Runs the C# NativeAOT smoke test on an Android ARM64 emulator/device. The tiny C
# executable only loads the test library; it is not part of the debug agent.
root=$(cd "$(dirname "$0")/../.." && pwd)
dotnet=${DOTNET:-dotnet}
sdk=${ANDROID_HOME:-${ANDROID_SDK_ROOT:-}}
ndk=${ANDROID_NDK_HOME:-${ANDROID_NDK_ROOT:-}}
serial=${ANDROID_SERIAL:-}
if [[ -z "$sdk" || -z "$ndk" || -z "$serial" ]]; then
    echo 'Set ANDROID_HOME, ANDROID_NDK_HOME, and ANDROID_SERIAL.' >&2
    exit 2
fi
adb="$sdk/platform-tools/adb"
version=$($dotnet --list-sdks | awk '$1 ~ /^11\./ { version=$1 } END { print version }')
if [[ -z "$version" ]]; then echo 'A .NET 11 Android SDK is required.' >&2; exit 2; fi
case "$(uname -s)" in
    Darwin) toolchain="$ndk/toolchains/llvm/prebuilt/darwin-x86_64/bin/aarch64-linux-android24-clang" ;;
    Linux) toolchain="$ndk/toolchains/llvm/prebuilt/linux-x86_64/bin/aarch64-linux-android24-clang" ;;
    *) echo 'Unsupported build host.' >&2; exit 2 ;;
esac
[[ -x "$adb" && -x "$toolchain" ]] || { echo 'adb or Android NDK compiler was not found.' >&2; exit 2; }

scratch=$(mktemp -d "${TMPDIR:-/tmp}/dap-nativeaot.XXXXXXXX")
remote="/data/local/tmp/$(basename "$scratch")"
cleanup() { "$adb" -s "$serial" shell rm -rf "$remote" >/dev/null 2>&1 || true; rm -rf "$scratch"; }
trap cleanup EXIT
mkdir -p "$scratch/src"
for project in JsonRpc.Generators JsonRpc Dap.Protocol Dap.Shared Dap.Server; do
    rsync -a --exclude bin --exclude obj "$root/src/$project" "$scratch/src/"
done
cp "$root/Directory.Build.props" "$root/Directory.Build.targets" "$root/Directory.Packages.props" "$root/Directory.Packages.supports.props" "$root/NuGet.config" "$root/lsp.snk" "$scratch/"
cp "$root/test/Dap.NativeAot.Smoke/Program.cs" "$scratch/Program.cs"
cat > "$scratch/global.json" <<JSON
{"sdk":{"version":"$version","allowPrerelease":true,"rollForward":"disable"}}
JSON
cat > "$scratch/AndroidSmoke.csproj" <<'XML'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>net11.0-android</TargetFramework><OutputType>Exe</OutputType><NativeLib>Shared</NativeLib><PublishAot>true</PublishAot><SelfContained>true</SelfContained><ImplicitUsings>enable</ImplicitUsings><EnableDefaultCompileItems>false</EnableDefaultCompileItems><Nullable>enable</Nullable><EnablePreviewFeatures>true</EnablePreviewFeatures></PropertyGroup>
  <ItemGroup>
    <Compile Include="Program.cs" />
    <ProjectReference Include="src/Dap.Server/Dap.Server.csproj" />
    <ProjectReference Include="src/Dap.Shared/Dap.Shared.csproj" />
    <ProjectReference Include="src/Dap.Protocol/Dap.Protocol.csproj" />
    <ProjectReference Include="src/JsonRpc/JsonRpc.csproj" />
    <PackageReference Include="Microsoft.Extensions.Logging" />
    <PackageReference Include="System.Reactive" />
    <TrimmerRootAssembly Include="OmniSharp.Extensions.JsonRpc" />
    <TrimmerRootAssembly Include="OmniSharp.Extensions.DebugAdapter" />
    <TrimmerRootAssembly Include="OmniSharp.Extensions.DebugAdapter.Shared" />
    <TrimmerRootAssembly Include="OmniSharp.Extensions.DebugAdapter.Server" />
  </ItemGroup>
</Project>
XML
# The project is isolated from the checkout so SDK 11 cannot alter SDK 10's asset cache.
(cd "$scratch" && "$dotnet" publish AndroidSmoke.csproj -c Release -r android-arm64 "-p:AndroidSdkDirectory=$sdk" "-p:AndroidNdkDirectory=$ndk")
cat > "$scratch/loader.c" <<C
#include <dlfcn.h>
#include <stdio.h>
#include <unistd.h>
int main(void) {
    void *library = dlopen("$remote/libAndroidSmoke.so", RTLD_NOW);
    if (!library) { fprintf(stderr, "dlopen: %s\\n", dlerror()); return 2; }
    int (*run)(void) = (int (*)(void))dlsym(library, "ProbeRun");
    if (!run) { fprintf(stderr, "dlsym: %s\\n", dlerror()); return 3; }
    printf("PROBE_PID:%ld\\n", (long)getpid()); fflush(stdout);
    int result = run();
    printf("PROBE_EXIT:%d\\n", result);
    return result;
}
C
"$toolchain" "$scratch/loader.c" -o "$scratch/loader" -ldl
"$adb" -s "$serial" shell mkdir -p "$remote"
"$adb" -s "$serial" push "$scratch/loader" "$scratch/bin/Release/net11.0-android/android-arm64/native/libAndroidSmoke.so" "$remote/" >/dev/null
"$adb" -s "$serial" shell "$remote/loader" | tee "$scratch/result.txt"
pid=$(sed -n 's/^PROBE_PID:\([0-9]*\).*/\1/p' "$scratch/result.txt" | head -1)
[[ -n "$pid" ]] || { echo 'The test loader did not report a process ID.' >&2; exit 1; }
"$adb" -s "$serial" logcat -d --pid "$pid" -s DOTNET > "$scratch/device.log"
for message in 'initialize successful' 'attach successful' 'stopped event successful' 'breakpoints successful' 'request 4 successful' 'request 5 successful' 'request 6 successful' 'request 7 successful' 'request 8 successful' 'request 9 successful' 'request 10 successful' 'request 11 successful'; do
    if ! grep -Fq "$message" "$scratch/device.log"; then cat "$scratch/device.log" >&2; echo "Missing: $message" >&2; exit 1; fi
done
echo 'Android ARM64 NativeAOT DAP smoke test passed.'
