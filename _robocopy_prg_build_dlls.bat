@echo off
set SOURCE=.\PrgBuild\bin\Debug\netstandard2.1
set TARGET=.\DemoProject\Assets\PrgAssemblies
robocopy %SOURCE% %TARGET% Prg*.dll
pause
