using System.Collections.ObjectModel;
using AquaSync.App.Contracts.Services;
using AquaSync.App.Models;
using AquaSync.Chihiros.Discovery;
using CommunityToolkit.Mvvm.Input;

namespace AquaSync.App.ViewModels;

public sealed class LampsViewModel : ViewModelBase, INavigationAware
{
    private readonly ILampService _lampService;
    private readonly IAquariumContext _aquariumContext;
    private readonly INavigationService _navigationService;
    private bool _isBusy;
    private Guid _currentAquariumId;

    public LampsViewModel(ILampService lampService, IAquariumContext aquariumContext, INavigationService navigationService)
    {
        _lampService = lampService;
        _aquariumContext = aquariumContext;
        _navigationService = navigationService;

        Lamps = [];
        Lamps.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsEmpty));

        SelectLampCommand = new RelayCommand<LampConfiguration>(lamp =>
            _navigationService.NavigateTo(typeof(LampDetailViewModel).FullName!, lamp!.Id));
    }

    public ObservableCollection<LampConfiguration> Lamps { get; }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public bool IsEmpty => Lamps.Count == 0;

    public Guid CurrentAquariumId => _currentAquariumId;

    public RelayCommand<LampConfiguration> SelectLampCommand { get; }

    public void OnNavigatedTo(object parameter)
    {
        if (_aquariumContext.CurrentAquarium is not { } aquarium) return;
        _currentAquariumId = aquarium.Id;
        _ = LoadLampsAsync();
    }

    public void OnNavigatedFrom() { }

    public async Task LoadLampsAsync()
    {
        IsBusy = true;
        try
        {
            var lamps = await _lampService.GetLampsForAquariumAsync(_currentAquariumId);
            Lamps.Clear();
            foreach (var lamp in lamps)
                Lamps.Add(lamp);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task AddLampAsync(DiscoveredDevice device)
    {
        await _lampService.AddLampAsync(_currentAquariumId, device);
        await LoadLampsAsync();
    }
}
