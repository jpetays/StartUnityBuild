@echo off
set MSBUILD="%ProgramFiles%\\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
rem set OPTIONS=-detailedSummary:[True]
pushd PrgBuild
%MSBUILD% %OPTIONS% PrgBuild.csproj
set RETVALUE=%ERRORLEVEL%
echo RETVALUE=%RETVALUE%
popd
if not "%RETVALUE%" == "0" (
    echo.
    echo Build failed %RETVALUE%
    echo.
    goto :done
)
echo.
echo Build success %RETVALUE%
echo.
set SOURCE=.\PrgBuild\bin\Debug\netstandard2.1
set TARGET=.\DemoProject\Assets\PrgAssemblies
robocopy %SOURCE% %TARGET% Prg*.dll
:done
if "%1" == "" pause
