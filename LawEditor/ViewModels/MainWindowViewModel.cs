using LawEditor.Commands.MainWindowCommands;
using LawEditor.Models.ChangableData;
using LawEditor.Models.RootClasses;
using LawEditor.ViewModels;
using System.Windows;
using System.Windows.Input;

public class MainWindowViewModel : BaseViewModel
{
    public MainWindowViewModel(Window window) : base(window)
    {
        this.Laws = new Laws();
        this.AddWordCommand = new AddWordCommand(this);
        this.GenerateCommand = new GenerateCommand(this);
        this.RemoveCommand = new RemoveCommand(this);
        this.UpdateCommand=new UpdateCommand(this);
        this.CreateLawsCommand = new CreateLawsCommand(this);
        
    }

    public ICommand AddWordCommand { get; set; }
    public ICommand GenerateCommand { get; set; }
    public ICommand RemoveCommand { get; set; }
    public ICommand UpdateCommand { get; set; }
    public ICommand CreateLawsCommand { get; set; }
    // --- Файлы ---
    public string FilePath { get; set; }
    private bool _fileisadded = false;
    public bool FileIsAdded
    {
        get => _fileisadded;
        set => Set(ref _fileisadded, value);
    }
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
    public Chapter ChapterReset { get; set; }
    public Section SectionReset { get; set; }
    private Laws _xML;
    public Laws XML
    {
        get => _xML;
        set => Set(ref _xML, value);
    }
    // --- Видимость ---
    private bool _isDisplayLeftVisible;
    public bool IsDisplayLeftVisible
    {
        get => _isDisplayLeftVisible;
        set => Set(ref _isDisplayLeftVisible, value);
    }
    private bool _isDisplayButtonsVisible=true;
    public bool IsDisplayButtonsVisible
    {
        get=> _isDisplayButtonsVisible;
        set=> Set(ref _isDisplayButtonsVisible, value);
    }       
    private bool _isDisplayRightVisible;
    public bool IsDisplayRightVisible
    {
        get => _isDisplayRightVisible;
        set => Set(ref _isDisplayRightVisible, value);
    }


    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
       set => Set(ref _isLoading, value);
    }

}