@echo off

set SOURCE=.\StartUnityBuild\bin\Release\net8.0-windows
set TARGET=.\temp\StartUnityBuild
robocopy %SOURCE% %TARGET% *.* /V /XD logs

set DIST_ZIPPED=.\dist\StartUnityBuild.zip
if exist %DIST_ZIPPED% (
	del /Q %DIST_ZIPPED%
)
"C:\Program Files\7-Zip\7z.exe" a %DIST_ZIPPED% %TARGET%

set DIST_ZIPPED_EXE=.\dist\StartUnityBuild.exe
if exist %DIST_ZIPPED_EXE% (
	del /Q %DIST_ZIPPED_EXE%
)
"C:\Program Files\7-Zip\7z.exe" a -sfx %DIST_ZIPPED_EXE% %TARGET%

pause
