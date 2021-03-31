@echo off

setlocal

set "dir=%CD%"
for %%I in ("%dir%\..\..") do set "EDMSuite_path=%%~fI"

cd /d %EDMSuite_path%\SEDM4\Libraries\

start SirCachealot

@echo on