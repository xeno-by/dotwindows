@echo off
cd /D "%PROJECTS%\cccp" 
call sbt stage
@pause