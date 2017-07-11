REM Build Console App
pushd ConsoleApp1
msbuild /t:Restore
msbuild /t:Build
popd

REM Build .NET Core app & library
pushd CoreApp1
msbuild /t:Restore /p:RuntimeIdentifier=win-x64
msbuild /t:Build /p:RuntimeIdentifier=win-x64
msbuild /t:Publish /p:RuntimeIdentifier=win-x64
popd

REM Run it.
.\ConsoleApp1\bin\Debug\ConsoleApp1.exe .\CoreApp1\bin\Debug\netcoreapp2.0\win-x64\publish\ClassLibrary1.dll