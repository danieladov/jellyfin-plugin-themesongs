using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ThemeSongs
{
    public class ThemeSongsManager : IServerEntryPoint
    {
        private readonly ILibraryManager _libraryManager;
        private readonly Timer _timer;
        private readonly ILogger _logger;

        public ThemeSongsManager(ILibraryManager libraryManager, ILogger logger)
        {
            _libraryManager = libraryManager;
            _logger = logger;
            _timer = new Timer(_ => OnTimerElapsed(), null, Timeout.Infinite, Timeout.Infinite);
        }

        private IEnumerable<Series> GetSeriesFromLibrary()
        {
            return _libraryManager.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes = new[] {nameof(Series)},
                IsVirtualItem = false,
                Recursive = true,
                HasTvdbId = true
            }).Select(m => m as Series);
        }

        public void DownloadAllThemeSongs()
        {
            var series = GetSeriesFromLibrary();
            foreach (var serie in series)
            {
                var tvdb = serie.GetProviderId(MetadataProvider.Tvdb);
                var themeSongPath = Path.Join(serie.Path, "theme.mp3");
                var link = $"http://tvthemes.plexapp.com/{tvdb}.mp3";
                _logger.LogDebug("Trying to download {seriesName}, {link}", serie.Name, link);

                try
                {
                    using var client = new WebClient();
                    client.DownloadFile(link, themeSongPath);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "{seriesName} not found, or no internet connection", serie.Name);
                }
            }
        }

        private void OnTimerElapsed()
        {
            // Stop the timer until next update
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public Task RunAsync()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}
