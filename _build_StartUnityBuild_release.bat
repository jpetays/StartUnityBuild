@echo off
set MSBUILD="%ProgramFiles%\\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
rem set OPTIONS=-detailedSummary:[True]
pushd StartUnityBuild
%MSBUILD% %OPTIONS% StartUnityBuild.csproj  /property:Configuration=Release
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
:done
if "%1" == "" pause
