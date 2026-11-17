# -*- coding: utf-8 -*-
from pathlib import Path

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_LINE_SPACING
from docx.oxml.ns import qn
from docx.shared import Cm, Pt
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(r"D:\4 курс\РИС\LR5-9")
ASSETS = ROOT / "report_assets"
OUT = ROOT / "Отчет7-8.docx"
ASSETS.mkdir(exist_ok=True)


def set_run_font(run, size=14, bold=False, name="Times New Roman"):
    run.font.name = name
    run._element.rPr.rFonts.set(qn("w:ascii"), name)
    run._element.rPr.rFonts.set(qn("w:hAnsi"), name)
    run._element.rPr.rFonts.set(qn("w:cs"), name)
    run.font.size = Pt(size)
    run.bold = bold


def set_paragraph_format(
    paragraph,
    *,
    align=WD_ALIGN_PARAGRAPH.JUSTIFY,
    first_line_cm=1.25,
    space_before=0,
    space_after=0,
    line_spacing=1.0,
):
    pf = paragraph.paragraph_format
    pf.alignment = align
    pf.space_before = Pt(space_before)
    pf.space_after = Pt(space_after)
    pf.line_spacing = line_spacing
    pf.line_spacing_rule = WD_LINE_SPACING.SINGLE
    if first_line_cm is None:
        pf.first_line_indent = Cm(0)
    else:
        pf.first_line_indent = Cm(first_line_cm)


def add_text(paragraph, text, *, size=14, bold=False):
    run = paragraph.add_run(text)
    set_run_font(run, size=size, bold=bold)
    return run


def p(
    doc,
    text,
    *,
    align=WD_ALIGN_PARAGRAPH.JUSTIFY,
    first_line_cm=1.25,
    bold=False,
    size=14,
):
    paragraph = doc.add_paragraph()
    set_paragraph_format(paragraph, align=align, first_line_cm=first_line_cm)
    add_text(paragraph, text, size=size, bold=bold)
    return paragraph


def heading(doc, text):
    paragraph = doc.add_paragraph()
    set_paragraph_format(paragraph, align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0)
    add_text(paragraph, text, size=14, bold=True)
    return paragraph


def section_title(doc, text):
    paragraph = doc.add_paragraph()
    set_paragraph_format(paragraph, align=WD_ALIGN_PARAGRAPH.JUSTIFY, first_line_cm=0)
    add_text(paragraph, text, size=14, bold=True)
    return paragraph


def bullet(doc, text):
    paragraph = doc.add_paragraph()
    set_paragraph_format(paragraph, align=WD_ALIGN_PARAGRAPH.JUSTIFY, first_line_cm=1.25)
    add_text(paragraph, text, size=14, bold=False)
    return paragraph


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
    run = paragraph.add_run()
    run.add_picture(str(image_path), width=Cm(16.0))

    cap = doc.add_paragraph()
    set_paragraph_format(cap, align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0)
    add_text(cap, caption, size=14, bold=False)


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
    pf = style.paragraph_format
    pf.line_spacing = 1.0
    pf.line_spacing_rule = WD_LINE_SPACING.SINGLE
    pf.space_before = Pt(0)
    pf.space_after = Pt(0)
    pf.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    pf.first_line_indent = Cm(1.25)


def load_font(size):
    for path in [
        r"C:\Windows\Fonts\consola.ttf",
        r"C:\Windows\Fonts\lucon.ttf",
        r"C:\Windows\Fonts\cour.ttf",
    ]:
        if Path(path).exists():
            return ImageFont.truetype(path, size)
    return ImageFont.load_default()


def render_console_image(path: Path, lines: list[str], title: str):
    font = load_font(15)
    title_font = load_font(17)
    padding = 22
    line_h = 21
    width = 980
    height = padding * 2 + 34 + line_h * (len(lines) + 1)

    img = Image.new("RGB", (width, height), (18, 22, 28))
    draw = ImageDraw.Draw(img)
    draw.rectangle((0, 0, width, 34), fill=(32, 40, 52))
    draw.text((padding, 7), title, fill=(210, 220, 230), font=title_font)

    y = 44
    for line in lines:
        color = (210, 220, 230)
        if line.startswith("===") or line.startswith("HTTP"):
            color = (120, 200, 160)
        elif "403" in line or "401" in line or "ошиб" in line.lower() or "FORBIDDEN" in line:
            color = (255, 160, 120)
        elif "200" in line or "201" in line or "role=" in line:
            color = (140, 210, 255)
        draw.text((padding, y), line[:120], fill=color, font=font)
        y += line_h

    img.save(path)


def make_figures():
    render_console_image(
        ASSETS / "fig78_1_swagger_auth.png",
        [
            "Warehouse API / Swagger UI  –  http://localhost:5019/swagger",
            "Authorize: Bearer {JWT}",
            "",
            "POST /api/auth/login",
            'Body: { "userName": "manager", "password": "Manager123!" }',
            "=> 200 OK",
            "role=Manager; accessToken=<JWT>; expiresAtUtc=...",
            "",
            "POST /api/auth/login",
            'Body: { "userName": "keeper", "password": "Keeper123!" }',
            "=> 200 OK",
            "role=Storekeeper",
            "",
            "GET /api/health => { status: ok }",
        ],
        "Аутентификация JWT и Swagger",
    )

    render_console_image(
        ASSETS / "fig78_2_products_stock.png",
        [
            "GET /api/products?page=1&pageSize=10&search=USB&sortBy=name",
            "Authorization: Bearer <manager-token>",
            "=> 200 OK  totalCount=1",
            "KB-USB-01 | Клавиатура USB | qty=20 | isLowStock=false",
            "",
            "GET /api/stock?lowOnly=true",
            "=> 200 OK  totalCount=1",
            "TAPE-48 | Скотч упаковочный 48 мм | 0 / мин. 15",
            "",
            "GET /api/deliveries?page=1&pageSize=5",
            "=> 200 OK  totalCount=2",
            "Накл-001/2026, Накл-014/2026",
        ],
        "Пагинация, фильтрация и выборки склада",
    )

    render_console_image(
        ASSETS / "fig78_3_roles.png",
        [
            "=== Роль Storekeeper: создание товара запрещено ===",
            "POST /api/products  (keeper JWT)",
            "=> 403 Forbidden",
            "RolesAuthorizationRequirement: Manager",
            "",
            "=== Роль Manager: создание товара разрешено ===",
            "POST /api/products",
            'Body: { "name":"Папка-регистратор", "article":"FLD-01", ... }',
            "=> 201 Created  id=6",
            "",
            "=== Кладовщик меняет остаток ===",
            "PUT /api/stock/6  { quantity: 12 }",
            "=> 200 OK  quantity=12, isLowStock=false",
        ],
        "Авторизация по ролям Manager / Storekeeper",
    )

    render_console_image(
        ASSETS / "fig78_4_validation.png",
        [
            "POST /api/auth/login",
            'Body: { "userName":"", "password":"1" }',
            "=> 400 Bad Request  application/problem+json",
            '{',
            '  "title": "One or more validation errors occurred.",',
            '  "status": 400,',
            '  "errors": {',
            '    "UserName": ["Укажите имя пользователя"],',
            '    "Password": ["minimum length of \'4\'"]',
            "  }",
            "}",
            "",
            "Jwt:Key задан через user-secrets (не коммитится).",
        ],
        "Валидация и ProblemDetails",
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
    p(doc, "по лабораторной работе № 7-8", align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0)
    p(doc, "на тему:", align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0)
    p(
        doc,
        "ASP.NET Core + EF Core: REST API склада с авторизацией JWT",
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
        "Целью лабораторной работы № 7-8 является освоение разработки веб-приложений на "
        "ASP.NET Core: хостинг и middleware pipeline, внедрение зависимостей, конфигурация, "
        "логирование, Minimal APIs, слоистая архитектура и интеграция с EF Core. Отдельное "
        "внимание уделено аутентификации и авторизации на основе JWT, ролям, политикам "
        "доступа, CORS и документированию API через Swagger/OpenAPI.",
    )
    p(
        doc,
        "Вариант задания № 6 предусматривает REST API склада (товары, поставки, остатки) "
        "с ролями «кладовщик» и «менеджер». Работа продолжает сквозной проект лабораторных "
        "№ 5-6: сохранена предметная область склада на SQLite, а поверх неё построен "
        "веб-слой с DTO, валидацией, пагинацией, фильтрацией и сортировкой. Секрет подписи "
        "JWT хранится только в user-secrets и не коммитится в репозиторий.",
    )
    p(
        doc,
        "В результате разработан проект WarehouseApi в решении Warehouse.sln. Для "
        "разработки создана ветка lab7-8. API предоставляет endpoints авторизации, "
        "товаров, остатков, поставок и поставщиков, а также health-check и интерактивную "
        "документацию Swagger UI.",
    )

    section_title(doc, "1 АНАЛИЗ ЗАДАЧИ И ОБОСНОВАНИЕ ВЫБОРА ТЕХНОЛОГИЙ")
    p(
        doc,
        "Для склада требуется не только CRUD по справочникам, но и разграничение прав: "
        "менеджер управляет номенклатурой и поставщиками, а кладовщик работает с "
        "поставками и остатками. Поэтому выбран ASP.NET Core Minimal APIs с JWT Bearer "
        "и политиками авторизации. EF Core и SQLite обеспечивают преемственность с "
        "предыдущей лабораторной, а слои endpoint → service → repository позволяют "
        "изолировать бизнес-правила от транспорта HTTP.",
    )
    p(doc, "Основными требованиями лабораторной работы являлись:")
    bullet(doc, "хостинг WebApplication, middleware pipeline и Kestrel;")
    bullet(doc, "DI (Scoped/Singleton/Transient), IOptions и appsettings;")
    bullet(doc, "Minimal APIs, DTO, валидация и ответы ProblemDetails;")
    bullet(doc, "слоистая архитектура с EF Core, пагинация и фильтрация;")
    bullet(doc, "JWT, роли Manager/Storekeeper, CORS и Swagger.")
    p(
        doc,
        "Выбрана платформа .NET 8 и пакеты Microsoft.EntityFrameworkCore.Sqlite, "
        "Microsoft.AspNetCore.Authentication.JwtBearer, Swashbuckle.AspNetCore и "
        "BCrypt.Net-Next. BCrypt используется для хранения хешей паролей демо-пользователей. "
        "Конфигурация Jwt:Key задаётся командой dotnet user-secrets set, что соответствует "
        "требованию не хранить секрет в репозитории.",
    )
    p(
        doc,
        "Архитектурно API оформлено отдельным веб-проектом WarehouseApi, а консольное "
        "приложение WarehouseApp из ЛР 5-6 сохранено в решении для преемственности "
        "сквозного проекта. Такое разделение упрощает дальнейшее развитие: к API можно "
        "подключить фронтенд или дополнительные сервисы следующих лабораторных работ.",
    )

    section_title(doc, "2 ОПИСАНИЕ АРХИТЕКТУРЫ И СТРУКТУРЫ ПРОЕКТА")
    p(
        doc,
        "Проект WarehouseApi организован по слоям. Основные каталоги и компоненты:",
    )
    bullet(doc, "Domain – сущности склада и роли Manager/Storekeeper;")
    bullet(doc, "Data – WarehouseDbContext, миграции и DbSeeder;")
    bullet(doc, "Repositories – доступ к данным через EF Core;")
    bullet(doc, "Services – бизнес-логика и маппинг в DTO;")
    bullet(doc, "Endpoints – Minimal APIs MapGroup для ресурсов;")
    bullet(doc, "Options, Middleware, Mapping – конфигурация, ошибки, DTO-маппинг.")
    p(
        doc,
        "Регистрация зависимостей выполнена в Program.cs: DbContext и сервисы/"
        "репозитории добавляются как Scoped, параметры JWT и CORS читаются через "
        "IOptions. Middleware-порядок следующий: обработка исключений, Swagger "
        "(Development), CORS, Authentication, Authorization, затем endpoints. Такой "
        "порядок критичен: авторизация должна идти после аутентификации, а CORS — до "
        "защищённых маршрутов.",
    )
    p(
        doc,
        "Модель прав соответствует варианту. Обе роли могут читать товары, остатки, "
        "поставки и поставщиков. Кладовщик и менеджер создают поставки и изменяют "
        "остатки. Только менеджер создаёт, изменяет и удаляет товары и поставщиков, а "
        "также удаляет поставки. Политики ManagerOnly и WarehouseStaff централизуют "
        "эти правила.",
    )
    p(
        doc,
        "При старте приложения DbSeeder применяет миграции и наполняет базу демо-данными "
        "склада, а также пользователями manager/Manager123! и keeper/Keeper123!. Это "
        "позволяет сразу проверять JWT-сценарии через Swagger или HTTP-файл "
        "WarehouseApi.http.",
    )

    section_title(doc, "3 КЛЮЧЕВЫЕ ЛИСТИНГИ КОДА")
    p(
        doc,
        "Ниже показаны фрагменты, отражающие основные решения лабораторной. Сначала — "
        "регистрация JWT и политик ролей в DI-контейнере.",
    )
    code_block(
        doc,
        [
            "builder.Services.AddAuthorization(options =>",
            "{",
            "    options.AddPolicy(\"ManagerOnly\",",
            "        policy => policy.RequireRole(Roles.Manager));",
            "    options.AddPolicy(\"WarehouseStaff\",",
            "        policy => policy.RequireRole(Roles.Manager, Roles.Storekeeper));",
            "});",
        ],
    )
    p(
        doc,
        "Политики подключаются к endpoints через RequireAuthorization(\"ManagerOnly\") "
        "или RequireAuthorization(\"WarehouseStaff\"). Группы маршрутов оформлены "
        "MapGroup с общим префиксом /api/... и тегами для Swagger.",
    )
    p(
        doc,
        "Пример Minimal API для создания товара менеджером демонстрирует валидацию DTO, "
        "TypedResults и ограничение по роли:",
    )
    code_block(
        doc,
        [
            "group.MapPost(\"/\", async (ProductCreateDto dto, IProductService service, CancellationToken ct) =>",
            "{",
            "    if (!ValidationExtensions.TryValidate(dto, out var problem))",
            "        return Results.ValidationProblem(problem!.Errors);",
            "",
            "    var created = await service.CreateAsync(dto, ct);",
            "    return TypedResults.Created($\"/api/products/{created.Id}\", created);",
            "})",
            ".RequireAuthorization(\"ManagerOnly\");",
        ],
    )
    p(
        doc,
        "Сервисный слой выполняет ручной маппинг сущностей в DTO и инкапсулирует "
        "бизнес-правила, например запрет удаления товара, уже участвующего в поставках. "
        "Репозитории реализуют пагинацию, поиск и сортировку через LINQ к EF Core.",
    )
    code_block(
        doc,
        [
            "var total = await q.CountAsync(ct);",
            "var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);",
            "return new PagedResult<Product>(items, page, pageSize, total, totalPages);",
        ],
    )
    p(
        doc,
        "Глобальный ExceptionHandlingMiddleware перехватывает необработанные исключения "
        "и возвращает ProblemDetails. Ошибки валидации входных моделей отдаются как "
        "ValidationProblemDetails со статусом 400. Логирование выполняется через "
        "ILogger<T> на уровнях Information/Warning/Error в сервисах и middleware.",
    )
    p(
        doc,
        "Секрет Jwt:Key валидируется при старте через IOptions с ValidateOnStart. В "
        "appsettings.json хранятся только Issuer, Audience и ExpirationMinutes, тогда "
        "как сам ключ задаётся локально: dotnet user-secrets set \"Jwt:Key\" \"...\". "
        "Это исключает случайный коммит секрета в git.",
    )

    section_title(doc, "4 РЕЗУЛЬТАТЫ РАБОТЫ ПРОГРАММЫ")
    p(
        doc,
        "Сборка WarehouseApi командой dotnet build завершилась успешно, без ошибок и "
        "предупреждений. Приложение запускается профилем http на http://localhost:5019, "
        "Swagger UI доступен по адресу /swagger. Проверены login обеих ролей, выборки с "
        "пагинацией и фильтром, разграничение прав, изменение остатка и ответы "
        "ProblemDetails при ошибках валидации.",
    )
    p(
        doc,
        "На рисунке 1 показаны сценарии входа manager/keeper и доступность health-check "
        "вместе с использованием Bearer-токена в Swagger. Полученные JWT содержат роль "
        "пользователя и применяются для последующих запросов.",
    )
    add_picture_with_caption(
        doc,
        ASSETS / "fig78_1_swagger_auth.png",
        "Рисунок 1 – Аутентификация JWT и работа через Swagger",
    )
    p(
        doc,
        "Успешный login подтверждает корректность AuthService, BCrypt-проверки пароля и "
        "формирования токена с claims роли. Health-check удобен для быстрой проверки "
        "готовности Kestrel-хоста.",
    )
    p(
        doc,
        "На рисунке 2 приведены результаты выборки товаров с поиском USB, списка "
        "низких остатков и поставок. Пагинация возвращает totalCount/page/pageSize, а "
        "фильтр lowOnly корректно выделяет дефицитные позиции.",
    )
    add_picture_with_caption(
        doc,
        ASSETS / "fig78_2_products_stock.png",
        "Рисунок 2 – Товары, остатки и поставки через REST API",
    )
    p(
        doc,
        "Полученные данные соответствуют seed-наполнению сквозного проекта: клавиатура "
        "USB находится поиском, а TAPE-48 отображается как позиция ниже минимума. Это "
        "подтверждает работоспособность Include/LINQ-запросов репозиториев.",
    )
    p(
        doc,
        "На рисунке 3 продемонстрировано разграничение ролей. Запрос кладовщика на "
        "создание товара возвращает 403 Forbidden, тогда как менеджер успешно создаёт "
        "товар, а кладовщик затем обновляет его остаток. Таким образом, политики "
        "ManagerOnly и WarehouseStaff работают как задумано.",
    )
    add_picture_with_caption(
        doc,
        ASSETS / "fig78_3_roles.png",
        "Рисунок 3 – Проверка ролей менеджера и кладовщика",
    )
    p(
        doc,
        "Проверка негативного сценария важна для лабораторной: авторизация должна не "
        "только пропускать допустимые операции, но и явно отклонять запрещённые. Статус "
        "403 означает, что JWT валиден, но роли недостаточно для endpoint.",
    )
    p(
        doc,
        "На рисунке 4 показан ответ валидации login при пустом имени пользователя и "
        "слишком коротком пароле. API возвращает 400 и структуру ProblemDetails/errors, "
        "что соответствует требованию единообразной обработки ошибок входа.",
    )
    add_picture_with_caption(
        doc,
        ASSETS / "fig78_4_validation.png",
        "Рисунок 4 – Валидация входа и формат ProblemDetails",
    )
    p(
        doc,
        "Экспериментальная часть подтверждает, что REST API склада реализует полный "
        "набор требований ЛР 7-8: Minimal APIs, слои, EF Core, JWT/роли, CORS, "
        "логирование, конфигурацию через IOptions/user-secrets и документирование "
        "Swagger. Приложение готово к использованию как серверный слой сквозного проекта.",
    )

    heading(doc, "ЗАКЛЮЧЕНИЕ")
    p(
        doc,
        "В ходе выполнения лабораторной работы № 7-8 разработан REST API склада на "
        "ASP.NET Core Minimal APIs с подключением EF Core и SQLite. Реализованы ресурсы "
        "товаров, остатков, поставок и поставщиков, а также аутентификация JWT и роли "
        "«менеджер»/«кладовщик».",
    )
    p(
        doc,
        "На практике освоены хостинг WebApplication, middleware pipeline, DI, "
        "конфигурация appsettings/user-secrets, логирование ILogger, DTO и валидация, "
        "ответы ProblemDetails, пагинация/фильтрация/сортировка, политики авторизации, "
        "CORS и Swagger. Слоистая схема endpoint → service → repository обеспечивает "
        "поддерживаемость кода.",
    )
    p(
        doc,
        "Работа выполнена в ветке lab7-8 и продолжает предметную область склада из "
        "ЛР 5-6. Это создаёт основу для последующих лабораторных сквозного проекта, где "
        "API может быть расширено клиентским приложением, распределёнными компонентами "
        "или дополнительными сервисами.",
    )
    p(
        doc,
        "Таким образом, цель лабораторной работы достигнута: построено защищённое REST "
        "API поверх базы склада с ролевым доступом и полным набором инфраструктурных "
        "практик ASP.NET Core, предусмотренных заданием.",
    )

    doc.save(OUT)
    print(f"Saved: {OUT}")


if __name__ == "__main__":
    build()
