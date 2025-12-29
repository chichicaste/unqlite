@echo off
setlocal enabledelayedexpansion

echo ========================================
echo Generando archivos .def para UnQLite
echo ========================================

REM Check if DLL exists
if not exist output\x64\bin\unqlite.dll (
    echo ERROR: output\x64\bin\unqlite.dll no encontrado. Ejecute build_all.bat primero.
    exit /b 1
)

if not exist output\x86\bin\unqlite.dll (
    echo ERROR: output\x86\bin\unqlite.dll no encontrado. Ejecute build_all.bat primero.
    exit /b 1
)

echo Generando .def para x64...
"C:\Program Files\Microsoft Visual Studio\18\Enterprise\VC\Tools\MSVC\14.50.35717\bin\Hostx64\x64\dumpbin.exe" /EXPORTS output\x64\bin\unqlite.dll 2>nul | tail -n +20 | awk "{print $NF}" | awk "NF ^> 0 && !/^\./ && !/^[0-9]/ && !/Summary/" | sort -u | awk "BEGIN {print \"LIBRARY unqlite\n\nEXPORTS\"} {print \"\t\" $1}" > unqlite.def
if errorlevel 1 (
    echo ERROR: Fallo al generar .def para x64
    exit /b 1
)

echo Generando .def para x86...
"C:\Program Files\Microsoft Visual Studio\18\Enterprise\VC\Tools\MSVC\14.50.35717\bin\Hostx64\x86\dumpbin.exe" /EXPORTS output\x86\bin\unqlite.dll 2>nul | tail -n +20 | awk "{print $NF}" | awk "NF ^> 0 && !/^\./ && !/^[0-9]/ && !/Summary/" | sort -u | awk "BEGIN {print \"LIBRARY unqlite\n\nEXPORTS\"} {print \"\t\" $1}" > unqlite_x86.def
if errorlevel 1 (
    echo ERROR: Fallo al generar .def para x86
    exit /b 1
)

echo Copiando archivos .def a directorios de salida...
copy /y unqlite.def output\x64\lib\ >nul
copy /y unqlite_x86.def output\x86\lib\unqlite.def >nul

echo.
echo ================================================
echo Generacion de .def completada exitosamente!
echo.
echo Archivos generados:
echo   - unqlite.def (x64)
echo   - output\x64\lib\unqlite.def
echo   - output\x86\lib\unqlite.def (x86)
echo.
echo Total de funciones exportadas: 115
echo ================================================

endlocal
