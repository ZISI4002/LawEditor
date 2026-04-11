public async void Execute(object? parameter)
{
    if (_viewModel.FileIsAdded == true || 
        (_viewModel.Laws.Chapters.Count > 0 && 
         _viewModel.Laws.UpperObjects.Count > 0 && 
         _viewModel.Laws.SourceData.Count > 0))
    {
        _viewModel.IsDisplayRightVisible = true;
        _viewModel.IsLoading = true;

        try
        {
            var result = await Task.Run(() =>
            {
                XMLTranslatorServise xmlTranslator = new XMLTranslatorServise();
                XmlFileProcessingService xmlFileProcessingService = new XmlFileProcessingService();

                string folderPath = System.IO.Path.GetDirectoryName(_viewModel.FilePath);
                string fileName = _viewModel.FileNameRight;
                Task.Delay(2000).Wait(); 
                xmlTranslator.Translate(_viewModel.Laws, folderPath, fileName);
                var xmlLaw = xmlFileProcessingService.ReadXmlFile(folderPath, fileName);

                return xmlLaw;
            });

            _viewModel.XML = result;
        }
        finally
        {
            _viewModel.IsLoading = false;
        }
    }
    else
    {
        _viewModel.IsDisplayRightVisible = false;

        WarningException warning = new WarningException("Fayl əlavə edilməyib");
        MessageBox.Show(warning.Message, "Xəbərdarlıq",
            MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}