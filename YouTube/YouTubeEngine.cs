using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;

namespace YouTubeBot.YouTube
{
    public class YouTubeEngine
    {
        //TO GET THE API KEY, USE GOOGLE CLOUD CONSOLE
        private readonly string channelId = "UCMt7ZwKIAoE3tIDudviqUSA";
        private readonly string apiKey = "AIzaSyDYTxDyFBdDZXIMno2H5H-MHDpxTMB-KcQ";

        public YouTubeVideo GetLatestVideo()
        {
            // Temporary variables for Video info
            string videoId;
            string videoUrl;
            string videoTitle;
            DateTime? videoPublishedAt;

            // Initializing the API
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = "MyApp"
            });

            // Setting up our video search query
            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.ChannelId = channelId;
            searchListRequest.MaxResults = 1;
            searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;

            // Executing the search
            var searchListResponse = searchListRequest.Execute();

            foreach (var searchResult in searchListResponse.Items)
            {
                if (searchResult.Id.Kind == "youtube#video" && searchResult.Id.VideoId != null && !searchResult.Snippet.LiveBroadcastContent.Equals("upcoming"))
                {
                    // Fetch video details including duration
                    var videoRequest = youtubeService.Videos.List("snippet,contentDetails");
                    videoRequest.Id = searchResult.Id.VideoId;
                    var videoResponse = videoRequest.Execute();

                    var videoDuration = videoResponse.Items[0].ContentDetails.Duration;

                    if (!IsShortVideo(videoDuration)) // Exclude YouTube Shorts
                    {
                        videoId = searchResult.Id.VideoId; // Setting our details
                        videoUrl = $"https://www.youtube.com/watch?v={videoId}";
                        videoTitle = searchResult.Snippet.Title;
                        videoPublishedAt = searchResult.Snippet.PublishedAt;
                        var thumbnail = searchResult.Snippet.Thumbnails.Default__.Url;

                        return new YouTubeVideo() // Storing in a class for use in the bot
                        {
                            videoId = videoId,
                            videoUrl = videoUrl,
                            videoTitle = videoTitle,
                            thumbnail = thumbnail,
                            PublishedAt = videoPublishedAt
                        };
                    }

                    else
                    {
                        Console.WriteLine($"[{DateTime.Now}] YouTube API: Shorts Video detected, ignoring");
                    }
                }
            }

            //If foreach loop didn't get anything, return nothing
            return null;
        }

        private bool IsShortVideo(string duration)
        {
            if (duration.StartsWith("PT") && duration.EndsWith("S"))
            {
                var secondsPart = duration.Substring(2, duration.Length - 3); // Extract seconds part
                if (int.TryParse(secondsPart, out int seconds))
                {
                    // Define a threshold for what can be considered a YouTube Short (e.g., videos less than 2 minutes)
                    const int ShortThresholdSeconds = 60;

                    return seconds < ShortThresholdSeconds;
                }
            }

            return false;
        }
    }
}
