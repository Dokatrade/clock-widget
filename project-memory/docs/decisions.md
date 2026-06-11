# Decisions

## 2026-06-10 — Project Memory
Решили использовать корневой `AGENTS.md` как точку входа для Codex, а `project-memory/` как папку проектной памяти:
- `project-memory/HANDOFF.md` = актуальный срез проекта
- `project-memory/TODO.md` = очередь задач
- `project-memory/docs/decisions.md` = журнал решений
- `project-memory/docs/chat-notes.md` = сжатые заметки из переписки

Причина:
Корневой `AGENTS.md` автоматически обнаруживается Codex при старте работы в репозитории, а остальные memory-файлы отделены от инструкций и не засоряют корень проекта.

## 2026-06-10 — Memory File Formats
Решили держать `project-memory/TODO.md` в формате `Now` / `Next` / `Later` / `Done`, а `project-memory/docs/decisions.md` вести как датированный журнал решений.

Причина:
Пользователь явно предложил эти шаблоны, и они делают проектную память проще для следующего чата.

## 2026-06-10 — Language
Решили отвечать пользователю на русском, если он явно не попросит другой язык.

Причина:
Вся рабочая коммуникация по проекту идёт на русском.

## 2026-06-10 — SSD Writes
Решили не добавлять периодические или фоновые записи на диск. Tray Show/Hide остаётся временным UI-состоянием и не вызывает сохранение настроек.

Причина:
Пользователь явно заботится о лишних записях на SSD.

## 2026-06-10 — Tray Implementation
Решили использовать стандартный WinForms `NotifyIcon` для tray menu и вынести эту логику в `TrayIconController`.

Причина:
Tray нужен для управления виджетом без taskbar-кнопки, а стандартный `NotifyIcon` не требует новых production dependencies.

## 2026-06-10 — Pomodoro Minimal UI
Решили не возвращать нижние текстовые статусы Pomodoro (`Focus`, `Break`, `Paused`) без отдельного запроса пользователя.

Причина:
Пользователь удалил эти подписи намеренно и подтвердил минималистичный UI.

## 2026-06-10 — Pomodoro Notifications Deferred
Решили пока не добавлять системные уведомления Pomodoro.

Причина:
Пользователь явно сказал, что уведомления сейчас не нужны.

## 2026-06-10 — Dependencies
Решили не добавлять новые production dependencies без явного подтверждения пользователя.

Причина:
Проект небольшой, и текущие задачи можно решать стандартными WPF/WinForms/.NET средствами.

## 2026-06-10 — Manifest Tracking
Решили отслеживать `ClockWidget/app.manifest` в git, несмотря на общее правило `.gitignore` для `*.manifest`.

Причина:
`ClockWidget.csproj` ссылается на manifest, поэтому чистый checkout должен содержать этот файл.

## 2026-06-10 — DPI Configuration
Решили держать DPI mode в `ClockWidget.csproj` через `ApplicationHighDpiMode`, а не в manifest.

Причина:
После включения WinForms SDK выдал предупреждение WFAC010 и рекомендовал перенести DPI-настройку из manifest.

## 2026-06-10 — Snap-To-Edge Behavior
Решили, что `SnapToScreenEdges` должен именно прилипать к краям, а не создавать внутренний отступ.

Причина:
Пользователь заметил, что первая реализация выглядела как отталкивание от края и почти не отличалась при включённой/выключенной галочке.

## 2026-06-10 — MainWindow Decomposition
Решили постепенно выносить отдельные ответственности из `MainWindow.xaml.cs`.

Причина:
Файл стал перегружен после настроек, Pomodoro, tray и позиционирования окна.

Уже вынесено:
- settings persistence -> `SettingsStore`
- tray icon/menu -> `TrayIconController`
- window placement/snap -> `WindowPlacementService`
- Pomodoro state transitions -> `PomodoroController`
- startup setting read/apply -> `StartupSettingsService`
- display tick scheduling -> `DisplayTickScheduler`
- display formatting/progress model -> `WidgetDisplayFormatter`
- settings dialog creation/apply dispatch -> `SettingsDialogController`
- Pomodoro display/session commands -> `PomodoroSession`

## 2026-06-10 — Pomodoro Controller Extraction
Решили вынести фазу, остаток времени, старт/паузу/сброс и завершение Pomodoro-фаз в `PomodoroController`, оставив `MainWindow` владельцем отображения, звуков и меню.

Причина:
Это уменьшает перегруженность `MainWindow.xaml.cs` без изменения минимального Pomodoro UI и без новых production dependencies.

## 2026-06-10 — Startup Settings Service
Решили вынести чтение и применение Windows startup-настройки из `MainWindow.xaml.cs` в `StartupSettingsService`, оставив `MainWindow` только владельцем предупреждения пользователю при ошибке.

Причина:
Это продолжает постепенную декомпозицию `MainWindow.xaml.cs`, изолирует registry try/catch вокруг `StartupManager` и не добавляет новых зависимостей.

## 2026-06-10 — Display Tick Scheduler
Решили вынести владение `DispatcherTimer` и расчет адаптивного следующего тика из `MainWindow.xaml.cs` в `DisplayTickScheduler`.

Причина:
Это уменьшает размер и ответственность `MainWindow.xaml.cs`, сохраняет текущую экономную частоту обновлений и делает расчет интервала проще для будущих тестов.

## 2026-06-10 — Display Brushes
Решили переиспользовать frozen `SolidColorBrush` для постоянных display-цветов и создавать новую кисть только для focus Pomodoro progress, где цвет действительно меняется по прогрессу.

Причина:
Это снижает аллокации на частых обновлениях виджета без изменения визуального поведения и без новых зависимостей.

## 2026-06-10 — Pomodoro Clock Injection
Решили заменить прямые обращения `DateTime.Now` внутри `PomodoroController` на injectable `IClock` с production-default `SystemClock.Instance`.

Причина:
Это делает Pomodoro state machine пригодной для детерминированных тестов без изменения UI-поведения и без новых production dependencies.

## 2026-06-10 — Test Project
Решили добавить отдельный `ClockWidget.Tests` project на xUnit v3 (`xunit.v3.mtp-v2` 3.2.2), подключенный к solution, и открыть internal-типы через `InternalsVisibleTo("ClockWidget.Tests")`.

Причина:
Нужны быстрые детерминированные проверки чистой логики (`PomodoroController`, `WidgetSettings.Normalize()`) без запуска WPF UI. Зависимость является test-only и не добавляется в production app.

## 2026-06-10 — SettingsStore Test Path
Решили добавить internal-конструктор `SettingsStore(string settingsDirectory)` для тестов, не меняя production-конструктор с `%APPDATA%\ClockWidget`.

Причина:
Это позволяет проверять сохранение, загрузку и no-write behavior на временной папке без записи в реальные пользовательские настройки.

## 2026-06-10 — Widget Display Formatter
Решили вынести форматирование текста времени/даты, Pomodoro duration, progress ratio и progress color из `MainWindow.xaml.cs` в `WidgetDisplayFormatter`.

Причина:
`MainWindow` должен применять готовую модель к WPF controls, а чистая display-логика должна тестироваться отдельно без перехода на полноценный MVVM.

## 2026-06-10 — Settings Dialog Controller
Решили вынести создание `SettingsWindow`, подписку на `SettingsApplied` и dispatch итоговых Apply/OK-настроек из `MainWindow.xaml.cs` в `SettingsDialogController`.

Причина:
Это убирает дублированную Apply/OK-обвязку из `MainWindow`, не меняя поведение окна настроек и не вводя новые зависимости.

## 2026-06-10 — Reset Position Command
Решили добавить `Reset position` в context menu и tray menu. Команда работает даже при `Lock position`, ставит виджет в правый верхний угол рабочей области текущего монитора и сохраняет новую позицию.

Причина:
Это быстрый способ вернуть виджет в видимую предсказуемую область после смены мониторов, масштаба или ручных перемещений.

## 2026-06-11 — Settings Preset Cancel Semantics
Решили, что `Save`, `Load` и `Delete` preset в окне настроек меняют только черновик диалога. В живой виджет и файл настроек изменения попадают только через `Apply` или `OK`.

Причина:
`Cancel` должен оставаться настоящей отменой. Прежнее мгновенное применение preset-действий делало поведение неожиданным и могло сохранять изменения, которые пользователь ожидал отменить.

## 2026-06-11 — Pomodoro Session Extraction
Решили вынести Pomodoro display mode, команды переключения/старта/сброса, completion transitions, текст пункта меню и tick-state в `PomodoroSession`, оставив `MainWindow` владельцем WPF-контролов, звука и сохранения.

Причина:
Это продолжает уменьшать ответственность `MainWindow.xaml.cs`, не меняет минимальный Pomodoro UI и оставляет новую логику тестируемой без запуска WPF.

## 2026-06-11 — Built-In Visual Presets
Решили добавить встроенные визуальные пресеты `Compact`, `Large`, `Minimal`, `Pomodoro` через `WidgetSettings.CreateBuiltInPresets()`. Они отображаются в Settings рядом с пользовательскими пресетами, но не сохраняются в `settings.json` и не удаляются; пользовательский preset с тем же именем работает как override.

Причина:
Нужны готовые варианты внешнего вида без раздувания пользовательского файла настроек и без неявного восстановления удалённых built-in записей.

## 2026-06-11 — Tabbed Settings Window
Решили перегруппировать окно настроек во вкладки `Presets`, `Appearance`, `Pomodoro`, `System` без новых production dependencies.

Причина:
Окно настроек стало длинным после Pomodoro, presets и системных опций; вкладки снижают прокрутку и отделяют разные сценарии настройки.

## 2026-06-11 — Settings Preset Catalog
Решили вынести правила списка, поиска, сохранения и удаления presets в `SettingsPresetCatalog`. UI теперь различает built-in, custom и custom override, не даёт удалить чистый built-in preset, а удаление custom override сбрасывает его к built-in.

Причина:
Preset-логика стала отдельным поведением с edge cases. Её лучше держать вне WPF code-behind и покрывать unit-тестами.

## 2026-06-11 — Settings Draft Dirty State
Решили включать `Apply` только когда текущий draft настроек отличается от последнего применённого состояния. Сравнение делается в памяти через тот же нормализованный JSON, который использует `SettingsStore`.

Причина:
Пользователь должен видеть, есть ли реальные неприменённые изменения, при этом это не должно создавать фоновых записей на диск.

## 2026-06-11 — Settings Import/Export
Решили добавить ручные `Import` / `Export` в Settings. Import меняет только draft диалога до `Apply` или `OK`; Export пишет только выбранный пользователем файл.

Причина:
Это даёт переносимость и безопасные эксперименты с настройками без фоновых disk writes и без новых dependencies.

## 2026-06-11 — Second Launch Activation
Решили, что второй запуск приложения должен сигналить первому процессу и выводить существующий виджет вперёд, а не просто молча завершаться.

Причина:
Single-instance guard уже предотвращал дубликаты, но пользовательский сценарий повторного запуска ожидает увидеть существующее окно.

## 2026-06-11 — Window Placement Geometry
Решили вынести чистую математику clamp/snap/default-position в `WindowPlacementGeometry`, оставив WPF/WinForms screen API в `WindowPlacementService`.

Причина:
Так позиционирование можно тестировать без запуска UI и без зависимости от текущих мониторов среды.
