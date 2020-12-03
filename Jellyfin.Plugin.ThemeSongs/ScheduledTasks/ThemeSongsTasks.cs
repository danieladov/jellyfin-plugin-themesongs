using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ThemeSongs.ScheduledTasks
{
    public class DownloadThemeSongsTask : IScheduledTask
    {
        private readonly ILogger<DownloadThemeSongsTask> _logger;
        private readonly ThemeSongsManager _themeSongsManager;

        public DownloadThemeSongsTask(ILibraryManager libraryManager, ILogger<DownloadThemeSongsTask> logger)
        {
            _logger = logger;
            _themeSongsManager = new ThemeSongsManager(libraryManager,  logger);
        }
        public Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            _logger.LogInformation("Starting plugin, Downloading TV Theme Songs...");
            _themeSongsManager.DownloadAllThemeSongs();
            _logger.LogInformation("All theme songs downloaded");
            return Task.CompletedTask;
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            // Run this task every 24 hours
            yield return new TaskTriggerInfo
            {
                Type = TaskTriggerInfo.TriggerInterval, 
                IntervalTicks = TimeSpan.FromHours(24).Ticks
            };
        }

        public string Name => "Download TV Theme Songs";
        public string Key => "DownloadTV ThemeSongs";
        public string Description => "Scans all libraries to download TV Theme Songs";
        public string Category => "Theme Songs";
    }

    public class DownloadMoviesThemeSongsTask : IScheduledTask
    {
        private readonly ILogger<DownloadThemeSongsTask> _logger;
        private readonly ThemeSongsManager _themeSongsManager;

        public DownloadMoviesThemeSongsTask(ILibraryManager libraryManager, ILogger<DownloadThemeSongsTask> logger)
        {
            _logger = logger;
            _themeSongsManager = new ThemeSongsManager(libraryManager, logger);
        }
        public Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            _logger.LogInformation("Starting plugin, Downloading Theme Songs...");
            _themeSongsManager.DownloadAllMoviesThemeSongsAsync();
            _logger.LogInformation("All theme songs downloaded");
            return Task.CompletedTask;
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            // Run this task every 24 hours
            yield return new TaskTriggerInfo
            {
                Type = TaskTriggerInfo.TriggerInterval,
                IntervalTicks = TimeSpan.FromHours(24).Ticks
            };
        }

        public string Name => "Download Movies Theme Songs (beta)";
        public string Key => "DownloadMoviesThemeSongs";
        public string Description => "Scans all libraries to download Movies Theme Songs";
        public string Category => "Theme Songs";
    }



}
