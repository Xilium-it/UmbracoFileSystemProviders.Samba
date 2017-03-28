SET curr=.\

cmd /c C:\CommandScripts\Scripts\ASP.NET_UpdateDll.cmd "Xilium.Lib" "release" "%curr%"

cmd /c C:\CommandScripts\Scripts\ASP.NET_UpdateDll.cmd "Xilium.WebLib" "release" "%curr%"

cmd /c C:\CommandScripts\Scripts\ASP.NET_UpdateDll2.cmd "Xilium.UmbLib\Xilium.UmbLib.U7_4_3\Xilium.UmbLib" "Xilium.UmbLib.U7_4_3" "release" "%curr%"

del "Xilium.Lib - DEBUG.txt"
del "Xilium.Lib - RELEASE.txt"
@ECHO Xilium.Lib: RELEASE version - %date% %time% >"Xilium.Lib - RELEASE.txt"

SET curr=
