cd ..
dotnet restore
dotnet publish -c Release -o ./publish/bin/NosCore.ReverseProxy
net stop NosCore.ReverseProxy
taskkill /F /IM mmc.exe
sc.exe delete NosCore.ReverseProxy
sc.exe create NosCore.ReverseProxy binpath= %CD%\publish\bin\NosCore.ReverseProxy\NosCore.ReverseProxy.exe start=auto
net start NosCore.ReverseProxy