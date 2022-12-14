using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using iTunesSearch.Library.Models;
using ReactiveUI;
using Album = Avalonia.MusicStore.Models.Album;

namespace Avalonia.MusicStore.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Text;
    using System.Windows.Input;
    using ReactiveUI;
    
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _collectionEmpty;

        public bool CollectionEmpty
        {
            get => _collectionEmpty;
            set => this.RaiseAndSetIfChanged(ref _collectionEmpty, value);
        }

        public ObservableCollection<AlbumViewModel> Albums { get; } = new();


        public MainWindowViewModel()
        {
            ShowDialog = new Interaction<MusicStoreViewModel, AlbumViewModel?>();
            
            BuyMusicCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var store = new MusicStoreViewModel();
                var result = await ShowDialog.Handle(store);
                if (result != null)
                {
                    Albums.Add(result);

                    await result.SaveToDiskAsync();
                }
                
            });

            this.WhenAnyValue(x => x.Albums.Count)
                .Subscribe(x => CollectionEmpty = x == 0);

            RxApp.MainThreadScheduler.Schedule(LoadAlbums);
        }

        public ICommand BuyMusicCommand { get; }
        
        public Interaction<MusicStoreViewModel, AlbumViewModel?> ShowDialog { get; }

        public async void LoadAlbums()
        {
            var albums = (await Album.LoadCacheAsync()).Select(x => new AlbumViewModel(x));

            foreach (var album in albums)
            {
                Albums.Add(album);
            }

            foreach (var album in Albums.ToList())
            {
                await album.LoadCover();
            }
        }

    }
    
}