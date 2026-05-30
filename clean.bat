@echo off

:: Delete all bin directories and their contents recursively
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S bin') DO (
    ECHO Deleting directory and contents: "%%G"
    RMDIR /S /Q "%%G"
)

:: Delete all obj directories and their contents recursively
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO (
    ECHO Deleting directory and contents: "%%G"
    RMDIR /S /Q "%%G"
)

:: Delete all log directories and their contents recursively
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S log') DO (
    ECHO Deleting directory and contents: "%%G"
    RMDIR /S /Q "%%G"
)

@echo on

:: rem nuget locals all -clear

