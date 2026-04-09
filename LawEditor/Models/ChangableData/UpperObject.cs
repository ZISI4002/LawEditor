using LawEditor.Models.ChangableData;
using System.Collections.ObjectModel;

namespace LawEditor.Models.ChangableData
{
    public class UpperObject
    {
        public int Id { get; set; }
        public string ObjectName { get; set; }
        public ObservableCollection<Header> Headers { get; set; }= new();
         public UpperObject() { }
    }
}