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

		public async Task DownloadAllMoviesThemeSongsAsync()
		{
			_logger.LogInformation("1");
			VideoSearch videoSearch = new VideoSearch();
			var movies = GetMoviesFromLibrary();
			_logger.LogInformation(movies.Count().ToString());
			foreach (var movie in movies)
			{

				var title = String.Format("{0} {1} Soundtrack", movie.Name, movie.ProductionYear);
				_logger.LogInformation(title);

				var results = await videoSearch.GetVideos(title, 1);
				

				var link = results[0].getUrl();
				_logger.LogInformation(link);
				try
				{
					//IEnumerable<YoutubeExtractor.VideoInfo> videoInfos = YoutubeExtractor.DownloadUrlResolver.GetDownloadUrls(link);
					await downloadYoutubeAudioAsync(movie.ContainingFolderPath, link);
				}
				catch(Exception e)
				{
					_logger.LogInformation(e.Message);
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


			//var source = path;
			//var youtube = YouTube.Default;
			//var vid = youtube.GetVideo(link);
			//File.WriteAllBytes(source + vid.FullName, vid.GetBytes());

			//var inputFile = new MediaFile { Filename = source + vid.FullName };
			//var outputFile = new MediaFile { Filename = "theme.mp3" };

			//using (var engine = new Engine())
			//{
			//	engine.GetMetadata(inputFile);

			//	engine.Convert(inputFile, outputFile);
			//}


			///*
			//* We want the first extractable video with the highest audio quality.
			//*/

			//YoutubeExtractor.VideoInfo video = videoInfos
			//	.Where(info => info.CanExtractAudio)
			//	.OrderByDescending(info => info.AudioBitrate)
			//	.First();

			///*
			//          * If the video has a decrypted signature, decipher it
			//          */
			//if (video.RequiresDecryption)
			//{
			//	YoutubeExtractor.DownloadUrlResolver.DecryptDownloadUrl(video);
			//}

			///*
			//          * Create the audio downloader.
			//          * The first argument is the video where the audio should be extracted from.
			//          * The second argument is the path to save the audio file.
			//          */
			//var audioDownloader = new AudioDownloader(video, Path.Join(path, "theme.mp3"));

			//// Register the progress events. We treat the download progress as 85% of the progress and the extraction progress only as 15% of the progress,
			//// because the download will take much longer than the audio extraction.
			//audioDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage * 0.85);
			//audioDownloader.AudioExtractionProgressChanged += (sender, args) => Console.WriteLine(85 + args.ProgressPercentage * 0.15);

			///*
			//          * Execute the audio downloader.
			//          * For GUI applications note, that this method runs synchronously.
			//          */
			//audioDownloader.Execute();

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
