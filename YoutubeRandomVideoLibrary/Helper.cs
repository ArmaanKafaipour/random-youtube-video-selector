using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeRandomVideoLibrary
{
    public class Helper
    {
        private static List<string> channelIdList = new List<string>();
        private static string selectedVideo = string.Empty;

        public async Task<string> GetRandomVideo(YouTubeService youtubeService)
        {
            List<string> videoIdList = new();
            bool isRandomVidFound = false;

            while (isRandomVidFound == false)
            {
                // Select a random youtube channel
                var randomChannel = RandomSelector.Select(channelIdList);

                // Search for videos from each channel ID 
                // uploaded within the past week
                var videoListRequest = youtubeService.Search.List("snippet");
                videoListRequest.ChannelId = randomChannel;
                videoListRequest.PublishedAfter = DateTime.Now.AddDays(-7);

                var videoListResponse = await videoListRequest.ExecuteAsync();

                // Number of videos uploaded within the past week
                int recentVidCount = videoListResponse.Items.Count();

                // Loop through each video uploaded in the past week
                // and add each video ID to the list
                for (int i = 0; i < recentVidCount; i++)
                {
                    videoIdList.Add(videoListResponse.Items[i].Id.VideoId);
                    
                }

                // Continue looping and searching for a video if selected channel has none available
                if(videoIdList.Count > 0)
                {
                    selectedVideo = RandomSelector.Select(videoIdList);
                    isRandomVidFound = true;               
                }

                // Make sure the video we selected is embeddable
                // If it's not, then repeat the loop and get a new video
                if (await CheckIfEmbeddable(youtubeService) == false)
                {
                    isRandomVidFound = false;
                }

            }
            return selectedVideo;

        }

        // Function to check if the video we have selected if capable of being embedded
        public async Task<bool> CheckIfEmbeddable(YouTubeService youtubeService)
        {
            var videoStatusRequest = youtubeService.Videos.List("snippet,contentDetails,status");
            videoStatusRequest.Id = selectedVideo;

            var videoStatusResponse = await videoStatusRequest.ExecuteAsync();

            return videoStatusResponse.Items[0].Status.Embeddable ?? true;
        }

        public async Task<YouTubeService> InitService()
        {

            UserCredential credential;

            var clientId = "660328448778-9te6ha1e6ts7rmct0cdn8eaqe670rees.apps.googleusercontent.com";
            var clientSecret = "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";
            // here is where we Request the user to give us access, or use the Refresh Token that was previously stored in %AppData%
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
                new[] { YouTubeService.Scope.YoutubeReadonly },
                Environment.UserName,
                CancellationToken.None,
                new FileDataStore(this.GetType().ToString()));


            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = this.GetType().ToString()
            });

            return youtubeService;
        }

       public async Task AuthorizeAsync(YouTubeService youtubeService)
       {
            var subscriptionsListRequest = youtubeService.Subscriptions.List("snippet,contentDetails");
            subscriptionsListRequest.MaxResults = 50;
            subscriptionsListRequest.Mine = true;
            var subscriptionsListResponse = await subscriptionsListRequest.ExecuteAsync();

            // Get TOTAL number of subscriptions
            int? totalSubCount = subscriptionsListResponse.PageInfo.TotalResults;
            int? subCount = subscriptionsListResponse.Items.Count();
            int subsPerPage = 50;

            // Find number of pages using TOTAL number of subscriptions (ceiling)
            int? pageCount = (totalSubCount + subsPerPage - 1) / subsPerPage;


            // Iterate through each page of results
            for (int i = 1; i <= pageCount; i++)
            {
                // Iterate through each subscription and add each channel ID to a list
                for (int j = 0; j < subCount; j++)
                {
                    channelIdList.Add(subscriptionsListResponse.Items[j].Snippet.ResourceId.ChannelId);
                }

                // Go to next page
                subscriptionsListRequest.PageToken = subscriptionsListResponse.NextPageToken;

                // Update new response from the next page
                subscriptionsListResponse = await subscriptionsListRequest.ExecuteAsync();

                // Update the new number of subscriptions on this page
                subCount = subscriptionsListResponse.Items.Count();
            }
            
        }

    }
}
