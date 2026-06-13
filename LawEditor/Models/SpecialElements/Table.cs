using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;

namespace LawEditor.Models.SpecialElements
{
    public class Table
    {
        private static int counter = 1;

        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public ObservableCollection<string> Headers { get; set; } = new();
        public ObservableCollection<TableRowData> Rows { get; set; } = new();

        public Table()
        {
            Id = counter++;
            Title = $"Table {Id}";
        }
        // Приватный конструктор — без счётчика, для ручного создания
        private Table(bool manual) { }

        public static Table CreateManual() => new Table(true);
        public static int GetCounter() => counter;
        public static void ResetCounter()
        {
            counter = 1;
        }
        public static void IncreaseCounter()
        {
            counter++;
        }
        public static void DecreaseCounter()
        {
            if (counter > 1) counter--;
        }


        /// <summary>Глубокая копия для редактирования без мутации оригинала до Save</summary>
        public Table Clone()
        {
            var clone = new Table() { Id = this.Id, Title = this.Title };

            foreach (var h in Headers)
                clone.Headers.Add(h);

            foreach (var row in Rows)
            {
                var newRow = new TableRowData();
                foreach (var cell in row.Cells)
                    newRow.Cells.Add(cell);
                clone.Rows.Add(newRow);
            }

            return clone;
        }

        /// <summary>Применить данные из другой таблицы (после Save)</summary>
        public void ApplyFrom(Table source)
        {
            Title = source.Title;

            Headers.Clear();
            foreach (var h in source.Headers)
                Headers.Add(h);

            Rows.Clear();
            foreach (var row in source.Rows)
            {
                var newRow = new TableRowData();
                foreach (var cell in row.Cells)
                    newRow.Cells.Add(cell);
                Rows.Add(newRow);
            }
        }
    }
}