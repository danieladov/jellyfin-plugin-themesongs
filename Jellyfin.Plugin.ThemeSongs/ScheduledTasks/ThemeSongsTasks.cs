using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.ThemeSongs.Api;
using MediaBrowser.Api;
using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ThemeSongs.ScheduledTasks
{
    public class DownloadThemeSongsTask : IScheduledTask
    {
        private readonly ILogger _logger;
        private readonly ThemeSongsManager _mergeVersionsManager;

        public DownloadThemeSongsTask(ILibraryManager libraryManager, ICollectionManager collectionManager, ILogger<VideosService> logger, IServerConfigurationManager serverConfigurationManager,
            IHttpResultFactory httpResultFactory,
            IUserManager userManager,
            IDtoService dtoService,
            IAuthorizationContext authContext)
        {
            _logger = logger;
            _mergeVersionsManager = new ThemeSongsManager(libraryManager, collectionManager, logger, serverConfigurationManager,
             httpResultFactory,
             userManager,
             dtoService,
             authContext, new GetId());
        }
        public Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            _logger.LogInformation("Starting plugin, Downloading Theme Songs...");
            _mergeVersionsManager.DownloadAllThemeSongs();
            _logger.LogInformation("All theme songs downloaded");
            return Task.CompletedTask;
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            // Run this task every 24 hours
            return new[] {
                new TaskTriggerInfo { Type = TaskTriggerInfo.TriggerInterval, IntervalTicks = TimeSpan.FromHours(24).Ticks}
            };
        }

        public string Name => "Download Theme Songs";
        public string Key => "DownloadThemeSongs";
        public string Description => "Scans all libraries to download Theme Songs";
        public string Category => "Theme Songs";
    }


    
}
