@echo off
echo Compilando ChessEduca...
echo.

REM Tentar compilar com dotnet
dotnet build > nul 2>&1
if %ERRORLEVEL% == 0 (
    echo Compilacao com sucesso usando dotnet!
    echo Executavel criado em: bin\Debug\net8.0\ChessEduca.exe
    goto :end
)

REM Tentar com MSBuild do Visual Studio
if exist "C:\Program Files\Microsoft Visual Studio\2022\Preview\MSBuild\Current\Bin\MSBuild.exe" (
    echo Tentando com MSBuild...
    "C:\Program Files\Microsoft Visual Studio\2022\Preview\MSBuild\Current\Bin\MSBuild.exe" ChessEduca.csproj /p:Configuration=Release /nologo /verbosity:quiet
    if %ERRORLEVEL% == 0 (
        echo Compilacao com sucesso usando MSBuild!
        echo Executavel criado em: bin\Release\net8.0\ChessEduca.exe
        goto :end
    )
)

echo.
echo ERRO: Nao foi possivel compilar o projeto.
echo.
echo Por favor, certifique-se de que o .NET SDK 8.0 esta instalado corretamente.
echo Voce pode baixa-lo em: https://dotnet.microsoft.com/download/dotnet/8.0
echo.
echo Como alternativa, voce pode abrir o projeto no Visual Studio e compilar por la.

:end
pause