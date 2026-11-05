#!/usr/bin/env bash
set -euo pipefail
if [[ $# != 3 ]]; then echo "Usage: $0 <package-directory> <package-version> <runtime-identifier>" >&2; exit 2; fi
package_dir=$(cd "$1" && pwd)
version=$2
rid=$3
root=$(cd "$(dirname "$0")/../.." && pwd)
dotnet=${DOTNET:-dotnet}
scratch=$(mktemp -d "${TMPDIR:-/tmp}/dap-package.XXXXXXXX")
trap 'rm -rf "$scratch"' EXIT
cp "$root/test/Dap.NativeAot.Smoke/Program.cs" "$root/global.json" "$scratch/"
# The caller controls both values, but reject XML/shell metacharacters rather than interpolating them.
[[ "$version" =~ ^[0-9A-Za-z.+-]+$ && "$package_dir" != *'&'* && "$package_dir" != *'<'* && "$package_dir" != *'"'* ]] || exit 2
cat > "$scratch/NuGet.Config" <<XML
<configuration><packageSources><clear/><add key="local" value="$package_dir"/><add key="nuget.org" value="https://api.nuget.org/v3/index.json"/></packageSources></configuration>
XML
cat > "$scratch/Consumer.csproj" <<XML
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><StartupObject>Program</StartupObject><PublishAot>true</PublishAot><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup>
  <ItemGroup><PackageReference Include="OmniSharp.Extensions.DebugAdapter.Server" Version="$version" /></ItemGroup>
</Project>
XML
(cd "$scratch" && "$dotnet" publish Consumer.csproj -c Release -r "$rid")
"$scratch/bin/Release/net10.0/$rid/publish/Consumer"
