using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;

namespace YouTubeBot.YouTube
{
    public class YouTubeEngine
    {
        //TO GET THE API KEY, USE GOOGLE CLOUD CONSOLE
        private readonly string channelId = "UCMt7ZwKIAoE3tIDudviqUSA";
        private readonly string apiKey = "AIzaSyDhcgQlQ52RfhusRL4Et1wiJfwJSo61Iko";

        public YouTubeVideo GetLatestVideo()
        {
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
                        var videoId = searchResult.Id.VideoId;

                        return new YouTubeVideo() // Storing in a class for use in the bot
                        {
                            videoId = searchResult.Id.VideoId,
                            videoUrl = $"https://www.youtube.com/watch?v={videoId}",
                            videoTitle = searchResult.Snippet.Title,
                            thumbnail = searchResult.Snippet.Thumbnails.Default__.Url,
                            PublishedAt = searchResult.Snippet.PublishedAtDateTimeOffset.Value.DateTime
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
                // Extract minutes and seconds parts
                string minutesPart = "";
                string secondsPart = "";

                if (duration.Contains("M"))
                {
                    minutesPart = duration.Substring(2, duration.IndexOf("M") - 2); // Extract minutes part
                }

                if (duration.Contains("S"))
                {
                    secondsPart = duration.Substring(duration.IndexOf("M") + 1, duration.Length - (duration.IndexOf("M") + 2)); // Extract seconds part
                }

                int totalSeconds = 0;

                if (!string.IsNullOrEmpty(minutesPart))
                {
                    totalSeconds += int.Parse(minutesPart) * 60; // Convert minutes to seconds
                }

                if (!string.IsNullOrEmpty(secondsPart))
                {
                    totalSeconds += int.Parse(secondsPart); // Add seconds
                }

                // Define a threshold for what can be considered a YouTube Short (e.g., videos less than 2 minutes)
                const int ShortThresholdSeconds = 65;

                return totalSeconds < ShortThresholdSeconds;
            }

            return false;
        }
    }
}
