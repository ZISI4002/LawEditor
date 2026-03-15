using LawEditor.Models.RootClasses;
using LawEditor.Models.ChangableSourse;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace LawEditor.Services
{
    public class LawDisplayService
    {
        private static readonly string[] RomanNumerals = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII", "XIII", "XIV", "XV" };
        private static readonly string[] Ordinals = { "Birinci", "İkinci", "Üçüncü", "Dördüncü", "Beşinci", "Altıncı", "Yeddinci", "Səkkizinci", "Doqquzuncu", "Onuncu" };

        public FlowDocument BuildDocument(Laws laws)
        {
            var doc = new FlowDocument();
            doc.PagePadding = new Thickness(12);

            // HEADER
            doc.Blocks.Add(new Paragraph(new Run(laws.Header))
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkBlue,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16)
            });

            // CHAPTERS
            int chapterIndex = 0;
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
                int sectionIndex = 0;

                foreach (var section in chapter.Sections)
                {
                    string sectionRoman = RomanNumerals[section.Id-1].ToString();

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
                        {
                            doc.Blocks.Add(new Paragraph(new Run(article.Title))
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

                            doc.Blocks.Add(new Paragraph(new Run($"{roman}. {clause.Text}"))
                            {
                                FontSize = 13,
                                Foreground = Brushes.Black,
                                Margin = new Thickness(24, 2, 0, 2)
                            });

                            clauseIndex++;

                            foreach (var sub in clause.SubClauses)
                            {
                                doc.Blocks.Add(new Paragraph(new Run($"{sub.Number}) {sub.Text}"))
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
            if (laws.transitionalProvisions.Count > 0)
            {
                doc.Blocks.Add(new Paragraph(new Run("Keçid müddəaları"))
                {
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkBlue,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 8)
                });

                foreach (var tp in laws.transitionalProvisions)
                {
                    doc.Blocks.Add(new Paragraph(new Run(tp.Title))
                    {
                        FontSize = 13,
                        Foreground = Brushes.Black,
                        Margin = new Thickness(16, 2, 0, 2)
                    });
                }

                // Дата подписания
                doc.Blocks.Add(new Paragraph(new Run("12 noyabr 1995-ci il"))
                {
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    TextAlignment = TextAlignment.Right,
                    Margin = new Thickness(0, 10, 16, 2)
                });
            }

            // CONSTITUTIONAL AMENDMENTS
            if (laws.constitutionalAmendments.Count > 0)
            {
                doc.Blocks.Add(new Paragraph(new Run("KONSTİTUSİYAYA EDİLMİŞ DƏYİŞİKLİK VƏ ƏLAVƏLƏRİN SİYAHISI"))
                {
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkBlue,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 8)
                });

                foreach (var amendment in laws.constitutionalAmendments)
                {
                    doc.Blocks.Add(new Paragraph(new Run($"[{amendment.Id}] {amendment.Title}"))
                    {
                        FontSize = 13,
                        Foreground = Brushes.Black,
                        Margin = new Thickness(16, 2, 0, 2)
                    });
                }
            }

            // SOURCE DOCUMENTS
            if (laws.sourceDocumentsLists.Count > 0)
            {
                doc.Blocks.Add(new Paragraph(new Run("İSTİFADƏ OLUNMUŞ MƏNBƏ SƏNƏDLƏRİNİN SİYAHISI"))
                {
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkBlue,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 8)
                });

                foreach (var source in laws.sourceDocumentsLists)
                {
                    doc.Blocks.Add(new Paragraph(new Run(source.Title))
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