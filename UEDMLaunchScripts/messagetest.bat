echo set WshShell = WScript.CreateObject("WScript.Shell") > %tmp%\tmp.vbs
echo WScript.Quit (WshShell.Popup( "You have 10 seconds to Click 'OK'." ,10 ,"Click OK", 0)) >> %tmp%\tmp.vbs
cscript /nologo %tmp%\tmp.vbs
if %errorlevel%==1 (
  echo You Clicked OK
) else (
  echo The Message timed out.
)
del %tmp%\tmp.vbs