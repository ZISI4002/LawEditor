using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace LawEditor.Services.WordServises {
    public class WordFileWritingService {
        private static long _drawingIdCounter = 1;

        public bool CompareEndnoteLists(List<string> list1, List<string> list2)
        {
            var set1 = new HashSet<string>(list1);
            var set2 = new HashSet<string>(list2);
            return set1.IsSubsetOf(set2);
        }

        public void WriteWordFile(string filePath, Laws laws) {
            List<string> allEndnoteIdesFromLaws = new List<string>();
            foreach (var chapter in laws.Chapters) {
                foreach (var section in chapter.Sections) {
                    foreach (var article in section.Articles) {
                        if (!string.IsNullOrEmpty(article.EndnoteId))
                            allEndnoteIdesFromLaws.Add(article.EndnoteId);

                        foreach (var clause in article.Clauses) {
                            if (!string.IsNullOrEmpty(clause.EndnoteId))
                                allEndnoteIdesFromLaws.Add(clause.EndnoteId);

                            foreach (var sub in clause.SubClauses) {
                                if (!string.IsNullOrEmpty(sub.EndnoteId))
                                    allEndnoteIdesFromLaws.Add(sub.EndnoteId);
                            }
                        }
                    }
                }
            }
            List<string> allIdesFromAmendments = new List<string>();
            var allAmendments = laws.SourcesData.FirstOrDefault(s => s.Id == 3);
            if (allAmendments != null) {
                foreach (var item in allAmendments.Source.OfType<ConstitutionalAmendment>()) {
                    allIdesFromAmendments.Add(item.Id);
                }
            }

            if (CompareEndnoteLists(allEndnoteIdesFromLaws, allIdesFromAmendments) == false) {
                MessageBox.Show("Endnote ID-ləri Konstitusiyaya edilmiş dəyişiklik və əlavələrin siyahısıyla üst-üstə düşmür. Unikal ID-lər təmin edilməlidir.", "Xəta", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try {
                using var doc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);

                var mainPart = doc.AddMainDocumentPart();

                var numberingPart = mainPart.AddNewPart<NumberingDefinitionsPart>();
                numberingPart.Numbering = new Numbering(
                    new AbstractNum(
                        new Level(
                            new NumberingFormat() { Val = NumberFormatValues.Decimal },
                            new LevelText() { Val = "%1." },
                            new StartNumberingValue() { Val = 1 }
                        ) { LevelIndex = 0 }
                    ) { AbstractNumberId = 1 },
                    new NumberingInstance(
                        new AbstractNumId() { Val = 1 }
                    ) { NumberID = 1 }
                );

                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                // ── Создаём EndnotesPart сразу, всегда ──
                var endnotePart = mainPart.AddNewPart<EndnotesPart>();
                endnotePart.Endnotes = new Endnotes();

                endnotePart.Endnotes.Append(new Endnote(
                    new Paragraph(new Run(new SeparatorMark()))) {
                    Type = FootnoteEndnoteValues.Separator,
                    Id = -1
                });
                endnotePart.Endnotes.Append(new Endnote(
                    new Paragraph(new Run(new ContinuationSeparatorMark()))) {
                    Type = FootnoteEndnoteValues.ContinuationSeparator,
                    Id = 0
                });

                var stylePart = mainPart.AddNewPart<StyleDefinitionsPart>();
                stylePart.Styles = new Styles();

                // ── Строим маппинг: amendmentId (строка) → int endnoteId ──
                // Числовые ID остаются как есть, нечисловые (KM1, KQ1...) получают
                // уникальные ID начиная с 1001 чтобы не конфликтовать с числовыми.
                var amendments = laws.SourcesData.FirstOrDefault(s => s.Id == 3);
                var idMapping = BuildIdMapping(amendments);

                // ── HEADER ──
                if (laws.UpperObjects.Count > 0 && laws.UpperObjects[0].Headers.Count > 0) {
                    var headerText = laws.UpperObjects[0].Headers[0].FullText;
                    if (!string.IsNullOrWhiteSpace(headerText)) {
                        var lines = headerText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in lines)
                            body.Append(CreateParagraph(line, align: JustificationValues.Center, indent: false));
                    }
                    body.Append(CreateEmptyParagraph());
                }

                // ── CHAPTERS ──
                foreach (var chapter in laws.Chapters) {
                    string chapterOrdinal = ToAzerbaijaniOrdinal(chapter.Id);

                    body.Append(CreateParagraph($"{chapterOrdinal} BÖLMƏ",
                        bold: true, align: JustificationValues.Center, indent: false));

                    if (!string.IsNullOrEmpty(chapter.Title))
                        body.Append(CreateParagraph(chapter.Title,
                            bold: true, align: JustificationValues.Center, indent: false));

                    if (chapter.Table != null) {
                        var chapterTable = chapter.Table;
                        AppendTable(body, chapterTable.Title, chapterTable.Headers,
                            chapterTable.Rows.Select(r => r.Cells));
                    }

                    if (chapter.Image != null) {
                        var chapterImage = chapter.Image;
                        AppendImage(mainPart, body, chapterImage.FilePath, chapterImage.Title);
                    }

                    body.Append(CreateEmptyParagraph());

                    foreach (var section in chapter.Sections) {
                        string sectionRoman = ToRoman(section.Id);

                        body.Append(CreateParagraph($"{sectionRoman} fəsil",
                            align: JustificationValues.Center, indent: false));

                        if (!string.IsNullOrEmpty(section.Title))
                            body.Append(CreateParagraph(section.Title,
                                align: JustificationValues.Center, indent: false));

                        if (section.Table != null) {
                            var sectionTable = section.Table;
                            AppendTable(body, sectionTable.Title, sectionTable.Headers,
                                sectionTable.Rows.Select(r => r.Cells));
                        }

                        if (section.Image != null) {
                            var sectionImage = section.Image;
                            AppendImage(mainPart, body, sectionImage.FilePath, sectionImage.Title);
                        }

                        body.Append(CreateEmptyParagraph());

                        foreach (var article in section.Articles) {
                            string articleId = FormatArticleId(article.Id);

                            var articlePara = CreateParagraph(
                                $"Maddə {articleId}. {article.Title}",
                                bold: true,
                                align: JustificationValues.Both,
                                indent: true
                            );
                            AppendEndnoteRef(articlePara, article.EndnoteId, idMapping);
                            body.Append(articlePara);

                            if (article.Table != null) {
                                var articleTable = article.Table;
                                AppendTable(body, articleTable.Title, articleTable.Headers,
                                    articleTable.Rows.Select(r => r.Cells));
                            }

                            if (article.Image != null) {
                                var articleImage = article.Image;
                                AppendImage(mainPart, body, articleImage.FilePath, articleImage.Title);
                            }

                            body.Append(CreateEmptyParagraph());

                            foreach (var clause in article.Clauses) {
                                Paragraph clausePara;

                                if (clause.Number > 0)
                                    clausePara = CreateParagraphWithEndnote(
                                        $"{ToRoman(clause.Number)}. {clause.Text}",
                                        clause.EndnoteId, idMapping);
                                else if (!string.IsNullOrEmpty(clause.Text))
                                    clausePara = CreateParagraphWithEndnote(
                                        clause.Text,
                                        clause.EndnoteId, idMapping);
                                else continue;

                                body.Append(clausePara);

                                if (clause.Table != null) {
                                    var clauseTable = clause.Table;
                                    AppendTable(body, clauseTable.Title, clauseTable.Headers,
                                        clauseTable.Rows.Select(r => r.Cells));
                                }

                                if (clause.Image != null) {
                                    var clauseImage = clause.Image;
                                    AppendImage(mainPart, body, clauseImage.FilePath, clauseImage.Title);
                                }

                                foreach (var sub in clause.SubClauses) {
                                    var subPara = CreateParagraphWithEndnote(
                                        $"{sub.Number}) {sub.Text}",
                                        sub.EndnoteId, idMapping);
                                    body.Append(subPara);

                                    if (sub.Table != null) {
                                        var subTable = sub.Table;
                                        AppendTable(body, subTable.Title, subTable.Headers,
                                            subTable.Rows.Select(r => r.Cells));
                                    }

                                    if (sub.Image != null) {
                                        var subImage = sub.Image;
                                        AppendImage(mainPart, body, subImage.FilePath, subImage.Title);
                                    }
                                }
                            }

                            body.Append(CreateEmptyParagraph());
                        }
                    }
                }

                // ── TRANSITIONAL ──
                var transitional = laws.SourcesData.FirstOrDefault(s => s.Id == 1);

                if (transitional?.Source.Count > 0) {
                    body.Append(CreateParagraph("KEÇİD MÜDDƏALARI",
                        bold: true, align: JustificationValues.Center, indent: false));
                    body.Append(CreateEmptyParagraph());

                    foreach (var item in transitional.Source.OfType<TransitionalProvisions>())
                        body.Append(CreateParagraph($"{item.Id}. {item.Title}",
                            align: JustificationValues.Both));

                    body.Append(new Paragraph(new Run(new Text(""))));

                    if (!string.IsNullOrWhiteSpace(TransitionalProvisions.Date)) {
                        var para = CreateParagraph("",
                            bold: true, align: JustificationValues.Start, indent: false);

                        var run = para.GetFirstChild<Run>();
                        run.RemoveAllChildren<Text>();

                        var dateLines = TransitionalProvisions.Date.Split('\n');
                        for (int i = 0; i < dateLines.Length; i++) {
                            run.Append(new Text(dateLines[i]) {
                                Space = SpaceProcessingModeValues.Preserve
                            });
                            if (i < dateLines.Length - 1)
                                run.Append(new Break());
                        }

                        body.Append(para);
                    }

                    body.Append(CreateEmptyParagraph());
                }

                // ── SOURCES ──
                var sources = laws.SourcesData.FirstOrDefault(s => s.Id == 2);

                if (sources?.Source.Count > 0) {
                    body.Append(CreateParagraph(
                        "İSTİFADƏ OLUNMUŞ MƏNBƏ SƏNƏDLƏRİNİN SİYAHISI",
                        bold: true, align: JustificationValues.Center, indent: false, fontSize: "20"));

                    body.Append(CreateEmptyParagraph());

                    foreach (var item in sources.Source.OfType<SourceDocumentsList>()) {
                        var para = new Paragraph();

                        var pPr = new ParagraphProperties(
                            new NumberingProperties(
                                new NumberingLevelReference() { Val = 0 },
                                new NumberingId() { Val = 1 }
                            ),
                            new Indentation() { Left = "720", Hanging = "360" },
                            new Justification() { Val = JustificationValues.Both }
                        );
                        para.Append(pPr);

                        RunProperties baseProps = new RunProperties(
                            new RunFonts() {
                                Ascii = "Palatino Linotype",
                                HighAnsi = "Palatino Linotype",
                                ComplexScript = "Palatino Linotype"
                            },
                            new FontSize() { Val = "20" }
                        );

                        if (!string.IsNullOrEmpty(item.LinkText) && !string.IsNullOrEmpty(item.Url)) {
                            var rel = mainPart.AddHyperlinkRelationship(new Uri(item.Url), true);
                            var hyperlink = new Hyperlink() { Id = rel.Id };

                            hyperlink.Append(new Run(
                                new RunProperties(
                                    new Underline() { Val = UnderlineValues.Single },
                                    new Color() { Val = "0000FF" },
                                    new FontSize() { Val = "20" }
                                ),
                                new Text(item.LinkText) {
                                    Space = SpaceProcessingModeValues.Preserve
                                }
                            ));

                            para.Append(hyperlink);
                            para.Append(new Run(new Text(" ") {
                                Space = SpaceProcessingModeValues.Preserve
                            }));

                            string rest = item.Title.Replace(item.LinkText, "").Trim();
                            if (!string.IsNullOrEmpty(rest)) {
                                para.Append(new Run(
                                    (RunProperties)baseProps.CloneNode(true),
                                    new Text(rest) { Space = SpaceProcessingModeValues.Preserve }
                                ));
                            }
                        }
                        else {
                            para.Append(new Run(baseProps,
                                new Text(item.Title) { Space = SpaceProcessingModeValues.Preserve }));
                        }

                        body.Append(para);
                    }

                    body.Append(CreateEmptyParagraph());
                }

                // ── AMENDMENTS (ENDNOTES) ──
                if (amendments?.Source.Count > 0) {
                    body.Append(CreateParagraph(
                         "KONSTİTUSİYAYA EDİLMİŞ DƏYİŞİKLİK VƏ ƏLAVƏLƏRİN SİYAHISI",
                         bold: true, align: JustificationValues.Center, indent: false, fontSize: "20"));
                    foreach (var item in amendments.Source.OfType<ConstitutionalAmendment>()) {
                        if (string.IsNullOrWhiteSpace(item.Title)) continue;

                        if (!idMapping.TryGetValue(item.Id, out int finalId))
                            continue;

                        bool isNumeric = int.TryParse(item.Id, out _);
                        string prefix = isNumeric ? "" : item.Id + " ";

                        var endnotePara = new Paragraph();

                        var refMark = new Run(new EndnoteReferenceMark());
                        refMark.RunProperties = new RunProperties(
                            new VerticalTextAlignment { Val = VerticalPositionValues.Superscript });
                        endnotePara.Append(refMark);

                        if (!string.IsNullOrEmpty(item.LinkText) && !string.IsNullOrEmpty(item.Url)) {
                            string titleText = prefix + item.Title;
                            int linkIndex = titleText.IndexOf(item.LinkText, StringComparison.Ordinal);

                            string before = linkIndex > 0
                                ? titleText.Substring(0, linkIndex)
                                : "";

                            endnotePara.Append(new Run(
                                new Text((before.Length > 0 ? " " + before : " ")) {
                                    Space = SpaceProcessingModeValues.Preserve
                                }
                            ));

                            var endnoteRel = endnotePart.AddHyperlinkRelationship(
                                new Uri(item.Url), true);

                            var hyperlink = new Hyperlink() { Id = endnoteRel.Id };
                            hyperlink.Append(new Run(
                                new RunProperties(
                                    new Underline() { Val = UnderlineValues.Single },
                                    new Color() { Val = "0000FF" }
                                ),
                                new Text(item.LinkText) {
                                    Space = SpaceProcessingModeValues.Preserve
                                }
                            ));
                            endnotePara.Append(hyperlink);

                            string after = linkIndex >= 0
                                ? titleText.Substring(linkIndex + item.LinkText.Length).Trim()
                                : "";

                            if (!string.IsNullOrEmpty(after)) {
                                endnotePara.Append(new Run(
                                    new Text(" " + after) {
                                        Space = SpaceProcessingModeValues.Preserve
                                    }
                                ));
                            }
                        }
                        else {
                            string fullText = prefix + item.Title;
                            endnotePara.Append(new Run(
                                new Text(" " + fullText) {
                                    Space = SpaceProcessingModeValues.Preserve
                                }
                            ));
                        }

                        endnotePart.Endnotes.Append(new Endnote(endnotePara) { Id = finalId });
                    }
                }

                // ── СОХРАНЕНИЕ ──
                endnotePart.Endnotes.Save();
                mainPart.Document.Save();
            }
            catch (IOException ex) {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }

        }

        // ── Строим маппинг amendmentId → int endnoteId ──────────────────────
        private Dictionary<string, int> BuildIdMapping(
            Models.ChangableSourse.SourceData? amendments) {
            var mapping = new Dictionary<string, int>();
            if (amendments == null) return mapping;

            int syntheticId = 1001;

            foreach (var item in amendments.Source.OfType<ConstitutionalAmendment>()) {
                if (string.IsNullOrWhiteSpace(item.Id)) continue;
                if (mapping.ContainsKey(item.Id)) continue;

                if (int.TryParse(item.Id, out int numericId))
                    mapping[item.Id] = numericId;
                else
                    mapping[item.Id] = syntheticId++;
            }

            return mapping;
        }

        // ── ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ──────────────────────────────────────────

        private Paragraph CreateParagraph(
            string text,
            bool bold = false,
            JustificationValues? align = null,
            bool indent = true,
            string fontSize = "24") {
            var para = new Paragraph();
            var justification = align ?? JustificationValues.Start;

            var spacing = new SpacingBetweenLines() {
                Line = "240",
                LineRule = LineSpacingRuleValues.Auto,
                Before = "0",
                After = "0"
            };

            var pPr = new ParagraphProperties();
            pPr.Append(new Justification() { Val = justification });
            pPr.Append(spacing);

            if (indent)
                pPr.Append(new Indentation() { FirstLine = "720" });

            para.Append(pPr);

            var run = new Run();
            var runProps = new RunProperties(
                new RunFonts() {
                    Ascii = "Palatino Linotype",
                    HighAnsi = "Palatino Linotype",
                    ComplexScript = "Palatino Linotype"
                },
                new FontSize() { Val = fontSize }
            );

            if (bold)
                runProps.Append(new Bold());

            run.RunProperties = runProps;
            run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
            para.Append(run);

            return para;
        }

        private Paragraph CreateParagraphWithEndnote(
            string text, string? endnoteId,
            Dictionary<string, int> idMapping) {
            var para = CreateParagraph(text, align: JustificationValues.Both);
            AppendEndnoteRef(para, endnoteId, idMapping);
            return para;
        }

        private void AppendEndnoteRef(
            Paragraph para, string? id,
            Dictionary<string, int> idMapping) {
            if (string.IsNullOrEmpty(id)) return;
            if (!idMapping.TryGetValue(id, out int refId)) return;

            var run = new Run(new EndnoteReference { Id = refId });
            run.RunProperties = new RunProperties(
                new VerticalTextAlignment { Val = VerticalPositionValues.Superscript });

            para.Append(run);
        }

        // ── ТАБЛИЦЫ ───────────────────────────────────────────────────────

        private void AppendTable(
            Body body, string? title,
            IEnumerable<string> headers,
            IEnumerable<IEnumerable<string>> rows) {
            var table = BuildOpenXmlTable(headers, rows);
            if (table == null) return;

            if (!string.IsNullOrWhiteSpace(title))
                body.Append(CreateCaptionParagraph(title));

            body.Append(table);
            body.Append(CreateEmptyParagraph());
        }

        private DocumentFormat.OpenXml.Wordprocessing.Table? BuildOpenXmlTable(
            IEnumerable<string> headers,
            IEnumerable<IEnumerable<string>> rows) {
            var headerList = headers?.ToList() ?? new List<string>();
            var rowsList = (rows ?? Enumerable.Empty<IEnumerable<string>>())
                .Select(r => r?.ToList() ?? new List<string>())
                .ToList();

            int columnCount = Math.Max(
                headerList.Count,
                rowsList.Count > 0 ? rowsList.Max(r => r.Count) : 0);

            if (columnCount == 0)
                return null;

            var table = new DocumentFormat.OpenXml.Wordprocessing.Table();

            table.Append(new TableProperties(
                new TableBorders(
                    new TopBorder() { Val = BorderValues.Single, Size = 6 },
                    new BottomBorder() { Val = BorderValues.Single, Size = 6 },
                    new LeftBorder() { Val = BorderValues.Single, Size = 6 },
                    new RightBorder() { Val = BorderValues.Single, Size = 6 },
                    new InsideHorizontalBorder() { Val = BorderValues.Single, Size = 6 },
                    new InsideVerticalBorder() { Val = BorderValues.Single, Size = 6 }
                ),
                new TableWidth() { Type = TableWidthUnitValues.Auto }
            ));

            var grid = new TableGrid();
            for (int i = 0; i < columnCount; i++)
                grid.Append(new GridColumn());
            table.Append(grid);

            if (headerList.Count > 0)
                table.Append(BuildTableRow(headerList, columnCount, bold: true));

            foreach (var row in rowsList)
                table.Append(BuildTableRow(row, columnCount, bold: false));

            return table;
        }

        private TableRow BuildTableRow(List<string> cells, int columnCount, bool bold) {
            var tr = new TableRow();

            for (int i = 0; i < columnCount; i++) {
                string text = i < cells.Count ? (cells[i] ?? string.Empty) : string.Empty;

                var tc = new TableCell();
                tc.Append(new TableCellProperties(
                    new TableCellWidth() { Type = TableWidthUnitValues.Auto }));

                var para = new Paragraph(
                    new ParagraphProperties(new Justification() { Val = JustificationValues.Both }));

                var run = new Run();
                var runProps = new RunProperties(
                    new RunFonts() {
                        Ascii = "Palatino Linotype",
                        HighAnsi = "Palatino Linotype",
                        ComplexScript = "Palatino Linotype"
                    },
                    new FontSize() { Val = "20" }
                );
                if (bold)
                    runProps.Append(new Bold());

                run.RunProperties = runProps;
                run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
                para.Append(run);

                tc.Append(para);
                tr.Append(tc);
            }

            return tr;
        }

        // ── КАРТИНКИ ──────────────────────────────────────────────────────

        private void AppendImage(
            MainDocumentPart mainPart, Body body,
            string? filePath, string? title) {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return;

            try {
                var imagePart = mainPart.AddImagePart(GetImagePartType(filePath));
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    imagePart.FeedData(stream);

                string relationshipId = mainPart.GetIdOfPart(imagePart);
                (long widthEmu, long heightEmu) = GetImageSizeEmu(filePath);

                if (!string.IsNullOrWhiteSpace(title))
                    body.Append(CreateCaptionParagraph(title));

                string drawingName = Path.GetFileNameWithoutExtension(filePath);
                body.Append(BuildImageParagraph(relationshipId, widthEmu, heightEmu, drawingName));
                body.Append(CreateEmptyParagraph());
            }
            catch (Exception ex) {
                MessageBox.Show($"Şəkil əlavə edilərkən xəta baş verdi ({filePath}): {ex.Message}");
            }
        }

        private (long widthEmu, long heightEmu) GetImageSizeEmu(
            string filePath, long maxWidthEmu = 5486400) // ~6 дюймов
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(Path.GetFullPath(filePath), UriKind.Absolute);
            bitmap.EndInit();

            double pixelWidth = bitmap.PixelWidth;
            double pixelHeight = bitmap.PixelHeight;
            double dpiX = bitmap.DpiX > 0 ? bitmap.DpiX : 96;
            double dpiY = bitmap.DpiY > 0 ? bitmap.DpiY : 96;

            long widthEmu = (long)(pixelWidth / dpiX * 914400);
            long heightEmu = (long)(pixelHeight / dpiY * 914400);

            if (widthEmu > maxWidthEmu && widthEmu > 0) {
                double scale = (double)maxWidthEmu / widthEmu;
                heightEmu = (long)(heightEmu * scale);
                widthEmu = maxWidthEmu;
            }

            return (widthEmu, heightEmu);
        }

        private DocumentFormat.OpenXml.Packaging.PartTypeInfo GetImagePartType(string filePath) {
            string ext = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant();
            return ext switch {
                "jpg" or "jpeg" => DocumentFormat.OpenXml.Packaging.ImagePartType.Jpeg,
                "png" => DocumentFormat.OpenXml.Packaging.ImagePartType.Png,
                "bmp" => DocumentFormat.OpenXml.Packaging.ImagePartType.Bmp,
                "gif" => DocumentFormat.OpenXml.Packaging.ImagePartType.Gif,
                "tif" or "tiff" => DocumentFormat.OpenXml.Packaging.ImagePartType.Tiff,
                "ico" => DocumentFormat.OpenXml.Packaging.ImagePartType.Icon,
                _ => DocumentFormat.OpenXml.Packaging.ImagePartType.Png
            };
        }

        private Paragraph BuildImageParagraph(
            string relationshipId, long widthEmu, long heightEmu, string name) {
            uint drawingId = (uint)Interlocked.Increment(ref _drawingIdCounter);

            var element = new Drawing(
                new DW.Inline(
                    new DW.Extent() { Cx = widthEmu, Cy = heightEmu },
                    new DW.EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                    new DW.DocProperties() { Id = drawingId, Name = name },
                    new DW.NonVisualGraphicFrameDrawingProperties(
                        new A.GraphicFrameLocks() { NoChangeAspect = true }),
                    new A.Graphic(
                        new A.GraphicData(
                            new PIC.Picture(
                                new PIC.NonVisualPictureProperties(
                                    new PIC.NonVisualDrawingProperties() { Id = 0U, Name = name },
                                    new PIC.NonVisualPictureDrawingProperties()),
                                new PIC.BlipFill(
                                    new A.Blip() { Embed = relationshipId },
                                    new A.Stretch(new A.FillRectangle())),
                                new PIC.ShapeProperties(
                                    new A.Transform2D(
                                        new A.Offset() { X = 0L, Y = 0L },
                                        new A.Extents() { Cx = widthEmu, Cy = heightEmu }),
                                    new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle })
                            )
                        ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                ) {
                    DistanceFromTop = 0U,
                    DistanceFromBottom = 0U,
                    DistanceFromLeft = 0U,
                    DistanceFromRight = 0U
                }
            );

            var para = new Paragraph(
                new ParagraphProperties(new Justification() { Val = JustificationValues.Center }));
            para.Append(new Run(element));

            return para;
        }

        private Paragraph CreateCaptionParagraph(string text) {
            var para = new Paragraph(
                new ParagraphProperties(new Justification() { Val = JustificationValues.Center }));

            var run = new Run();
            run.RunProperties = new RunProperties(
                new RunFonts() {
                    Ascii = "Palatino Linotype",
                    HighAnsi = "Palatino Linotype",
                    ComplexScript = "Palatino Linotype"
                },
                new FontSize() { Val = "20" },
                new Italic()
            );
            run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
            para.Append(run);

            return para;
        }

        private string FormatArticleId(decimal id) =>
            id == (int)id ? ((int)id).ToString() : id.ToString("0.0");

        private string ToRoman(int number) {
            string[] romans = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
            int[] values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
            string result = "";

            for (int i = 0; i < values.Length; i++)
                while (number >= values[i]) {
                    number -= values[i];
                    result += romans[i];
                }

            return result;
        }

        private string ToAzerbaijaniOrdinal(int n) {
            string[] arr = {
                "", "BIRINCI","IKINCI","ÜÇÜNCÜ","DÖRDÜNCÜ","BEŞINCI",
                "ALTINCI","YEDDINCI","SƏKKIZINCI","DOQQUZUNCU","ONUNCU"
            };
            return n < arr.Length ? arr[n] : n.ToString();
        }

        private Paragraph CreateEmptyParagraph() =>
            new Paragraph(new Run(new Text("")));
    }
}