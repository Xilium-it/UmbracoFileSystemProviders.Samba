SET curr=.\

cmd /c C:\CommandScripts\Scripts\ASP.NET_UpdateDll.cmd "Xilium.Lib" "debug" "%curr%"

cmd /c C:\CommandScripts\Scripts\ASP.NET_UpdateDll.cmd "Xilium.WebLib" "debug" "%curr%"

cmd /c C:\CommandScripts\Scripts\ASP.NET_UpdateDll2.cmd "Xilium.UmbLib\Xilium.UmbLib.U7_4_3\Xilium.UmbLib" "Xilium.UmbLib.U7_4_3" "debug" "%curr%"

del "Xilium.Lib - DEBUG.txt"
del "Xilium.Lib - RELEASE.txt"
@ECHO Xilium.Lib: DEBUG version - %date% %time% >"Xilium.Lib - DEBUG.txt"

SET curr=
