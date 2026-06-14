using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Win32;

namespace LawEditor.Models.SpecialElements
{
    public class Images
    {
        private static readonly string SaveFolder = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "SpecialImages");

        public ObservableCollection<Image> ImageList { get; set; } = new();

        public Image? AddFromDialog()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выбрать изображение",
                Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
                return null;

            var image = new Image();
            image.LoadFromFile(dialog.FileName);
            image.Title = Path.GetFileNameWithoutExtension(dialog.FileName);
            ImageList.Add(image);
            return image;
        }

        public Image? GetByTitle(string title)
        {
            return ImageList.FirstOrDefault(i => i.Title == title);
        }

        /// <summary>Сохраняет все изображения в папку SpecialImages рядом с exe</summary>
        public void SaveToFolder()
        {
            Directory.CreateDirectory(SaveFolder);

            foreach (var image in ImageList)
            {
                if (image.Data.Length == 0) continue;

                string ext = string.IsNullOrWhiteSpace(image.FilePath)
                    ? ".png"
                    : Path.GetExtension(image.FilePath);

                string fileName = $"{image.Id}_{image.Title}{ext}";
                string fullPath = Path.Combine(SaveFolder, fileName);

                File.WriteAllBytes(fullPath, image.Data);
                image.FilePath = fullPath; // обновляем путь на сохранённый
            }
        }

        /// <summary>Удаляет физический файл изображения с диска и из коллекции</summary>
        public void DeleteFile(int id)
        {
            var image = ImageList.FirstOrDefault(i => i.Id == id);
            if (image == null) return;

            if (!string.IsNullOrWhiteSpace(image.FilePath) && File.Exists(image.FilePath))
                File.Delete(image.FilePath);

            ImageList.Remove(image);
            Image.DecreaseCounter();
        }

        /// <summary>Удаляет всю папку SpecialImages со всеми файлами</summary>
        public void DeleteFolder()
        {
            if (Directory.Exists(SaveFolder))
                Directory.Delete(SaveFolder, recursive: true);

            ImageList.Clear();
        }

        public void Remove(int id)
        {
            var item = ImageList.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                ImageList.Remove(item);
                Image.DecreaseCounter();
            }
        }
    }
}