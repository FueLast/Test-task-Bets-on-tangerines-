# 🟠 MandarinBid

> Веб-платформа аукциона в реальном времени на ASP.NET Core с акцентом на конкурентность, фоновые задачи и чистую архитектуру.

![.NET](https://img.shields.io/badge/.NET-ASP.NET%20Core-512BD4)
![Framework](https://img.shields.io/badge/Framework-ASP.NET%20Core-blue)
![Architecture](https://img.shields.io/badge/Architecture-Layered%20%2B%20Services-green)
![ORM](https://img.shields.io/badge/ORM-Entity%20Framework%20Core-6DB33F)
![Auth](https://img.shields.io/badge/Auth-ASP.NET%20Identity-orange)
![Database](https://img.shields.io/badge/Database-SQL%20Server-red)
![Background](https://img.shields.io/badge/Background-IHostedService-purple)
![Concurrency](https://img.shields.io/badge/Concurrency-Optimistic%20(RowVersion)-informational)
![Status](https://img.shields.io/badge/status-active-success)

---

## 📌 Обзор

**MandarinBid** — это веб-приложение, реализующее механику онлайн-аукциона, где пользователи делают ставки на лоты (мандаринки) с ограниченным временем жизни.

Система спроектирована с упором на:
- корректную обработку конкурентных запросов
- устойчивость к race conditions
- асинхронную обработку фоновых задач
- масштабируемость и отзывчивость
- чистую архитектуру (разделение ответственности)

---

## ⚙️ Технологический стек

- **ASP.NET Core MVC**
- **Entity Framework Core**
- **ASP.NET Identity**
- **SQL Server**
- **IHostedService / Background Services**
- **Mailtrap API (email)**
- **Dependency Injection**

---

## 🧠 Архитектура

Проект построен по принципу **layered architecture**:

### 🔹 Controllers
- принимают HTTP-запросы
- выполняют базовую валидацию
- делегируют работу сервисам

### 🔹 Services
- содержат бизнес-логику
- управляют ставками
- инициируют фоновые задачи
- работают с БД через EF Core

### 🔹 Background Services
- выполняют задачи вне HTTP-потока
- обеспечивают асинхронность и масштабируемость

### 🔹 Data Layer
- `ApplicationDbContext`
- модели: `Mandarin`, `Bid`
- настройка precision и типов данных

---

## 🔥 Основной функционал

- 📈 **Ставки в реальном времени**
  - обновление UI каждые 5 секунд

- ⚔️ **Защита от конкурентных конфликтов**
  - optimistic concurrency (`RowVersion`)
  - обработка `DbUpdateConcurrencyException`

- 🧠 **Бизнес-валидация**
  - запрет самоперебивания
  - проверка минимальной ставки
  - запрет ставок после завершения

- 📬 **Асинхронные уведомления**
  - уведомление о перебитой ставке
  - уведомление победителя

- 🔁 **Retry механизм**
  - повторные попытки отправки email

- ⚙️ **Фоновые процессы**
  - генерация лотов
  - завершение аукционов
  - обработка очереди задач

---

## 📸 Screenshots

| 🏠 Main Page |
|------|
| <img src="https://github.com/user-attachments/assets/4328935d-90fb-49d6-922e-5dc4fb7a057f" width="850"/>|

| 📫 MailTrap |
|--------|
| <img src="https://github.com/user-attachments/assets/6f576890-1671-4c8d-9e38-826b8573cdd7" width="724"/>|

| 📜 Electronic Check |
|------|
| <img src="https://github.com/user-attachments/assets/31dfeb0d-cc18-458c-819e-82ad22d8e55e" width="340"/> |

---

## ⚠️ Обработка конкурентности

Используется комбинация:

- `RowVersion` (optimistic concurrency)
- проверка актуальной цены
- обработка исключений EF Core

Это гарантирует:
- консистентность данных
- корректную работу при одновременных ставках

---

## 🔄 Фоновые процессы

### 📌 Сервисы:

- `MandarinGeneratorService`
  - создаёт новые лоты

- `MandarinCleanupService`
  - завершает аукционы
  - определяет победителя
  - инициирует отправку email

- `EmailBackgroundService`
  - обрабатывает очередь задач

---

## 📩 Email flow

1. пользователь делает ставку  
2. определяется предыдущий лидер  
3. задача отправки добавляется в очередь  
4. background service обрабатывает задачу  
5. письмо отправляется через Mailtrap API  

---

## 🧩 Архитектурные решения

- **Очередь вместо прямой отправки email**
  - не блокирует HTTP-запрос
  - повышает производительность

- **AsNoTracking**
  - снижает нагрузку на EF Core

- **Денормализация UserName в Bid**
  - уменьшает количество join-запросов

- **Короткие аукционы**
  - удобны для тестирования и демонстрации

---

## 🧪 Обработанные edge cases

- ставка после завершения  
- самоперебивание  
- ставка ниже текущей  
- race conditions  
- отсутствие email  
- сбои email API  

---

## 🤖 Использование ИИ

В процессе разработки использовались LLM (Large Language Models).

ИИ применялся:
- для ускорения написания шаблонного кода
- для генерации вспомогательных частей
- для улучшения документации

Ключевые решения (архитектура, бизнес-логика, конкурентность) реализованы самостоятельно.

> ИИ использовался как инструмент повышения продуктивности, а не как замена инженерного мышления.

---

## 🚀 Запуск проекта

1. Указать строку подключения в `appsettings.json`

2. Применить миграции:
   ```bash
   dotnet ef database update

3. Запустить приложение:
   ```bash
   dotnet run

4. Открыть в браузере:
  https://localhost:5001

---

## 📎 Дополнительно
- Проект содержит XML-документацию методов
- Код покрыт комментариями для упрощения навигации
- Логирование реализовано через ILogger

---

## 📌 Итог

Проект демонстрирует:

- уверенное владение ASP.NET Core
- понимание конкурентности и многопоточности
- работу с background processing
- умение проектировать масштабируемые системы
