dotnet publish -c Release -r win-x64 /p:PublishTrimmed=true /p:TrimMode=Link --self-contained
warp-packer -a windows-x64 -i bin/Release/net5.0/win-x64/publish -e ZY1280Monitor.exe -o ZY1280Monitor.exe