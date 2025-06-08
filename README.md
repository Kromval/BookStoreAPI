# BookStoreAPI

Веб-API интернет-магазина книг на ASP.NET Core. Проект включает серверную часть, тесты и подробную документацию.

## Структура репозитория

```
BookStoreAPI/
├── BookStoreApi/         # исходный код веб-API
├── BookStoreApi.Tests/   # модульные тесты xUnit
├── Docs/                 # документация (см. Docs/README.md)
└── BookStoreApi.sln      # решение Visual Studio
```

## Быстрый запуск

1. Перейдите в каталог `BookStoreApi`.
2. Создайте локальные настройки при необходимости (`appsettings.Development.json`).
3. Запустите сервис командой:
   ```bash
   dotnet run
   ```
4. Swagger будет доступен по адресу `https://localhost:5001/swagger`.

Для запуска базы данных можно использовать `docker-compose.yml` из каталога `BookStoreApi`.

## Документация

Все русскоязычные материалы находятся в каталоге [Docs](Docs/). Начните с [Docs/README.md](Docs/README.md), где приведено содержание и ссылки на отдельные файлы.
