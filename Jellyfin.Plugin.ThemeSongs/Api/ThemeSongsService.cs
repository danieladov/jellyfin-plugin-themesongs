using System;
using MediaBrowser.Api;
using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Dto;
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
        private readonly ThemeSongsManager _mergeVersionsManager;
        private readonly ILogger<VideosService> _logger;
        private readonly IProgress<double> progress;

        public ThemeSongsService(ILibraryManager libraryManager, ICollectionManager collectionManager, ILogger<VideosService> logger, IServerConfigurationManager serverConfigurationManager,
            IHttpResultFactory httpResultFactory,
            IUserManager userManager,
            IDtoService dtoService,
            IAuthorizationContext authContext)
        {
            _mergeVersionsManager = new ThemeSongsManager( libraryManager,  collectionManager,  logger,  serverConfigurationManager,
             httpResultFactory,
             userManager,
             dtoService,
             authContext,new GetId());
            _logger = logger;
        }
        
        public void Post(DownloadRequest request)
        {
            _logger.LogInformation("Starting a manual refresh, looking up for repeated versions");
            _mergeVersionsManager.DownloadAllThemeSongs();
            _logger.LogInformation("Completed refresh");
        }

        

    }
}