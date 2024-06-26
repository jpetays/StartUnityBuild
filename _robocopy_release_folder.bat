rem @echo off
set SOURCE=.\StartUnityBuild\bin\Release\net8.0-windows
set TARGET=.\temp\StartUnityBuild
set DIST_ZIPPED=.\dist\StartUnityBuild.zip

robocopy %SOURCE% %TARGET% *.* /V /XD logs

if exist %DIST_ZIPPED% (
	del /Q %DIST_ZIPPED%
)
@echo on
"C:\Program Files\7-Zip\7z.exe" a %DIST_ZIPPED% %TARGET%
@echo off
echo.
pause
