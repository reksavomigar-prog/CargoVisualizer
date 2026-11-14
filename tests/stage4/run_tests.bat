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
echo [TEST 01] Обратные зависимости для D: глубина MaxDepth = 0
%APP% tests/stage4/rev_depth_0.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 02] Обратные зависимости для D: глубина MaxDepth = 1 (только B и C)
%APP% tests/stage4/rev_depth_1.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 03] Обратные зависимости для D: ромбовидный граф и цикл (MaxDepth = 5)
%APP% tests/stage4/rev_diamond_cycle.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 04] Обратные зависимости для неявного листа F (только справа от ':')
%APP% tests/stage4/rev_implicit_leaf.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 05] Пакет A без обратных зависимостей (верхний узел DAG)
%APP% tests/stage4/rev_no_dependents.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 06] Обратные зависимости при взаимном цикле A ^<-^> B
%APP% tests/stage4/rev_mutual_cycle.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 07] Обратные зависимости при самоссылке (петле) C -^> C
%APP% tests/stage4/rev_self_loop.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 08] Многобуквенные имена пакетов: цепочка GG -^> DD -^> BB -^> AA
%APP% tests/stage4/rev_multiletter.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 09] Ошибка: целевой пакет Z отсутствует в репозитории
%APP% tests/stage4/err_not_found_pkg.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 10] Ошибка: файл тестового репозитория не найден
%APP% tests/stage4/err_missing_repo.csv
echo Exit code: !ERRORLEVEL!

echo.
echo [TEST 11] Ошибка: нарушение синтаксиса файла репозитория (нет ':')
%APP% tests/stage4/err_bad_repo_syntax.csv
echo Exit code: !ERRORLEVEL!
