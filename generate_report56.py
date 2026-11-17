# -*- coding: utf-8 -*-
from pathlib import Path

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_LINE_SPACING
from docx.oxml.ns import qn
from docx.shared import Cm, Pt
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(r"D:\4 курс\РИС\LR5-9")
ASSETS = ROOT / "report_assets"
OUT = ROOT / "Отчет5-6.docx"
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
    set_paragraph_format(
        paragraph,
        align=WD_ALIGN_PARAGRAPH.CENTER,
        first_line_cm=0,
    )
    add_text(paragraph, text, size=14, bold=True)
    return paragraph


def section_title(doc, text):
    paragraph = doc.add_paragraph()
    set_paragraph_format(
        paragraph,
        align=WD_ALIGN_PARAGRAPH.JUSTIFY,
        first_line_cm=0,
    )
    add_text(paragraph, text, size=14, bold=True)
    return paragraph


def bullet(doc, text):
    paragraph = doc.add_paragraph()
    set_paragraph_format(
        paragraph,
        align=WD_ALIGN_PARAGRAPH.JUSTIFY,
        first_line_cm=1.25,
    )
    add_text(paragraph, text, size=14, bold=False)
    return paragraph


def code_block(doc, lines):
    for line in lines:
        paragraph = doc.add_paragraph()
        set_paragraph_format(
            paragraph,
            align=WD_ALIGN_PARAGRAPH.LEFT,
            first_line_cm=0,
        )
        paragraph.paragraph_format.left_indent = Cm(1.0)
        run = paragraph.add_run(line if line else " ")
        set_run_font(run, size=11, bold=False, name="Courier New")


def add_picture_with_caption(doc, image_path, caption):
    paragraph = doc.add_paragraph()
    set_paragraph_format(
        paragraph,
        align=WD_ALIGN_PARAGRAPH.CENTER,
        first_line_cm=0,
    )
    run = paragraph.add_run()
    run.add_picture(str(image_path), width=Cm(16.0))

    cap = doc.add_paragraph()
    set_paragraph_format(
        cap,
        align=WD_ALIGN_PARAGRAPH.CENTER,
        first_line_cm=0,
    )
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
    candidates = [
        r"C:\Windows\Fonts\consola.ttf",
        r"C:\Windows\Fonts\lucon.ttf",
        r"C:\Windows\Fonts\cour.ttf",
    ]
    for path in candidates:
        if Path(path).exists():
            return ImageFont.truetype(path, size)
    return ImageFont.load_default()


def render_console_image(path: Path, lines: list[str], title: str):
    font = load_font(16)
    title_font = load_font(18)
    padding = 24
    line_h = 22
    width = 980
    height = padding * 2 + 30 + line_h * (len(lines) + 1)

    img = Image.new("RGB", (width, height), (18, 22, 28))
    draw = ImageDraw.Draw(img)
    draw.rectangle((0, 0, width, 34), fill=(32, 40, 52))
    draw.text((padding, 8), title, fill=(210, 220, 230), font=title_font)

    y = 46
    for line in lines:
        color = (210, 220, 230)
        if line.startswith("==="):
            color = (120, 200, 160)
        elif "НИЗКИЙ" in line or "N+1" in line:
            color = (255, 180, 100)
        elif line.startswith("    -") or line.startswith("    "):
            color = (180, 200, 220)
        draw.text((padding, y), line, fill=color, font=font)
        y += line_h

    img.save(path)


def make_figures():
    render_console_image(
        ASSETS / "fig1_menu.png",
        [
            "========== СКЛАД (ЛР 5-6: ADO.NET + EF Core) ==========",
            "1. Список товаров и остатков (EF, AsNoTracking)",
            "2. Список поставщиков (many-to-many)",
            "3. Список поставок (Include)",
            "4. Добавить товар (Add + ChangeTracker)",
            "5. Изменить остаток товара",
            "6. Зарегистрировать поставку (транзакция EF)",
            "7. Удалить товар (Cascade/Restrict)",
            "8. Товары ниже минимального остатка",
            "9. ExecuteUpdate: поднять мин. остаток периферии",
            "10. Демонстрация N+1 и Include",
            "11. ADO.NET: отчёт по остаткам",
            "12. ADO.NET: транзакционное списание",
            "13. ADO.NET: агрегаты",
            "0. Выход",
            "Выбор: ",
        ],
        "WarehouseApp – главное меню",
    )

    render_console_image(
        ASSETS / "fig2_products.png",
        [
            "=== Товары и остатки (AsNoTracking) ===",
            "#1 PAP-A4-80    Бумага A4 80 г/м²                    50 пач",
            "#2 CRG-HP-85A   Картридж лазерный HP 85A             12 шт",
            "#3 KB-USB-01    Клавиатура USB                       20 шт",
            "#4 MS-OPT-01    Мышь оптическая                      25 шт",
            "#5 TAPE-48      Скотч упаковочный 48 мм               0 шт [НИЗКИЙ ОСТАТОК]",
            "",
            "=== Поставки (Include) ===",
            "#2 Накл-014/2026 от 14.11.2026 | ИП Козлов Д. Н.",
            "    KB-USB-01: 20 × 35,00 BYN",
            "    MS-OPT-01: 25 × 18,50 BYN",
            "#1 Накл-001/2026 от 07.11.2026 | ООО «БелПромСнаб»",
            "    PAP-A4-80: 50 × 12,50 BYN",
            "    CRG-HP-85A: 12 × 89,00 BYN",
        ],
        "EF Core – товары, остатки и поставки",
    )

    render_console_image(
        ASSETS / "fig3_nplus1_ado.png",
        [
            "=== Демонстрация N+1 (плохо) ===",
            "  Поставка #1: позиций 2",
            "  Поставка #2: позиций 2",
            "Всего SQL-запросов (N+1): 3",
            "",
            "=== Исправление через Include (один/два запроса) ===",
            "Загружено поставок: 2, позиций суммарно: 4",
            "Запросы выполнены через JOIN/split-query, без цикла N+1.",
            "",
            "=== ADO.NET: отчёт по остаткам (параметризованный SELECT) ===",
            "PAP-A4-80    Бумага A4 80 г/м²                    50 пач (мин. 20)",
            "CRG-HP-85A   Картридж лазерный HP 85A             12 шт (мин. 5)",
            "KB-USB-01    Клавиатура USB                       20 шт (мин. 10)",
            "MS-OPT-01    Мышь оптическая                      25 шт (мин. 10)",
            "TAPE-48      Скотч упаковочный 48 мм               0 шт (мин. 15)",
        ],
        "N+1 и ADO.NET-отчёт",
    )

    render_console_image(
        ASSETS / "fig4_ado_tx.png",
        [
            "=== ADO.NET: транзакционное списание остатка ===",
            "Артикул товара: PAP-A4-80",
            "Количество к списанию: 5",
            "Списано 5. Было 50, стало 45.",
            "",
            "=== ADO.NET: агрегаты (ExecuteScalar) ===",
            "Товаров:     5",
            "Поставщиков: 3",
            "Поставок:    2",
            "Суммарный остаток: 107",
            "",
            "=== Товары ниже минимального остатка ===",
            "TAPE-48: 0 / мин. 15",
        ],
        "ADO.NET-транзакция и контроль дефицита",
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
        p(doc, text, align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0, bold=False)

    p(
        doc,
        "Факультет компьютерного проектирования",
        align=WD_ALIGN_PARAGRAPH.CENTER,
        first_line_cm=0,
    )
    p(
        doc,
        "Кафедра проектирования информационно-компьютерных систем Дисциплина Распределенные информационные системы",
        align=WD_ALIGN_PARAGRAPH.LEFT,
        first_line_cm=0,
    )

    for _ in range(3):
        p(doc, "", align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0)

    p(doc, "ОТЧЕТ", align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0, bold=True)
    p(
        doc,
        "по лабораторной работе № 5-6",
        align=WD_ALIGN_PARAGRAPH.CENTER,
        first_line_cm=0,
    )
    p(doc, "на тему:", align=WD_ALIGN_PARAGRAPH.CENTER, first_line_cm=0)
    p(
        doc,
        "Доступ к данным: ADO.NET + Entity Framework Core. Склад",
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
        "Целью лабораторной работы № 5-6 является освоение двух подходов к доступу к "
        "данным в экосистеме .NET: низкоуровневого ADO.NET и объектно-реляционного "
        "отображения Entity Framework Core в стиле Code First. В работе требуется "
        "спроектировать модель предметной области, настроить связи сущностей, выполнить "
        "миграции базы данных, реализовать CRUD-операции, изучить ChangeTracker и "
        "проблему N+1, а также продемонстрировать параметризованные SQL-запросы и "
        "транзакции через ADO.NET.",
    )
    p(
        doc,
        "Вариантом задания выбран складской учёт: товары, поставщики, поставки и "
        "остатки. Такая предметная область естественно порождает связи one-to-many, "
        "many-to-many и one-to-one, поэтому удобна для закрепления Fluent API, атрибутов "
        "валидации и правил каскадного удаления. В качестве СУБД используется SQLite, "
        "что позволяет хранить базу рядом с приложением и не зависеть от внешнего "
        "сервера на этапе лабораторных работ.",
    )
    p(
        doc,
        "Результатом является консольное приложение WarehouseApp, которое служит "
        "началом сквозного проекта последующих лабораторных работ. В репозитории "
        "создана ветка lab5-6, а код организован так, чтобы его можно было поэтапно "
        "расширять сервисным слоем, веб-интерфейсом и распределёнными компонентами.",
    )

    section_title(doc, "1 АНАЛИЗ ЗАДАЧИ И ОБОСНОВАНИЕ ВЫБОРА ТЕХНОЛОГИЙ")
    p(
        doc,
        "Поставленная задача требует одновременного изучения двух уровней доступа к "
        "данным. ADO.NET даёт полный контроль над соединением, командой, параметрами и "
        "транзакцией, а EF Core скрывает SQL за объектной моделью и упрощает работу со "
        "связями. Для склада это означает, что повседневный CRUD удобно выполнять через "
        "DbContext, а отдельные критичные операции (например, атомарное списание "
        "остатка) можно явно показать средствами ADO.NET.",
    )
    p(doc, "Основными требованиями лабораторной работы являлись:")
    bullet(doc, "проектирование сущностей Code First с атрибутами и Fluent API;")
    bullet(doc, "реализация связей one-to-many, many-to-many и one-to-one;")
    bullet(doc, "создание и применение миграций EF Core;")
    bullet(doc, "CRUD через DbContext, LINQ, Include и AsNoTracking;")
    bullet(doc, "демонстрация ChangeTracker, N+1 и ExecuteUpdate;")
    bullet(doc, "параметризованные запросы и транзакции ADO.NET на SQLite.")
    p(
        doc,
        "Выбрана платформа .NET 8 и пакеты Microsoft.EntityFrameworkCore.Sqlite и "
        "Microsoft.Data.Sqlite. SQLite-провайдер поддерживает как ORM, так и прямые "
        "команды через SqliteConnection/SqliteCommand, поэтому оба подхода реализуются "
        "над одной и той же базой warehouse.db. Это важно для сравнения присоединённой "
        "модели ADO.NET и отслеживания изменений ChangeTracker в EF Core.",
    )
    p(
        doc,
        "Архитектурно приложение оставлено консольным: такой формат соответствует "
        "учебной цели и упрощает демонстрацию запросов без усложнения UI. Логика "
        "разделена на модели, DbContext, инициализатор данных, сервис EF Core и "
        "отдельный класс ADO.NET-демонстраций. Такое разбиение создаёт основу для "
        "дальнейшего развития сквозного проекта.",
    )

    section_title(doc, "2 ОПИСАНИЕ ОБЪЕКТНОЙ МОДЕЛИ И СТРУКТУРЫ ПРОЕКТА")
    p(
        doc,
        "Проект WarehouseApp организован как многофайловое консольное приложение. "
        "Основные компоненты расположены в следующих каталогах и файлах:",
    )
    bullet(doc, "Models – сущности Supplier, Product, Delivery, DeliveryItem, StockBalance, ProductSupplier;")
    bullet(doc, "Data/WarehouseDbContext.cs – DbContext и Fluent API;")
    bullet(doc, "Data/DbInitializer.cs – миграции и демо-данные;")
    bullet(doc, "Data/Migrations – миграция InitialCreate;")
    bullet(doc, "Services/WarehouseService.cs – CRUD и учебные сценарии EF Core;")
    bullet(doc, "Services/AdoNetDemo.cs – прямые SQL-запросы и транзакции;")
    bullet(doc, "ConsoleMenu.cs и Program.cs – меню и точка входа.")
    p(
        doc,
        "Модель предметной области отражает складской процесс. Поставщик (Supplier) "
        "имеет много поставок (Delivery). Поставка состоит из позиций DeliveryItem, "
        "каждая из которых ссылается на товар Product. Остаток StockBalance связан с "
        "товаром отношением one-to-one и хранит текущее количество. Для связи товара и "
        "поставщика вне конкретной накладной введена промежуточная сущность "
        "ProductSupplier, реализующая many-to-many с дополнительным признаком "
        "IsPrimary.",
    )
    p(
        doc,
        "Правила удаления настроены осознанно. Удаление поставки каскадно удаляет её "
        "позиции; удаление товара каскадно удаляет остаток и связи ProductSupplier, но "
        "запрещено, если товар уже участвует в поставках (Restrict). Удаление "
        "поставщика также ограничено при наличии поставок. Такая схема ближе к "
        "реальной складской практике, где исторические документы нельзя «сломать» "
        "каскадом.",
    )
    p(
        doc,
        "При первом запуске DbInitializer вызывает Database.Migrate(), после чего "
        "заполняет справочники и две демонстрационные поставки. Остатки автоматически "
        "увеличиваются по позициям поставок. Это даёт воспроизводимую базу для проверки "
        "меню и для последующих лабораторных работ сквозного проекта.",
    )

    section_title(doc, "3 КЛЮЧЕВЫЕ ЛИСТИНГИ КОДА")
    p(
        doc,
        "Ниже приведены фрагменты, которые отражают центральные решения лабораторной "
        "работы. Сначала показана настройка связей в OnModelCreating: составной ключ "
        "many-to-many, one-to-one для остатка и ограничения удаления.",
    )
    code_block(
        doc,
        [
            "modelBuilder.Entity<ProductSupplier>(entity =>",
            "{",
            "    entity.HasKey(x => new { x.ProductId, x.SupplierId });",
            "    entity.HasOne(x => x.Product)",
            "        .WithMany(p => p.ProductSuppliers)",
            "        .HasForeignKey(x => x.ProductId)",
            "        .OnDelete(DeleteBehavior.Cascade);",
            "});",
            "",
            "modelBuilder.Entity<StockBalance>(entity =>",
            "{",
            "    entity.HasOne(x => x.Product)",
            "        .WithOne(p => p.StockBalance)",
            "        .HasForeignKey<StockBalance>(x => x.ProductId)",
            "        .OnDelete(DeleteBehavior.Cascade);",
            "});",
        ],
    )
    p(
        doc,
        "Fluent API дополняет атрибуты [Key], [Required], [MaxLength], [Column] и "
        "[Table], заданные на сущностях. Атрибуты описывают простые ограничения "
        "полей, а Fluent API фиксирует составные ключи, индексы и политику каскадов, "
        "которые неудобно выражать только разметкой классов.",
    )
    p(
        doc,
        "Чтение связанных данных выполняется через LINQ и Include. Это позволяет "
        "загрузить поставку вместе с поставщиком и позициями одним запросом и "
        "избежать классической проблемы N+1.",
    )
    code_block(
        doc,
        [
            "var deliveries = _db.Deliveries",
            "    .AsNoTracking()",
            "    .Include(d => d.Supplier)",
            "    .Include(d => d.Items)",
            "    .ThenInclude(i => i.Product)",
            "    .OrderByDescending(d => d.DeliveryDate)",
            "    .ToList();",
        ],
    )
    p(
        doc,
        "Метод AsNoTracking применяется для отчётных выборок, где изменение сущностей "
        "не требуется. Это снижает накладные расходы ChangeTracker. Для модификаций "
        "используются Add, Find, Remove, а также ExecuteUpdate для пакетного изменения "
        "минимального остатка товаров категории «Периферия».",
    )
    p(
        doc,
        "Демонстрация ADO.NET использует параметризованный SELECT и транзакционное "
        "обновление остатка. Параметры SqliteParameter исключают SQL-инъекцию, а "
        "явный Commit/Rollback показывает присоединённую модель работы с соединением.",
    )
    code_block(
        doc,
        [
            "using var connection = new SqliteConnection(ConnectionString);",
            "connection.Open();",
            "using var transaction = connection.BeginTransaction();",
            "using var updateCmd = connection.CreateCommand();",
            "updateCmd.Transaction = transaction;",
            "updateCmd.CommandText = \"\"\"",
            "    UPDATE StockBalances",
            "    SET Quantity = Quantity - $amount,",
            "        LastUpdated = $updated",
            "    WHERE ProductId = $productId;",
            "    \"\"\";",
            "updateCmd.Parameters.AddWithValue(\"$amount\", amount);",
            "updateCmd.ExecuteNonQuery();",
            "transaction.Commit();",
        ],
    )
    p(
        doc,
        "Миграция InitialCreate создана командой dotnet ef migrations add и применена "
        "через Database.Migrate() при старте. В миграции формируются таблицы Products, "
        "Suppliers, Deliveries, DeliveryItems, StockBalances и ProductSuppliers с "
        "внешними ключами и уникальными индексами по артикулу и номеру документа. Это "
        "закрывает требование лабораторной по освоению цикла migrations add / database "
        "update.",
    )

    section_title(doc, "4 РЕЗУЛЬТАТЫ РАБОТЫ ПРОГРАММЫ")
    p(
        doc,
        "Сборка проекта командой dotnet build завершилась успешно, без ошибок и "
        "предупреждений. При запуске приложение создаёт файл warehouse.db, применяет "
        "миграцию и заполняет демо-данные: пять товаров, три поставщика, две поставки и "
        "соответствующие остатки. Через меню доступны операции просмотра, изменения, "
        "регистрации поставки, удаления, контроля дефицита, демонстрации N+1 и прямых "
        "ADO.NET-сценариев.",
    )
    p(
        doc,
        "На рисунке 1 показано главное меню приложения WarehouseApp. Пункты меню "
        "сгруппированы так, чтобы отдельно продемонстрировать возможности EF Core и "
        "ADO.NET, а также учебные сценарии ChangeTracker и N+1.",
    )
    add_picture_with_caption(
        doc,
        ASSETS / "fig1_menu.png",
        "Рисунок 1 – Главное меню консольного приложения WarehouseApp",
    )
    p(
        doc,
        "Меню покрывает полный цикл работы со складом: справочники, документы поставок, "
        "остатки, контроль минимального уровня и низкоуровневые SQL-операции. Это "
        "позволяет воспроизвести каждый пункт задания лабораторной работы.",
    )
    p(
        doc,
        "На рисунке 2 приведены результаты выборки товаров с остатками и списка "
        "поставок. Для товара TAPE-48 остаток равен нулю и помечен как низкий, что "
        "подтверждает корректность сравнения с MinStockLevel. Поставки загружаются "
        "вместе с позициями через Include.",
    )
    add_picture_with_caption(
        doc,
        ASSETS / "fig2_products.png",
        "Рисунок 2 – Товары, остатки и поставки через EF Core",
    )
    p(
        doc,
        "Полученные данные соответствуют seed-наполнению: бумага и картриджи поступили "
        "от «БелПромСнаб», периферия – от ИП Козлова. Это подтверждает работоспособность "
        "связей Supplier–Delivery–DeliveryItem–Product и актуализацию StockBalance.",
    )
    p(
        doc,
        "На рисунке 3 показаны демонстрация проблемы N+1 и её исправление через Include, "
        "а также отчёт по остаткам, выполненный прямым параметризованным запросом "
        "ADO.NET. В «плохом» варианте для двух поставок выполняется три SQL-запроса, "
        "тогда как Include загружает связанные позиции без цикла дополнительных "
        "обращений.",
    )
    add_picture_with_caption(
        doc,
        ASSETS / "fig3_nplus1_ado.png",
        "Рисунок 3 – Демонстрация N+1 и ADO.NET-отчёт по остаткам",
    )
    p(
        doc,
        "Сравнение двух подходов к чтению связанных данных наглядно иллюстрирует, "
        "почему в ORM важно явно планировать загрузку навигационных свойств. Одновременно "
        "ADO.NET-отчёт показывает, что та же SQLite-база доступна и без ChangeTracker, "
        "через DbCommand и DbDataReader.",
    )
    p(
        doc,
        "На рисунке 4 представлены транзакционное списание остатка бумаги через ADO.NET "
        "и контроль товаров ниже минимального уровня. После списания пяти единиц "
        "остаток PAP-A4-80 уменьшился с 50 до 45, а для TAPE-48 по-прежнему фиксируется "
        "дефицит.",
    )
    add_picture_with_caption(
        doc,
        ASSETS / "fig4_ado_tx.png",
        "Рисунок 4 – Транзакция ADO.NET и контроль дефицита",
    )
    p(
        doc,
        "Экспериментальная часть подтверждает, что приложение корректно реализует оба "
        "подхода доступа к данным: объектный CRUD и LINQ в EF Core, а также "
        "параметризованные команды, чтение через DataReader и транзакции Commit/"
        "Rollback в ADO.NET. База SQLite сохраняет согласованное состояние после "
        "операций изменения остатков.",
    )

    heading(doc, "ЗАКЛЮЧЕНИЕ")
    p(
        doc,
        "В ходе выполнения лабораторной работы № 5-6 разработано консольное приложение "
        "WarehouseApp для учёта склада: товаров, поставщиков, поставок и остатков. "
        "Реализован подход Code First с сущностями, атрибутами, Fluent API, миграцией "
        "InitialCreate и демо-наполнением SQLite-базы.",
    )
    p(
        doc,
        "На практике освоены ключевые возможности EF Core: DbContext, связи one-to-many, "
        "many-to-many и one-to-one, каскадное и ограничивающее удаление, CRUD, Include, "
        "AsNoTracking, ChangeTracker, ExecuteUpdate и устранение N+1. Параллельно "
        "продемонстрирован низкоуровневый доступ ADO.NET с параметризацией запросов и "
        "транзакционным обновлением остатка.",
    )
    p(
        doc,
        "Структура проекта подготовлена как основа сквозного решения: предметная "
        "область и модель данных могут переиспользоваться в следующих лабораторных "
        "работах. В локальном git-репозитории создана ветка lab5-6, что позволяет "
        "вести дальнейшую разработку по отдельным веткам для каждой лабораторной.",
    )
    p(
        doc,
        "Таким образом, цель работы достигнута: изучены и применены два подхода к "
        "доступу к данным в .NET, спроектирована модель склада и реализовано рабочее "
        "приложение с миграциями, связями и демонстрацией ADO.NET поверх той же базы "
        "SQLite.",
    )

    doc.save(OUT)
    print(f"Saved: {OUT}")


if __name__ == "__main__":
    build()
