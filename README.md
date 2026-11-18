# Wastegate.Lybrary.HotKey

![Логотип Wastegate.Lybrary.HotKey](Images/west.png)

Платформенно-независимая библиотека для регистрации сочетаний клавиш и вызова связанных с ними обработчиков. Целевая платформа — .NET Standard 2.0.

## Использование

Создайте `HotKeyManager`, зарегистрируйте сочетание и передавайте ему события клавиатуры из вашего UI-фреймворка. `TryHandle` возвращает `true`, если сочетание зарегистрировано, и вызывает обработчик. Регистрация снимается через `Dispose`.

```csharp
using System;
using Wastegate.Lybrary.HotKey;

var hotKeys = new HotKeyManager();
var save = hotKeys.Register(
    new HotKeyGesture(ConsoleKey.S, HotKeyModifiers.Control),
    () => SaveDocument());

// В обработчике клавиатуры приложения:
var handled = hotKeys.TryHandle(
    new HotKeyGesture(ConsoleKey.S, HotKeyModifiers.Control));

// При завершении работы:
save.Dispose();
hotKeys.Dispose();
```

Доступные модификаторы: `Alt`, `Control`, `Shift` и `Windows`. Одно и то же сочетание нельзя зарегистрировать повторно одновременно. Обработчики вызываются вне внутренней блокировки менеджера; исключения из обработчиков передаются вызывающему коду.

Библиотека не перехватывает ввод операционной системы и не регистрирует глобальные системные горячие клавиши: приложение передает ей уже распознанные события клавиатуры.

## Сборка и публикация

- Pull request в `dev` запускает сборку и упаковку NuGet-пакета с dev-версией. После успешной сборки workflow создает prerelease в разделе **Releases**. Для каждого обновления PR создается отдельный prerelease.
- Push в `release` собирает пакет стабильной версии, создает GitHub Release и публикует пакет в NuGet.org.
- Для публикации в NuGet.org добавьте в настройках репозитория GitHub Actions секрет `NUGET_API_KEY` с API-ключом NuGet.org.
- Чтобы автоматически создавать dev-релизы, workflow `publish-dev-release.yml` должен быть доступен в ветке по умолчанию (`master`); PR-сборка запускается после появления `build-pr.yml` в целевой ветке `dev`.

## Gitflow

- `master` — стабильная ветка.
- `dev` — интеграционная ветка; изменения поступают через pull request.
- `release` — подготовка стабильной публикации; каждый push в эту ветку публикует новый пакет.
- Рабочие ветки создаются от `dev` и вливаются обратно через pull request. Подготовленный релиз переносится в `master`; после релиза изменения синхронизируются обратно в `dev`.

