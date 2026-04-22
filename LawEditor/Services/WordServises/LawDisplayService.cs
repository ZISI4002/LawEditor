using DocumentFormat.OpenXml.Bibliography;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace LawEditor.Services.WordServises
{
    public class LawDisplayService
    {
        private static readonly string[] RomanNumerals = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII", "XIII", "XIV", "XV" ,};
        private static readonly string[] Ordinals =
        {
    "BIRINCI",
    "IKINCI",
    "ÜÇÜNCÜ",
    "DÖRDÜNCÜ",
    "BEŞINCI",
    "ALTINCI",
    "YEDDINCI",
    "SƏKKIZINCI",
    "DOQQUZUNCU",
    "ONUNCU"
};
        public FlowDocument BuildDocument(Laws laws)
        {
            var doc = new FlowDocument();
            doc.PagePadding = new Thickness(12);

            // Проверка наличия данных
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

            // CHAPTERS
            int chapterIndex = 0;
            int sectionIndex = 0;
            foreach (var chapter in laws.Chapters)
            {
               string chapterOrdinal = chapterIndex < Ordinals.Length ? Ordinals[chapterIndex] : (chapterIndex + 1).ToString();

                // BÖLMƏ label
                doc.Blocks.Add(new Paragraph(new Run($"{chapterOrdinal} BÖLMƏ"))
                {
                    FontSize = 15,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkBlue,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 16, 0, 2)
                });

                // Chapter title
                doc.Blocks.Add(new Paragraph(new Run(chapter.Title))
                {
                    FontSize = 17,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkBlue,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 8)
                });

                chapterIndex++;
               

                foreach (var section in chapter.Sections)
                {
                  
                    string sectionRoman = RomanNumerals[sectionIndex].ToString();

                    // FƏSİL label
                    doc.Blocks.Add(new Paragraph(new Run($"{sectionRoman} fəsil"))
                    {
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.MediumBlue,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 10, 0, 2)
                    });

                    // Section title
                    doc.Blocks.Add(new Paragraph(new Run(section.Title))
                    {
                        FontSize = 15,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.MediumBlue,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 6)
                    });

                    sectionIndex++;

                    foreach (var article in section.Articles)
                    {
                        // Maddə номер
                        doc.Blocks.Add(new Paragraph(new Run($"Maddə {article.Id}."))
                        {
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.Navy,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 10, 0, 0)
                        });

                        // Article title
                        if (!string.IsNullOrWhiteSpace(article.Title))
                        { // Если есть заголовок, отображаем его вместе с EndnoteId
                            string titleWithEndnote = string.IsNullOrWhiteSpace(article.EndnoteId) ? article.Title : $"{article.Title} [{article.EndnoteId}]";
                            doc.Blocks.Add(new Paragraph(new Run(titleWithEndnote))
                            {
                                FontSize = 14,
                                FontWeight = FontWeights.Bold,
                                Foreground = Brushes.Navy,
                                TextAlignment = TextAlignment.Center,
                                Margin = new Thickness(0, 0, 0, 6)
                            });
                        }

                        int clauseIndex = 0;
                        foreach (var clause in article.Clauses)
                        {
                            string roman = clauseIndex < RomanNumerals.Length ? RomanNumerals[clauseIndex] : (clauseIndex + 1).ToString();
                            string clauseTextWithEndnote = string.IsNullOrWhiteSpace(clause.EndnoteId) ? clause.Text : $"{clause.Text} [{clause.EndnoteId}]";
                            doc.Blocks.Add(new Paragraph(new Run($"{roman}. {clauseTextWithEndnote}"))
                            {
                                FontSize = 13,
                                Foreground = Brushes.Black,
                                Margin = new Thickness(24, 2, 0, 2)
                            });

                            clauseIndex++;

                            foreach (var sub in clause.SubClauses)
                            {
                                string subTextWithEndnote = string.IsNullOrWhiteSpace(sub.EndnoteId) ? sub.Text : $"{sub.Text} [{sub.EndnoteId}]";
                                doc.Blocks.Add(new Paragraph(new Run($"{sub.Number}) {subTextWithEndnote}"))
                                {
                                    FontSize = 13,
                                    Foreground = Brushes.Black,
                                    Margin = new Thickness(48, 1, 0, 1)
                                });
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

                // Дата подписания
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
    }
}