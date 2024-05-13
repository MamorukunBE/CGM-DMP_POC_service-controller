@echo off
rem Params variables
set PROJECT_FILE_PATH=%~1
set PUBLISHED_SERVICE_FILE_PATH=%~2
set SOLUTION_PATH=%~3

rem Extract the repository directory path from our config file
set xmlFilePath=%SOLUTION_PATH%config.xml
for /f "tokens=2 delims==" %%i in ('findstr /c:"RepositoryDirectory" "%xmlFilePath%"') do (
    set dirtyPath=%%i
    echo ----------------
    echo %dirtyPath%
    echo ----------------
    set REPOSITORY_DIRECTORY_PATH=%dirtyPath:~2,-4%
)
rem -----
set PUBLISHED_SERVICE_FILE_NAME=%~nn2
set PUBLISHED_SERVICE_FILE_FULL_NAME=%~nxn2
set REPOSITORY_SERVICE_FILE_PATH=%REPOSITORY_DIRECTORY_PATH%%PUBLISHED_SERVICE_FILE_FULL_NAME%
set PUBLISHED_SERVICE_VERSION_FILE_PATH=%REPOSITORY_DIRECTORY_PATH%%PUBLISHED_SERVICE_FILE_NAME%_version.txt

rem Processes
echo Publishing the service...
dotnet publish %PROJECT_FILE_PATH%
rem -----
echo Copying the published service to the web repository...
xcopy /y %PUBLISHED_SERVICE_FILE_PATH% %REPOSITORY_DIRECTORY_PATH%
rem -----
echo Extracting the published service version...
for /f "tokens=2 delims==" %%I in (
    'wmic datafile where "name='%REPOSITORY_SERVICE_FILE_PATH:\=\\%'" get Version /value'
) do (
    echo %%I > "%PUBLISHED_SERVICE_VERSION_FILE_PATH%"
)
