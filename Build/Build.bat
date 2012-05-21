@echo off

pushd "%~dp0"

powershell .\Support\Build.ps1

pause

popd
