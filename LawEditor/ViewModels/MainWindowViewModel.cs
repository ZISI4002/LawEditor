using LawEditor.Commands.MainWindowCommands;
using LawEditor.Models.RootClasses;
using LawEditor.ViewModels;
using System.Windows;
using System.Windows.Input;

public class MainWindowViewModel : BaseViewModel
{
    public MainWindowViewModel(Window window) : base(window)
    {
        this.AddWordCommand = new AddWordCommand(this);
    }

    public ICommand AddWordCommand { get; set; }

    // --- Файлы ---
    private string _fileNameLeft;
    public string FileNameLeft
    {
        get => _fileNameLeft;
        set => Set(ref _fileNameLeft, value);
    }

    private string _fileNameRight;
    public string FileNameRight
    {
        get => _fileNameRight;
        set => Set(ref _fileNameRight, value);
    }

    private Laws _laws;
    public Laws Laws
    {
        get => _laws;
        set => Set(ref _laws, value);
    }
    // --- Видимость ---
    private bool _isDisplayLeftVisible;
    public bool IsDisplayLeftVisible
    {
        get => _isDisplayLeftVisible;
        set => Set(ref _isDisplayLeftVisible, value);
    }
/*
    private bool _isDisplayRightVisible;
    public bool IsDisplayRightVisible
    {
        get => _isDisplayRightVisible;
        set => Set(ref _isDisplayRightVisible, value);
    }

  */ 

   
}