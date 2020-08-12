using System;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Services;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ThemeSongs.Api
{
    [Route("/ThemeSongs/Download", "POST", Summary = "Downloads theme songs")]
    [Authenticated]
    public class DownloadRequest : IReturnVoid
    {
    }

    public class GetId 
    {
        [ApiMember(Name = "Id", Description = "Item Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "GET")]
        public Guid Id { get; set; }
    }

    public class ThemeSongsService : IService
    {
        private readonly ThemeSongsManager _themeSongsManager;
        private readonly ILogger<ThemeSongsService> _logger;

        public ThemeSongsService(
            ILibraryManager libraryManager,
            ILogger<ThemeSongsService> logger)
        {
            _themeSongsManager = new ThemeSongsManager(libraryManager,  logger);
            _logger = logger;
        }
        
        public void Post(DownloadRequest request)
        {
            _logger.LogInformation("Starting a manual refresh, looking up for repeated versions");
            _themeSongsManager.DownloadAllThemeSongs();
            _logger.LogInformation("Completed refresh");
        }

        

    }
}