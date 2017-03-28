SET curr=.\

cmd /c C:\CommandScripts\Scripts\ASP.NET_UpdateProjectDll.cmd "Xilium.ImageFlower\source" "Xilium.ImageFlower" "debug" "%curr%"

del "Xilium.ImageFlower - DEBUG.txt"
del "Xilium.ImageFlower - RELEASE.txt"
@ECHO Xilium.ImageFlower: DEBUG version - %date% %time% >"Xilium.ImageFlower - DEBUG.txt"

SET curr=
