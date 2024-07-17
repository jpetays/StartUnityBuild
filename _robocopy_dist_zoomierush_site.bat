@echo off
set SOURCE=.\dist
set TARGET=C:\Users\petays\Dropbox\tekstit\zoomierush\www\dist
robocopy %SOURCE% %TARGET% *.*
:done
if "%1" == "" pause
