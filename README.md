### Режим обратных зависимостей (Этап 4)

При включенном тестовом режиме (`IsTestMode,true`) утилита выполняет поиск **обратных зависимостей** для пакета, указанного в параметре `Name` (определяет все пакеты репозитория, которые напрямую или транзитивно зависят от целевого пакета, с учетом `MaxDepth` и циклических связей).

#### Пример конфигурации (`tests/stage4/rev_diamond.csv`)
```csv
Name,D
Url,tests/stage4/repo_diamond_cycle.txt
IsTestMode,true
Version,1.0.0
OutputFileName,reverse_graph.png
MaxDepth,3