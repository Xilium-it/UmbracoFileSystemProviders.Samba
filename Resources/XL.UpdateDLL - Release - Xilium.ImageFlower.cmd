SET curr=.\

cmd /c C:\CommandScripts\Scripts\ASP.NET_UpdateProjectDll.cmd "Xilium.ImageFlower\source" "Xilium.ImageFlower" "release" "%curr%"

del "Xilium.ImageFlower - DEBUG.txt"
del "Xilium.ImageFlower - RELEASE.txt"
@ECHO Xilium.ImageFlower: RELEASE version - %date% %time% >"Xilium.ImageFlower - RELEASE.txt"

SET curr=
