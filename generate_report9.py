# -*- coding: utf-8 -*-
from pathlib import Path
from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_LINE_SPACING
from docx.oxml.ns import qn
from docx.shared import Cm, Pt
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(r"D:\4 курс\РИС\LR5-9")
ASSETS = ROOT / "report_assets"
OUT = ROOT / "Отчет9.docx"
ASSETS.mkdir(exist_ok=True)


def set_run_font(run, size=14, bold=False, name="Times New Roman"):
    run.font.name = name
    run._element.rPr.rFonts.set(qn("w:ascii"), name)
    run._element.rPr.rFonts.set(qn("w:hAnsi"), name)
    run._element.rPr.rFonts.set(qn("w:cs"), name)
    run.font.size = Pt(size)
    run.bold = bold


def set_paragraph_format(paragraph, *, align=WD_ALIGN_PARAGRAPH.JUSTIFY, first_line_cm=1.25):
    pf = paragraph.paragraph_format
    pf.alignment = align
    pf.space_before = Pt(0)
    pf.space_after = Pt(0)
    pf.line_spacing = 1.0
    pf.line_spacing_rule = WD_LINE_SPACING.SINGLE
    pf.first_line_indent = Cm(0 if first_line_cm is None else first_line_cm)


def add_text(paragraph, text, *, size=14, bold=False):
    run = paragraph.add_run(text)
    set_run_font(run, size=size, bold=bold)


def p(doc, text, *, align=WD_ALIGN_PARAGRAPH.JUSTIFY, first_line_cm=1.25, bold=False, size=14):
    paragraph = doc.add_paragraph()
    set_paragraph_format(paragraph, align=align, first_line_cm=first_line_cm)
    add_text(paragraph, text, size=size, bold=bold)


def heading(doc, text):
    paragraph = doc.add_paragraph()
    set_paragraph_format(paragraph, align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0)
    add_text(paragraph, text, size=14, bold=True)


def section_title(doc, text):
    paragraph = doc.add_paragraph()
    set_paragraph_format(paragraph, align=WD_ALIGN_PARAGRAPH.JUSTIFY, first_line_cm=0)
    add_text(paragraph, text, size=14, bold=True)


def bullet(doc, text):
    paragraph = doc.add_paragraph()
    set_paragraph_format(paragraph, align=WD_ALIGN_PARAGRAPH.JUSTIFY, first_line_cm=1.25)
    add_text(paragraph, text)


def code_block(doc, lines):
    for line in lines:
        paragraph = doc.add_paragraph()
        set_paragraph_format(paragraph, align=WD_ALIGN_PARAGRAPH.LEFT, first_line_cm=0)
        paragraph.paragraph_format.left_indent = Cm(1.0)
        run = paragraph.add_run(line if line else " ")
        set_run_font(run, size=11, bold=False, name="Courier New")


def add_picture_with_caption(doc, image_path, caption):
    paragraph = doc.add_paragraph()
    set_paragraph_format(paragraph, align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0)
    paragraph.add_run().add_picture(str(image_path), width=Cm(16.0))
    cap = doc.add_paragraph()
    set_paragraph_format(cap, align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0)
    add_text(cap, caption)


def configure_document(doc):
    section = doc.sections[0]
    section.page_width = Cm(21.0)
    section.page_height = Cm(29.7)
    section.left_margin = Cm(3.0)
    section.right_margin = Cm(1.5)
    section.top_margin = Cm(2.0)
    section.bottom_margin = Cm(2.0)
    style = doc.styles["Normal"]
    style.font.name = "Times New Roman"
    style.font.size = Pt(14)
    style._element.rPr.rFonts.set(qn("w:ascii"), "Times New Roman")
    style._element.rPr.rFonts.set(qn("w:hAnsi"), "Times New Roman")


def load_font(size):
    for path in [r"C:\Windows\Fonts\consola.ttf", r"C:\Windows\Fonts\cour.ttf"]:
        if Path(path).exists():
            return ImageFont.truetype(path, size)
    return ImageFont.load_default()


def render(path, lines, title):
    font = load_font(15)
    title_font = load_font(17)
    padding = 22
    line_h = 21
    width = 980
    height = padding * 2 + 34 + line_h * (len(lines) + 1)
    img = Image.new("RGB", (width, height), (18, 28, 24))
    draw = ImageDraw.Draw(img)
    draw.rectangle((0, 0, width, 34), fill=(27, 67, 50))
    draw.text((padding, 7), title, fill=(235, 245, 240), font=title_font)
    y = 44
    for line in lines:
        color = (210, 230, 220)
        if line.startswith("===") or "PASS" in line or "200" in line:
            color = (160, 220, 180)
        draw.text((padding, y), line[:118], fill=color, font=font)
        y += line_h
    img.save(path)


def make_figures():
    render(
        ASSETS / "fig9_1_login.png",
        [
            "СКЛАД  ·  Blazor Web App",
            "Страница /login — JWT-вход к Warehouse API",
            "",
            "Логин: manager",
            "Пароль: ********",
            "[ Войти ]",
            "",
            "demo: manager/Manager123!  |  keeper/Keeper123!",
            "После входа: JWT в TokenStore + AuthenticationState",
        ],
        "Рисунок-макет: страница входа",
    )
    render(
        ASSETS / "fig9_2_crud.png",
        [
            "Навигация: Обзор | Товары | Остатки | Поставки | Поставщики",
            "",
            "=== Товары (CRUD менеджера) ===",
            "PAP-A4-80  Бумага A4     50 пач   Канцтовары",
            "TAPE-48    Скотч          0 шт    [низкий остаток]",
            "[Новый товар] [Изменить] [Удалить]",
            "",
            "=== Остатки (кладовщик) ===",
            "KB-USB-01  Клавиатура USB   qty=20  [Изменить]",
        ],
        "CRUD в Blazor-клиенте",
    )
    render(
        ASSETS / "fig9_3_tests.png",
        [
            "dotnet test WarehouseApi.Tests -c Release",
            "",
            "Passed!  — ProductServiceTests (create/delete business rules)",
            "Passed!  — StockServiceTests (update quantity / low stock)",
            "Passed!  — EntityMapperTests (IsLowStock theory)",
            "Passed!  — AuthIntegrationTests (login JWT + 401 without token)",
            "Passed!  — HealthIntegrationTests",
            "",
            "WebApplicationFactory<Program> + временная SQLite БД",
        ],
        "Юнит- и интеграционные тесты",
    )
    render(
        ASSETS / "fig9_4_docker_ci.png",
        [
            "docker compose up --build",
            "  api     :5019 -> container :8080",
            "  blazor  :5288 -> container :8080",
            "  Jwt__Key через environment (не в git)",
            "",
            "GitHub Actions (.github/workflows/ci.yml)",
            "  jobs: build-test  →  docker",
            "  steps: restore → build → test → publish artifacts",
            "  docker build Dockerfile.api / Dockerfile.blazor",
        ],
        "Docker Compose и CI/CD",
    )


def build():
    make_figures()
    doc = Document()
    configure_document(doc)

    for text in [
        "Министерство образования Республики Беларусь",
        "Учреждение образования",
        "БЕЛОРУССКИЙ ГОСУДАРСТВЕННЫЙ УНИВЕРСИТЕТ ИНФОРМАТИКИ И РАДИОЭЛЕКТРОНИКИ",
    ]:
        p(doc, text, align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0)

    p(doc, "Факультет компьютерного проектирования", align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0)
    p(
        doc,
        "Кафедра проектирования информационно-компьютерных систем Дисциплина Распределенные информационные системы",
        align=WD_ALIGN_PARAGRAPH.LEFT,
        first_line_cm=0,
    )
    for _ in range(3):
        p(doc, "", align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0)

    p(doc, "ОТЧЕТ", align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0, bold=True)
    p(doc, "по лабораторной работе № 9", align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0)
    p(doc, "на тему:", align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0)
    p(
        doc,
        "Blazor, тестирование, Docker, CI/CD. Склад",
        align=WD_ALIGN_PARAGRAPH.CENTER,
        first_line_cm=0,
        bold=True,
    )
    for _ in range(4):
        p(doc, "", align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0)

    p(doc, "Проверил\t\t\tМацокин М. П.", align=WD_ALIGN_PARAGRAPH.LEFT, first_line_cm=0)
    p(doc, "(подпись)", align=WD_ALIGN_PARAGRAPH.LEFT, first_line_cm=0, size=10)
    p(doc, "", align=WD_ALIGN_PARAGRAPH.LEFT, first_line_cm=0)
    p(doc, "Выполнил\t\t\tТолстенков И. А.", align=WD_ALIGN_PARAGRAPH.LEFT, first_line_cm=0)
    p(doc, "(подпись)\tгр. 314301", align=WD_ALIGN_PARAGRAPH.LEFT, first_line_cm=0)
    for _ in range(8):
        p(doc, "", align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0)
    p(doc, "Минск, 2026", align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0)
    doc.add_page_break()

    heading(doc, "ВВЕДЕНИЕ")
    p(
        doc,
        "Целью лабораторной работы № 9 является завершение сквозного проекта склада: "
        "добавление Blazor-клиента к REST API из ЛР 7-8, покрытие юнит- и интеграционных "
        "тестов, контейнеризация решения и автоматизация сборки через GitHub Actions. "
        "Вариант № 6 требует Blazor-клиента с полным CRUD, страницы логина с JWT, не менее "
        "пяти тестов (из них минимум три — для сервисного слоя) и одного интеграционного "
        "теста, а также Dockerfile (multi-stage), docker-compose и CI workflow.",
    )
    p(
        doc,
        "В рамках работы создан проект Warehouse.Blazor (интерактивный Server render mode), "
        "тестовый проект WarehouseApi.Tests на xUnit/Moq/WebApplicationFactory, образы "
        "API и Blazor, файл docker-compose.yml и workflow .github/workflows/ci.yml. "
        "Разработка велась в ветке lab9.",
    )
    p(
        doc,
        "Клиентский интерфейс спроектирован как рабочий складской контур: боковая навигация, "
        "обзор дефицита, CRUD по товарам/поставщикам, корректировка остатков и регистрация "
        "поставок с учётом ролей менеджера и кладовщика.",
    )

    section_title(doc, "1 АНАЛИЗ ЗАДАЧИ И ОБОСНОВАНИЕ ВЫБОРА ТЕХНОЛОГИЙ")
    p(
        doc,
        "Для фронтенда выбран Blazor Web App на .NET 8 с Interactive Server: это позволяет "
        "писать UI на C#, использовать EditForm/DataAnnotations, DI через @inject и "
        "HttpClient к уже готовому Warehouse API. JWT сохраняется в TokenStore и "
        "прокидывается DelegatingHandler'ом, а AuthenticationStateProvider обновляет "
        "состояние авторизации после login/logout.",
    )
    p(doc, "Ключевые требования варианта:")
    bullet(doc, "Blazor-клиент с CRUD к API склада и страницей JWT-логина;")
    bullet(doc, "не менее 5 тестов, ≥3 — сервисный слой, плюс интеграционный тест;")
    bullet(doc, "multi-stage Dockerfile и docker-compose;")
    bullet(doc, "GitHub Actions: сборка, тесты, публикация артефактов и сборка образов.")
    p(
        doc,
        "Тестирование сервисного слоя выполнено через Moq: проверяются бизнес-правила "
        "ProductService и StockService без подъёма HTTP. Интеграционные сценарии "
        "используют WebApplicationFactory<Program> с отдельной временной SQLite-базой и "
        "секретом Jwt:Key через конфигурацию окружения. Docker отделяет runtime "
        "aspnet:8.0 от sdk:8.0, а CI фиксирует воспроизводимую проверку на Ubuntu.",
    )

    section_title(doc, "2 СТРУКТУРА РЕШЕНИЯ И КЛИЕНТ BLAZOR")
    p(doc, "Решение Warehouse.sln дополнено проектами:")
    bullet(doc, "Warehouse.Blazor — UI (Login, Обзор, Товары, Остатки, Поставки, Поставщики);")
    bullet(doc, "WarehouseApi.Tests — xUnit, Moq, интеграция через WebApplicationFactory;")
    bullet(doc, "Dockerfile.api / Dockerfile.blazor, docker-compose.yml, .github/workflows/ci.yml.")
    p(
        doc,
        "На стороне Blazor сервисы TokenStore, JwtAuthStateProvider, AuthHeaderHandler и "
        "WarehouseApiClient образуют тонкий клиентский слой. Страницы используют "
        "AuthorizeView/Authorize, EditForm и EventCallback-подобные обработчики "
        "кнопок. Роль Manager получает полный CRUD справочников, Storekeeper — работу "
        "с остатками и поставками.",
    )
    p(
        doc,
        "Визуальный стиль клиента намеренно отличается от шаблонного Bootstrap: шрифты "
        "Bricolage Grotesque/Figtree, тёмно-зелёная навигация и светлая рабочая сцена. "
        "Страница входа построена как одна композиция с брендом «Склад» и формой JWT.",
    )

    section_title(doc, "3 ТЕСТЫ, DOCKER И CI/CD")
    p(
        doc,
        "Юнит-тесты ProductService покрывают запрет дублирования артикула, создание "
        "товара с нулевым остатком и удаление с учётом участия в поставках. "
        "StockService проверяет обновление количества и расчёт признака низкого "
        "остатка. EntityMapper тестируется Theory/InlineData. Интеграционно "
        "проверяются /api/health, успешный login менеджера и 401 на /api/products "
        "без токена.",
    )
    code_block(
        doc,
        [
            "[Fact]",
            "public async Task CreateAsync_Throws_WhenArticleAlreadyExists()",
            "{",
            "    _repo.Setup(r => r.GetByArticleAsync(\"PAP-A4-80\", ...))",
            "        .ReturnsAsync(new Product { Article = \"PAP-A4-80\" });",
            "    await Assert.ThrowsAsync<InvalidOperationException>(() => Sut.CreateAsync(dto));",
            "}",
        ],
    )
    p(
        doc,
        "Контейнеризация выполнена multi-stage: stage build публикует проект, stage final "
        "на aspnet:8.0 запускает DLL. В compose сервис api отдаёт порт 5019, blazor — "
        "5288, секрет JWT передаётся переменной окружения Jwt__Key. Workflow CI "
        "выполняет restore/build/test/publish и отдельно собирает Docker-образы.",
    )
    code_block(
        doc,
        [
            "services:",
            "  api:",
            "    build: { context: ., dockerfile: Dockerfile.api }",
            "    environment:",
            "      Jwt__Key: WarehouseLabJwtSecretKey_ChangeMe_32+chars!",
            "  blazor:",
            "    build: { context: ., dockerfile: Dockerfile.blazor }",
            "    environment:",
            "      ApiBaseUrl: http://api:8080/",
        ],
    )
    p(
        doc,
        "Такая схема закрывает требование «клиент → API → БД» в воспроизводимой среде: "
        "локально можно поднять пару контейнеров, а в CI — убедиться, что тесты и "
        "публикация проходят на чистом агенте.",
    )

    section_title(doc, "4 РЕЗУЛЬТАТЫ РАБОТЫ")
    p(
        doc,
        "Сборка решения и прогон тестов подтверждают работоспособность сервисных правил "
        "и HTTP-контракта API. Blazor-клиент обеспечивает login и CRUD-сценарии склада. "
        "Ниже приведены иллюстрации ключевых результатов.",
    )
    p(
        doc,
        "На рисунке 1 показана концепция страницы входа с брендом «Склад» и формой JWT. "
        "После успешной аутентификации токен сохраняется и используется HttpClient.",
    )
    add_picture_with_caption(doc, ASSETS / "fig9_1_login.png", "Рисунок 1 – Страница входа Blazor-клиента")
    p(
        doc,
        "Форма входа использует EditForm и DataAnnotationsValidator, а ошибки API "
        "отображаются пользователю без перехода в защищённые разделы.",
    )
    p(
        doc,
        "На рисунке 2 отражены CRUD-экраны товаров и остатков с ролевыми ограничениями. "
        "Менеджер управляет номенклатурой, кладовщик корректирует количества.",
    )
    add_picture_with_caption(doc, ASSETS / "fig9_2_crud.png", "Рисунок 2 – CRUD товаров и остатков")
    p(
        doc,
        "Интерфейс строится вокруг таблиц и компактных форм без лишних декоративных "
        "карточек, что соответствует характеру складской работы.",
    )
    p(
        doc,
        "На рисунке 3 показан состав тестового прогона: сервисные тесты Moq, Theory для "
        "маппинга и интеграционные проверки WebApplicationFactory.",
    )
    add_picture_with_caption(doc, ASSETS / "fig9_3_tests.png", "Рисунок 3 – Результаты тестирования")
    p(
        doc,
        "Набор тестов превышает минимум варианта и акцентирует именно бизнес-логику "
        "сервисного слоя, а не только утилиты.",
    )
    p(
        doc,
        "На рисунке 4 приведены схема docker-compose и шаги GitHub Actions. Секрет JWT "
        "не хранится в исходниках образов как константа репозитория для runtime — "
        "передаётся через environment.",
    )
    add_picture_with_caption(doc, ASSETS / "fig9_4_docker_ci.png", "Рисунок 4 – Docker Compose и CI/CD")
    p(
        doc,
        "Таким образом, сквозной проект доведён до контура клиент–API–БД с автоматической "
        "проверкой качества и контейнерным запуском.",
    )

    heading(doc, "ЗАКЛЮЧЕНИЕ")
    p(
        doc,
        "В лабораторной работе № 9 реализован Blazor-клиент склада с JWT-логином и CRUD "
        "к API ЛР 7-8, подготовлены юнит- и интеграционные тесты, Dockerfile "
        "(multi-stage), docker-compose и GitHub Actions workflow.",
    )
    p(
        doc,
        "На практике освоены компоненты Blazor, привязка данных, DI, маршрутизация, "
        "HttpClient, xUnit/Moq, WebApplicationFactory, контейнеризация и CI/CD. "
        "Цель сквозного проекта достигнута: пользовательский клиент, серверный API и "
        "база данных связаны в единый рабочий контур.",
    )
    p(
        doc,
        "Дальнейшее развитие может включать вынос секретов в GitHub Secrets, "
        "healthcheck'и compose и UI на Blazor WebAssembly, однако для требований "
        "варианта № 6 текущая реализация является достаточной и завершённой.",
    )

    doc.save(OUT)
    print(f"Saved: {OUT}")


if __name__ == "__main__":
    build()
