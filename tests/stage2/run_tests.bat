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
echo [TEST 01] Валидный пакет с зависимостями (serde 1.0.195)
%APP% tests/stage2/valid_serde.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 02] Валидный пакет с множеством зависимостей (tokio 1.36.0)
%APP% tests/stage2/valid_tokio.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 03] Валидный пакет без прямых зависимостей (scopeguard 1.2.0)
%APP% tests/stage2/valid_no_deps.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 04] Ошибка: файл конфигурации не найден
%APP% tests/stage2/not_exists.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 05] Ошибка: превышено число аргументов CLI
%APP% tests/stage2/valid_serde.csv extra_arg.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 06] Ошибка: пакет не найден в реестре (404 Not Found)
%APP% tests/stage2/err_not_found_pkg.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 07] Ошибка: версия пакета не найдена в реестре (404 Not Found)
%APP% tests/stage2/err_not_found_ver.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 08] Ошибка: недоступный хост / сбой соединения
%APP% tests/stage2/err_unreachable_host.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 09] Ошибка: некорректный URL реестра (не API Cargo)
%APP% tests/stage2/err_wrong_registry.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 10] Ошибка: невалидный формат URL в конфигурации
%APP% tests/stage2/err_invalid_url.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 11] Ошибка: пустая версия пакета
%APP% tests/stage2/err_empty_version.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 12] Ошибка: пропущен обязательный ключ Url
%APP% tests/stage2/err_missing_key.csv
echo Exit code: !ERRORLEVEL!
