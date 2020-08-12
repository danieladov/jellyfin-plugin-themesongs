using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.ThemeSongs.Api;
using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ThemeSongs
{
    public class ThemeSongsManager : IServerEntryPoint
    {
        private readonly ILibraryManager _libraryManager;
        private readonly Timer _timer;
        private readonly ILogger _logger; // TODO logging
        private readonly IUserManager _userManager;
        private readonly SessionInfo _session;
        private readonly ISessionManager _sessionManager;
		private ICollectionManager collectionManager;
		private IServerConfigurationManager serverConfigurationManager;
		private IHttpResultFactory httpResultFactory;
		private IDtoService dtoService;
		private IAuthorizationContext authContext;
		private GetId getId;

		public ThemeSongsManager(ILibraryManager libraryManager, ICollectionManager collectionManager, ILogger logger, IServerConfigurationManager serverConfigurationManager,
            IHttpResultFactory httpResultFactory,
            IUserManager userManager,
            IDtoService dtoService,
            IAuthorizationContext authContext, GetId request)
        {
            var id = request.Id;
            _session = new SessionInfo(_sessionManager, logger);
            _libraryManager = libraryManager;
            _userManager = userManager;
            _logger = logger;
            _timer = new Timer(_ => OnTimerElapsed(), null, Timeout.Infinite, Timeout.Infinite);
            this.getId = getId;
        }

		

		private IEnumerable<Series> GetSeriesFromLibrary()
        {
            var episodes = _libraryManager.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { nameof(Series) },
                IsVirtualItem = false,
                Recursive = true,
                HasTvdbId = true,

            }).Select(m => m as Series);

            return episodes.ToList();

        }

        public void DownloadAllThemeSongs()
        {
            var series = GetSeriesFromLibrary();
            foreach (var serie in series)
            {
                var tvdb = serie.GetProviderId(MetadataProvider.Tvdb);
                var themeSongPath = serie.Path + "/theme.mp3";
                var link = "http://tvthemes.plexapp.com/" + tvdb + ".mp3";
                _logger.LogDebug($"Triying to download {serie.Name} , {link}");

                try
                {
                    using (var client = new WebClient())
                        client.DownloadFile(link, themeSongPath);
                }catch(Exception e)
				{
                    _logger.LogError($"{serie.Name} not found, or not internet conection");
                    _logger.LogError(e.Message);
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
