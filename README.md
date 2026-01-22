# MVCLibrary (ASP.NET MVC + SQL Server Express)

Веб-приложение библиотека на ASP.NET MVC с подключением к SQL Server Express.  
Реализовано управление книгами и авторами, а также связь книга–авторы через промежуточную таблицу.

## Функционал

- Просмотр списка книг
- Добавление книги (выбор одного/нескольких авторов)
- Редактирование книги (в том числе изменение авторов)
- Удаление книги
- Просмотр списка авторов
- Добавление автора
- Редактирование автора
- Удаление автора (запрещено, если автор привязан к книге)

## Технологии

- ASP.NET Core MVC
- SQL Server Express
- ADO.NET (SqlConnection / SqlCommand)
- Stored Procedures

## Как запустить проект

### 1) Требования

Установить:
- Visual Studio 2022 (или Rider)
- .NET SDK (версия проекта, например .NET 6 / .NET 7 / .NET 8)
- SQL Server Express
- SQL Server Management Studio — желательно

### 2) Создать базу данных

1. SQL Server Management Studio 
2. Подключись к серверу:localhost\SQLEXPRESS

3. Открой файл `Library.sql` из репозитория  
4. Выполни скрипт

После этого появится база данных `Library`, таблицы и все процедуры.

### 3) Настроить строку подключения

Открой файл:
`appsettings.json`
Проверь, что строка подключения выглядит так:
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=Library;Integrated Security=True;Encrypt=False"
}
```
## Запустить проект
Открыть проект в Visual Studio → нажать Run





## Структура проекта

-Controllers/ — контроллеры Books и Autors
-Models/ — модели Book / Autor + модели для форм
-Data/ — работа с БД (ADO.NET, вызов процедур)
-Views/ — Razor страницы
-Library.sql — скрипт создания БД, таблиц, процедур
