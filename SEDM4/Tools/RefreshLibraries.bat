@echo off

setlocal

echo This is %COMPUTERNAME%
set "dir=%CD%"
for %%I in ("%dir%\..\..") do set "EDMSuite_path=%%~fI"

echo Using EDMSuite path "%EDMSuite_path%"

echo Copying from SirCahcealot build directory to SEDM4 Libraries directory
del "%EDMSuite_path%\SEDM4\Libraries\*.dll" /S /Q
del "%EDMSuite_path%\SEDM4\Libraries\*.exe" /S /Q
xcopy "%EDMSuite_path%\SirCachealot\bin\EDMAnalysis\*.dll" "%EDMSuite_path%\SEDM4\Libraries" /E /Y
xcopy "%EDMSuite_path%\SirCachealot\bin\EDMAnalysis\*.exe" "%EDMSuite_path%\SEDM4\Libraries" /E /Y

pause
