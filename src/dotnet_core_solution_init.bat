@echo off

set _root=%~dp0

set /p _solution=solution name: 
set _solution_path=%_root%%_solution%

set /p _project=project name: 
set _project_path=%_root%%_solution%\%_project%

set /p _project_type=project type(console): 
set _test_path=%_root%%_solution%\%_project%.Tests
::set /p _test_type=test type(xunit): 
set _test_type=xunit

dotnet new solution -o %_solution_path%

dotnet new %_project_type% -o %_project_path%
dotnet sln %_solution_path%\ add %_project_path%

dotnet new %_test_type% -o %_test_path%
dotnet sln %_solution_path%\ add %_test_path%
dotnet add %_test_path% reference %_project_path%

echo.

@echo off

echo. & pause
