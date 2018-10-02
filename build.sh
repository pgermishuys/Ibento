#!/usr/bin/env bash
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
DOTNET_INSTALL_PATH=$SCRIPT_DIR/.dotnet
echo "${0##*/}": Installing DotNetCore.
curl -sSL https://dot.net/dotnet-install.sh | bash /dev/stdin --channel current --version 2.1.401 --install-dir $DOTNET_INSTALL_PATH

echo "${0##*/}": Running Build.
dotnet run -p build/build.csproj --project "$@"
