@echo off

IF NOT "%VS140COMNTOOLS%" == "" (call "%VS140COMNTOOLS%vsvars32.bat")

.paket\paket.bootstrapper.exe
.paket\paket.exe restore

msbuild.exe /tv:14.0 "MONI.sln" /p:configuration=Debug /p:platform="Any CPU" /m /t:Clean,Rebuild
msbuild.exe /tv:14.0 "MONI.sln" /p:configuration=Release /p:platform="Any CPU" /m /t:Clean,Rebuild
