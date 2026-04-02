using LawEditor.Models.ChangableData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Models.TreeClasses
{
    public class LawAnchor
    {
        public Chapter? Chapter { get; set; }
        public Section? Section { get; set; }
        public Article? Article { get; set; }
        public Clause? Clause { get; set; }
        public SubClause? SubClause { get; set; }

        // Получить самый глубокий выбранный элемент
        public object? GetDeepestItem()
        {
            if (SubClause != null) return SubClause;
            if (Clause != null) return Clause;
            if (Article != null) return Article;
            if (Section != null) return Section;
            if (Chapter != null) return Chapter;
            return null;
        }

        // Определить уровень вложенности
        public int GetLevel()
        {
            if (SubClause != null) return 5;
            if (Clause != null) return 4;
            if (Article != null) return 3;
            if (Section != null) return 2;
            if (Chapter != null) return 1;
            return 0;
        }

        // Проверка, что якорь заполнен корректно
        public bool IsValid()
        {
            if (Chapter == null) return false;
            // Если есть Section, должен быть Chapter
            if (Section != null && Chapter == null) return false;
            // Если есть Article, должны быть Section и Chapter
            if (Article != null && (Section == null || Chapter == null)) return false;
            // Если есть Clause, должны быть Article, Section, Chapter
            if (Clause != null && (Article == null || Section == null || Chapter == null)) return false;
            // Если есть SubClause, должны быть все родители
            if (SubClause != null && (Clause == null || Article == null || Section == null || Chapter == null)) return false;

            return true;
        }
    }
}