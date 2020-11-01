using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using YouTubeSearch;
using YoutubeExplode;

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
				IncludeItemTypes = new[] { nameof(Series) },
				IsVirtualItem = false,
				Recursive = true,
				HasTvdbId = true
			}).Select(m => m as Series);
		}

		private IEnumerable<Movie> GetMoviesFromLibrary()
		{
			return _libraryManager.GetItemList(new InternalItemsQuery
			{
				IncludeItemTypes = new[] { nameof(Movie) },
				IsVirtualItem = false,
				Recursive = true,
				HasImdbId = true
			}).Select(m => m as Movie);
		}

		public void DownloadAllTVThemeSongs()
		{
			var series = GetSeriesFromLibrary();
			foreach (var serie in series)
			{
				if (!hasThemeSong(serie))
				{
					var tvdb = serie.GetProviderId(MetadataProvider.Tvdb);
					var themeSongPath = Path.Join(serie.Path, "theme.mp3");
					var link = $"http://tvthemes.plexapp.com/{tvdb}.mp3";
					_logger.LogDebug("Trying to download {seriesName}, {link}", serie.Name, link);

					try
					{
						using var client = new WebClient();
						client.DownloadFile(link, themeSongPath);
						_logger.LogInformation("{serieName} theme song downloaded", serie.Name);
					}
					catch (Exception e)
					{
						_logger.LogError("{serieName} not found, or no internet connection", serie.Name);
					}
				}
			}
		}

		public bool hasThemeSong(BaseItem item)
		{
			return item.ThemeSongIds.Length != 0;
		}

		public async Task DownloadAllMoviesThemeSongsAsync()
		{
			VideoSearch videoSearch = new VideoSearch();
			var movies = GetMoviesFromLibrary();
			foreach (var movie in movies)
			{

				var title = String.Format("{0} {1} Soundtrack", movie.Name, movie.ProductionYear);
				_logger.LogInformation(title);

				var results = await videoSearch.GetVideos(title, 1);
				

				var link = results[0].getUrl();
				_logger.LogInformation(link);
				try
				{
					await downloadYoutubeAudioAsync(movie.ContainingFolderPath, link);
				}
				catch(Exception e)
				{
					_logger.LogDebug(e.Message,"Error trying to download " + title + "Theme Song" );
					_logger.LogInformation("error");

				}


				

				
			}
		}

		private async Task downloadYoutubeAudioAsync( String path,String link)
		{
			var youtube = new YoutubeClient();
			var streamManifest = await youtube.Videos.Streams.GetManifestAsync(parseLink(link));
			_logger.LogInformation(parseLink(link));
			var streamInfo = streamManifest.GetAudioOnly().FirstOrDefault();

			if (streamInfo != null)
			{

				// Download the stream to file
				await youtube.Videos.Streams.DownloadAsync(streamInfo, Path.Join(path, "theme.mp3"));
			}

			

		}

		private String parseLink(String link)
		{
			var str = link.ToCharArray();
			String result = "";
			bool symbolReached = false;
			foreach(var character in str)
			{
				

				if (symbolReached)
				{
					result += character;
				}

				if (character == '=')
				{
					symbolReached = true;
				}

			}
			return result;
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
