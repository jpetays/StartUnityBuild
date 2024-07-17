@echo off
set SOURCE=..\PrgBuild\bin\Debug\netstandard2.1
set TARGET=.\Assets\PrgAssemblies
robocopy %SOURCE% %TARGET% Prg*.dll
pause
