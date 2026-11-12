@echo off
chcp 65001 > nul
setlocal enabledelayedexpansion

echo === Сборка проекта ===
dotnet build CargoVisualizer.slnx --nologo -v quiet
if %ERRORLEVEL% neq 0 (
    echo Ошибка сборки проекта!
    exit /b 1
)

set APP=dotnet run --no-build --project src/CargoVisualizer --

echo.
echo [TEST 01] Запуск по умолчанию (config.csv из корня)
%APP%
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 02] Валидный конфиг (удаленный репозиторий)
%APP% tests/stage1/valid_remote.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 03] Валидный конфиг (тестовый режим)
%APP% tests/stage1/valid_test_mode.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 04] Ошибка: файл не найден
%APP% tests/stage1/not_exists.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 05] Ошибка: превышено число аргументов CLI
%APP% tests/stage1/valid_remote.csv extra_arg.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 06] Ошибка: пропущен обязательный ключ (Version)
%APP% tests/stage1/err_missing_key.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 07] Ошибка: пустое имя пакета (Name)
%APP% tests/stage1/err_empty_name.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 08] Ошибка: некорректный Url при IsTestMode=false
%APP% tests/stage1/err_invalid_url.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 09] Ошибка: некорректный флаг IsTestMode
%APP% tests/stage1/err_invalid_mode.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 10] Ошибка: пустая версия (Version)
%APP% tests/stage1/err_empty_version.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 11] Ошибка: недопустимые символы в OutputFileName
%APP% tests/stage1/err_invalid_output.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 12] Ошибка: отрицательная глубина MaxDepth
%APP% tests/stage1/err_negative_depth.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 13] Ошибка: нечисловая глубина MaxDepth
%APP% tests/stage1/err_nan_depth.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 14] Ошибка: нарушение формата CSV (нет запятой)
%APP% tests/stage1/err_bad_syntax.csv
echo Exit code: !ERRORLEVEL!
