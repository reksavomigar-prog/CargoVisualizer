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
echo [TEST 01] Тестовый граф: глубина MaxDepth = 0 (только корень A)
%APP% tests/stage3/valid_depth_0.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 02] Тестовый граф: глубина MaxDepth = 1 (первый уровень)
%APP% tests/stage3/valid_depth_1.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 03] Тестовый граф: глубина MaxDepth = 2 (ромбовидная связь B-^>D и C-^>D)
%APP% tests/stage3/valid_depth_2.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 04] Тестовый граф: полный обход с циклом A-^>C-^>E-^>A (MaxDepth = 5)
%APP% tests/stage3/valid_cycle_full.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 05] Тестовый граф: взаимный цикл A^<-^>B и петля C-^>C
%APP% tests/stage3/valid_self_cycle.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 06] Тестовый граф: многобуквенные пакеты (AA, BB) и неявные листья
%APP% tests/stage3/valid_multiletter.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 07] Удаленный реестр crates.io: транзитивный граф serde (MaxDepth = 2)
%APP% tests/stage3/valid_remote_serde.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 08] Ошибка: файл тестового репозитория не найден
%APP% tests/stage3/err_missing_repo.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 09] Ошибка: стартовый пакет Z отсутствует в тестовом репозитории
%APP% tests/stage3/err_root_not_found.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 10] Ошибка: строчные буквы в имени пакета тестового репозитория
%APP% tests/stage3/err_lowercase_pkg.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 11] Ошибка: дублирование описания пакета в тестовом репозитории
%APP% tests/stage3/err_duplicate_pkg.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 12] Ошибка: нарушение синтаксиса строки тестового репозитория (нет ':')
%APP% tests/stage3/err_bad_repo_syntax.csv
echo Exit code: !ERRORLEVEL!
