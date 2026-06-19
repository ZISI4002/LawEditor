using LawEditor.Models.ChangableSourse;
using LawEditor.Models.ChangableData;
using LawEditor.Models.RootClasses;
using LawEditor.Models.SpecialElements;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Table = LawEditor.Models.SpecialElements.Table;

namespace LawEditor.Services.WordServises
{
    public class LawDisplayService
    {
        private static readonly string[] RomanNumerals = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII", "XIII", "XIV", "XV" };
        private static readonly string[] Ordinals =
        {
            "BIRINCI", "IKINCI", "ÜÇÜNCÜ", "DÖRDÜNCÜ", "BEŞINCI",
            "ALTINCI", "YEDDINCI", "SƏKKIZINCI", "DOQQUZUNCU", "ONUNCU"
        };

        public FlowDocument BuildDocument(Laws laws)
        {
            var doc = new FlowDocument();
            doc.PagePadding = new Thickness(12);

            if (laws?.UpperObjects?.Count == 0 || laws?.UpperObjects[0]?.Headers?.Count == 0)
            {
                doc.Blocks.Add(new Paragraph(new Run("Нет доступных данных")));
                return doc;
            }

            // HEADER
            doc.Blocks.Add(new Paragraph(new Run(laws.UpperObjects[0].Headers[0].FullText))
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkBlue,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16)
            });

            int chapterIndex = 0;
            int sectionIndex = 0;

            foreach (var chapter in laws.Chapters)
            {
                string chapterOrdinal = chapterIndex < Ordinals.Length ? Ordinals[chapterIndex] : (chapterIndex + 1).ToString();

                doc.Blocks.Add(new Paragraph(new Run($"{chapterOrdinal} BÖLMƏ"))
                {
                    FontSize = 15,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkBlue,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 16, 0, 2)
                });

                doc.Blocks.Add(new Paragraph(new Run(chapter.Title))
                {
                    FontSize = 17,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkBlue,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 8)
                });

                chapterIndex++;

                if (chapter.Table != null)
                    doc.Blocks.Add(BuildTable(chapter.Table));

                if (chapter.Image != null)
                    AddImageBlock(doc, chapter.Image);

                foreach (var section in chapter.Sections)
                {
                    string sectionRoman = RomanNumerals[sectionIndex];

                    doc.Blocks.Add(new Paragraph(new Run($"{sectionRoman} fəsil"))
                    {
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.MediumBlue,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 10, 0, 2)
                    });

                    doc.Blocks.Add(new Paragraph(new Run(section.Title))
                    {
                        FontSize = 15,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.MediumBlue,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 6)
                    });

                    sectionIndex++;

                    if (section.Table != null)
                        doc.Blocks.Add(BuildTable(section.Table));

                    if (section.Image != null)
                        AddImageBlock(doc, section.Image);

                    foreach (var article in section.Articles)
                    {
                        doc.Blocks.Add(new Paragraph(new Run($"Maddə {article.Id}."))
                        {
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.Navy,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 10, 0, 0)
                        });

                        if (!string.IsNullOrWhiteSpace(article.Title))
                        {
                            string titleWithEndnote = string.IsNullOrWhiteSpace(article.EndnoteId)
                                ? article.Title
                                : $"{article.Title} [{article.EndnoteId}]";

                            doc.Blocks.Add(new Paragraph(new Run(titleWithEndnote))
                            {
                                FontSize = 14,
                                FontWeight = FontWeights.Bold,
                                Foreground = Brushes.Navy,
                                TextAlignment = TextAlignment.Center,
                                Margin = new Thickness(0, 0, 0, 6)
                            });
                        }

                        if (article.Table != null)
                            doc.Blocks.Add(BuildTable(article.Table));

                        if (article.Image != null)
                            AddImageBlock(doc, article.Image);

                        int clauseIndex = 0;
                        foreach (var clause in article.Clauses)
                        {
                            string roman = clauseIndex < RomanNumerals.Length
                                ? RomanNumerals[clauseIndex]
                                : (clauseIndex + 1).ToString();

                            string clauseTextWithEndnote = string.IsNullOrWhiteSpace(clause.EndnoteId)
                                ? clause.Text
                                : $"{clause.Text} [{clause.EndnoteId}]";

                            var clauseParagraph = new Paragraph
                            {
                                FontSize = 13,
                                Foreground = Brushes.Black,
                                Margin = new Thickness(24, 2, 0, 2)
                            };

                            string fullClauseText = $"{roman}. {clauseTextWithEndnote}";

                            if (!string.IsNullOrEmpty(clause.LinkText))
                            {
                                int linkIndex = fullClauseText.IndexOf(clause.LinkText, StringComparison.Ordinal);

                                if (linkIndex >= 0)
                                {
                                    if (linkIndex > 0)
                                        clauseParagraph.Inlines.Add(new Run(fullClauseText.Substring(0, linkIndex)));

                                    clauseParagraph.Inlines.Add(new Hyperlink(new Run(clause.LinkText))
                                    {
                                        Foreground = new SolidColorBrush(Color.FromRgb(0x34, 0x98, 0xDB)),
                                        FontWeight = FontWeights.SemiBold,
                                        TextDecorations = TextDecorations.Underline,
                                        NavigateUri = Uri.TryCreate(clause.Url, UriKind.Absolute, out var uri) ? uri : null
                                    });

                                    int afterStart = linkIndex + clause.LinkText.Length;
                                    if (afterStart < fullClauseText.Length)
                                        clauseParagraph.Inlines.Add(new Run(fullClauseText.Substring(afterStart)));
                                }
                                else
                                {
                                    clauseParagraph.Inlines.Add(new Run(fullClauseText));
                                }
                            }
                            else
                            {
                                clauseParagraph.Inlines.Add(new Run(fullClauseText));
                            }

                            doc.Blocks.Add(clauseParagraph);

                            clauseIndex++;

                            if (clause.Table != null)
                                doc.Blocks.Add(BuildTable(clause.Table));

                            if (clause.Image != null)
                                AddImageBlock(doc, clause.Image);

                            foreach (var sub in clause.SubClauses)
                            {
                                string subTextWithEndnote = string.IsNullOrWhiteSpace(sub.EndnoteId)
                                    ? sub.Text
                                    : $"{sub.Text} [{sub.EndnoteId}]";

                                doc.Blocks.Add(new Paragraph(new Run($"{sub.Number}) {subTextWithEndnote}"))
                                {
                                    FontSize = 13,
                                    Foreground = Brushes.Black,
                                    Margin = new Thickness(48, 1, 0, 1)
                                });

                                if (sub.Table != null)
                                    doc.Blocks.Add(BuildTable(sub.Table));

                                if (sub.Image != null)
                                    AddImageBlock(doc, sub.Image);
                            }
                        }
                    }
                }
            }

            // TRANSITIONAL PROVISIONS
            var transitionalProvisions = laws.SourcesData[0] as dynamic;
            if (transitionalProvisions.Source.Count > 1)
            {
                doc.Blocks.Add(new Paragraph(new Run("Keçid müddəaları"))
                {
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkBlue,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 8)
                });

                foreach (var tp in transitionalProvisions.Source)
                {
                    if (tp is TransitionalProvisionsDateNote)
                        continue;

                    doc.Blocks.Add(new Paragraph(new Run($" {tp.Id}. {tp.Title}"))
                    {
                        FontSize = 13,
                        Foreground = Brushes.Black,
                        Margin = new Thickness(16, 2, 0, 2)
                    });
                }

                doc.Blocks.Add(new Paragraph(new Run($"{TransitionalProvisions.Date}"))
                {
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    TextAlignment = TextAlignment.Right,
                    Margin = new Thickness(0, 10, 16, 2)
                });
            }

            // SOURCE DOCUMENTS
            var sourceDocumentsLists = laws.SourcesData[1] as dynamic;
            if (sourceDocumentsLists.Source.Count > 0)
            {
                doc.Blocks.Add(new Paragraph(new Run("İSTİFADƏ OLUNMUŞ MƏNBƏ SƏNƏDLƏRİNİN SİYAHISI"))
                {
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkBlue,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 8)
                });

                foreach (var source in sourceDocumentsLists.Source)
                {
                    doc.Blocks.Add(new Paragraph(new Run($" {source.Id}. {source.Title}"))
                    {
                        FontSize = 13,
                        Foreground = Brushes.Black,
                        Margin = new Thickness(16, 2, 0, 2)
                    });
                }
            }

            // CONSTITUTIONAL AMENDMENTS
            var constitutionalAmendments = laws.SourcesData[2] as dynamic;
            if (constitutionalAmendments.Source.Count > 0)
            {
                doc.Blocks.Add(new Paragraph(new Run("KONSTİTUSİYAYA EDİLMİŞ DƏYİŞİKLİK VƏ ƏLAVƏLƏRİN SİYAHISI"))
                {
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkBlue,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 8)
                });

                foreach (var amendment in constitutionalAmendments.Source)
                {
                    doc.Blocks.Add(new Paragraph(new Run($"{amendment.Id}. {amendment.Title}"))
                    {
                        FontSize = 13,
                        Foreground = Brushes.Black,
                        Margin = new Thickness(16, 2, 0, 2)
                    });
                }
            }

            return doc;
        }

        private void AddImageBlock(FlowDocument doc, Models.SpecialElements.Image image)
        {
            if (string.IsNullOrEmpty(image.FilePath) || !File.Exists(image.FilePath))
            {
                doc.Blocks.Add(new Paragraph(new Run($"[Şəkil tapılmadı: {image.FileName}]"))
                {
                    Foreground = Brushes.Gray,
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(24, 4, 0, 4)
                });
                return;
            }

            try
            {
                byte[] fileBytes = File.ReadAllBytes(image.FilePath);

                var bitmap = new BitmapImage();
                using (var stream = new MemoryStream(fileBytes))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }
                bitmap.Freeze();

                var imageControl = new System.Windows.Controls.Image
                {
                    Source = bitmap,
                    MaxWidth = 600,
                    Stretch = Stretch.Uniform,
                    ToolTip = image.Title ?? image.FileName
                };

                doc.Blocks.Add(new BlockUIContainer(imageControl)
                {
                    Margin = new Thickness(24, 8, 0, 8)
                });
            }
            catch (Exception ex)
            {
                doc.Blocks.Add(new Paragraph(new Run($"[Şəkil yüklənmədi: {ex.Message}]"))
                {
                    Foreground = Brushes.Red,
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(24, 4, 0, 4)
                });
            }
        }

        private System.Windows.Documents.Table BuildTable(Table model)
        {
            var table = new System.Windows.Documents.Table
            {
                Margin = new Thickness(24, 8, 0, 8),
                BorderBrush = Brushes.Navy,
                BorderThickness = new Thickness(1),
                CellSpacing = 0
            };

            int colCount = model.Headers.Count;
            for (int i = 0; i < colCount; i++)
                table.Columns.Add(new TableColumn());

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                var titleRow = new TableRow();
                var titleCell = new TableCell(new Paragraph(new Run(model.Title))
                {
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Navy,
                    TextAlignment = TextAlignment.Center
                })
                {
                    ColumnSpan = colCount == 0 ? 1 : colCount,
                    Background = new SolidColorBrush(Color.FromRgb(13, 23, 61)),
                    BorderBrush = Brushes.Navy,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(6, 4, 6, 4)
                };
                titleRow.Cells.Add(titleCell);
                rowGroup.Rows.Add(titleRow);
            }

            if (model.Headers.Count > 0)
            {
                var headerRow = new TableRow();
                foreach (var header in model.Headers)
                {
                    headerRow.Cells.Add(new TableCell(new Paragraph(new Run(header))
                    {
                        FontSize = 12,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Color.FromRgb(56, 189, 248)),
                        TextAlignment = TextAlignment.Center
                    })
                    {
                        Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
                        BorderBrush = Brushes.Navy,
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(8, 4, 8, 4)
                    });
                }
                rowGroup.Rows.Add(headerRow);
            }

            bool alternate = false;
            foreach (var rowData in model.Rows)
            {
                var tableRow = new TableRow();
                var rowBg = alternate
                    ? new SolidColorBrush(Color.FromRgb(17, 24, 39))
                    : new SolidColorBrush(Color.FromRgb(26, 35, 61));

                for (int i = 0; i < colCount; i++)
                {
                    string cellText = i < rowData.Cells.Count ? rowData.Cells[i] : string.Empty;
                    tableRow.Cells.Add(new TableCell(new Paragraph(new Run(cellText))
                    {
                        FontSize = 12,
                        Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                        TextAlignment = TextAlignment.Left
                    })
                    {
                        Background = rowBg,
                        BorderBrush = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(8, 4, 8, 4)
                    });
                }

                rowGroup.Rows.Add(tableRow);
                alternate = !alternate;
            }

            return table;
        }
    }
}