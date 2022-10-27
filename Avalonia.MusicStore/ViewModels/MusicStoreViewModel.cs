using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using iTunesSearch.Library.Models;
using ReactiveUI;
using Album = Avalonia.MusicStore.Models.Album;

namespace Avalonia.MusicStore.ViewModels;

using ReactiveUI;

public class MusicStoreViewModel : ViewModelBase
{
    
    public MusicStoreViewModel()
    {
        // for (int indice = 100; indice > 0; indice--)
        // {
        //     SearchResults.Add(new AlbumViewModel());
        // }

        BuyMusicCommand = ReactiveCommand.Create(() => { return SelectedAlbum; });

        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(400))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(DoSearch!);

    }
    
    
    private bool _isBusy;
    private string? _searchText;

    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }

    private AlbumViewModel? _selectedAlbum;
    public ObservableCollection<AlbumViewModel> SearchResults { get; } = new();

    public AlbumViewModel? SelectedAlbum
    {
        get => _selectedAlbum;
        set => this.RaiseAndSetIfChanged(ref _selectedAlbum, value);
    }
    
    public ReactiveCommand<Unit, AlbumViewModel?> BuyMusicCommand { get; }
    
    private CancellationTokenSource? _cancellationTokenSource;

    private async void DoSearch(string s)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        IsBusy = true;
        SearchResults.Clear();

        if (!string.IsNullOrWhiteSpace(s))
        {
            var albums = await Album.SearchAsync(s);

            foreach (var album in albums)
            {
                var vm = new AlbumViewModel(album);

                SearchResults.Add(vm);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                LoadCovers(cancellationToken);
            }
        }

        IsBusy = false;
    }

    private async void LoadCovers(CancellationToken cancellationToken)
    {
        foreach (var album in SearchResults.ToList())
        {
            await album.LoadCover();

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

}