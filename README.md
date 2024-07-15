### Simple Build System for UNITY

This is Simple (wizard-like) Build System for UNITY applications _(works on Windows platform)_.

![image](https://github.com/jpetays/StartUnityBuild/blob/main/etc/images/readme_pic01.gif)

'Simple' means that is simple to use after it has been setup and configured properly :-)

Just press 'buttons' **① - ② - ③ - ④ - ⑤** and you are done in no time.

This repo contains several folders for projects that are required to use and test the build system.

**StartUnityBuild** is the UI for the build system. It is a C# Windows Forms desktop application (made using VS 2022).

**PrgBuild** is support library to integrate the build system with the actual UNITY application that is going to be build with this.

**PrgFrame** is general support library for any UNITY applications to use.

**DemoProject** is an UNITY application that can be used to test the build system.

**WebGlBuilds** is simple javascript automation project for creating a HTML page containing list of WebGL builds.  
This is totally optional package to create a history (list) of created WebGL builds to load (for testing).

**etc** contains some supporting folders, for example [Actual Installer](https://www.actualinstaller.com/) configuration file for creating a setup program for the UI.

### Configuration

The build system requires and uses some files and folders for its configuration and operation.  
Most of the files and folders has hard coded names or are given in configuration files itself.

The UNITY application that is going to be built using the build system needs to have both `PrgBuild` and `PrgFrame` DLLs imported.  
They can be copied with their respective .meta files from `DemoProject\Assets\PrgAssemblies` folder for minimal fuss.

Working folder for `StartUnityBuild` executable should be set to be the same as the UNITY project that is going to be built.  
Optionally it can be set from `File->Set Project Folder` menu that makes it global setting to be used if current working folder for `StartUnityBuild` is not actual UNITY project folder.

#### Folders

`.\etc\batchBuild` contains config file(s) for the build system.  
`.\etc\secretKeys` contains all (sensitive) files that are not kept in version control but required for the build.

#### Files

`etc\batchBuild\_auto_build.env` is the config file for the build system.

`ProjectSettings\ProjectSettings.asset` is UNITY project settings asset file that the build system modifies.
`ProjectSettings\ProjectVersion.txt` is used to determine which UNITY version and executable is used for the build.
`Assets\Resources\releasenotes.txt` is UNITY text asset for optional WebGL build history list.

#### Android

`.\etc\secretKeys\AndroidOptions.txt` for Android production build config.

Android build for [Google Play](https://play.google.com/store/apps) using [Google Play Console](https://developer.android.com/distribute/console) requires signing the `.aab` output file using [UNITY keystore](https://docs.unity3d.com/Manual/android-keystore.html) and its related settings like passwords.
These are kept in the `AndroidOptions.txt` file that contains the settings (used in [PlayerSettings](https://docs.unity3d.com/ScriptReference/PlayerSettings.html) 
and [Android PlayerSettings](https://docs.unity3d.com/ScriptReference/PlayerSettings.Android.html)) required to sign the build.

#### GameAnalytics

`.\etc\secretKeys\GameAnalytics_Settings.asset` GameAnalytics production build settings (non versioned).
`.\Assets\Resources\GameAnalytics\Settings.asset` GameAnalytics settings asset file in version control.

[GameAnalytics](https://gameanalytics.com/) uses secret API keys that should not be kept in version control.  
`GameAnalytics_Settings.asset` file with _correct production API keys_ can be put in `.\etc\secretKeys\` folder
and it will copied over original versioned file before UNITY build so that the built product has correct API keys asset inside.

### StartUnityBuild

`StartUnityBuild` has wizard style workflow to create a UNITY build for one or more target platforms.

Supported platforms are Android, WebGL, Win64.

Supported third party integrations are: GameAnalytics.

Workflow goes in following steps:
* **① Git pull** to update local project folder to latest from version control.
* **② Update Build** to update automatically UNITY application product version and related files for the build.
* **③ Git push** to commit and push changes made to the project for the build back to version control.
* **④ Start Build** to start the UNITY application build using UNITY executable (in batch mode) to do this.
* **⑤ Post Process** is optional tasks to do after build(s). Currently used for WebGL builds.

#### Touching files during build

All version controlled files that are changed during build are reverted back to their original (pre-build) state after build.  
Currently this requires manual configuration in `_auto_build.env` file.

### PrgBuild

`PrgBuild` is UNITY support library that actually invokes UNITY [BuildPipeline](https://docs.unity3d.com/ScriptReference/BuildPipeline.html) to do the build for one platform at a time.  
This library is Editor only and can not be included in final built application.

### PrgFrame

`PrgFrame` is UNITY support library containing general utilies for any UNITY application.  
This library can  be included in final built application.

### DemoProject

`DemoProject` is simple one scene project to test the build system and libraries in real world use case.

### WebGlBuilds

`WebGlBuilds` contains example HTML page and javascript to create a list of builds from JSON file (that can be created in WebGL post processing phase by the build system).

### Actual Installer

Actual Installer can be used to create a installation program for `StartUnityBuild` (the build system).

### Example configuration file

Below is example configuration file for WebGL build (with GameAnalytics).

```
# Supported replacement variable names:
# $UNITY_VERSION$   = Unity version from ProjectVersion.txt for unityPath
# $BUILD_TARGET$    = Current build target name (for some copy options)
# $UNIQUE_NAME$     = Unique 'build name' to create output directory (for some copy options)
#
# Comma separated list of auto build options for these targets: Android, WebGL, Win64
#
buildTargets=WebGL
unityPath=C:\Program Files\Unity\Hub\Editor\$UNITY_VERSION$\Editor\Unity.exe
#
# Copy options BEFORE build
#
before.copy.1.source=.\etc\secretKeys\GameAnalytics_Settings.asset
before.copy.1.target=.\Assets\Resources\GameAnalytics\Settings.asset
#
# Revert options AFTER build
#
after.revert.1.file=ProjectSettings\ProjectSettings.asset
after.revert.2.file=Assets\Resources\GameAnalytics\Settings.asset
```
_This part contains configuration required for automatic build history._
```
#
# Copy options AFTER build ($BUILD_TARGET$ target specific)
#
after.copy.1.WebGL.sourceDir=.\build$BUILD_TARGET$
after.copy.1.WebGL.targetDir=.\webserver\www\demo-$UNIQUE_NAME$
#
# WebGL build specific
#
webgl.build.history.json=.\webserver\www\builds\build.history.json
```
