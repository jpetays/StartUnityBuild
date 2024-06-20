@echo off
set SOURCE=.\StartUnityBuild\bin\Release\net8.0-windows
set TARGET=..\..\StartUnityBuild\net8.0-windows
set RELEASE=.\StartUnityBuild\release\StartUnityBuild.zip

robocopy %SOURCE% %TARGET% *.* /V
if exist %RELEASE% (
	del /Q %RELEASE%
)
"C:\Program Files\7-Zip\7z.exe" a %RELEASE% %TARGET%
echo.
pause