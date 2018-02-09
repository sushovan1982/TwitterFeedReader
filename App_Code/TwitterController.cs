using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.IO;

public class TwitterController : ApiController
{
    public DataTable GetTwitts(string userName, int count, string accessToken = null)
    {
        try
        {
            //Fetching access token for accessing Twitter API 
            accessToken = GetAccessToken();
            //This is for workaround to handle response JSON problem
            //Last 3-4 tweets are not returning entities->expanded_url which is responsible for getting the tweet image
            //So, increasing the target tweet count by 5, later last 5 tweets will be discarded
            count = count + 5;
            //GET request to Twitter API
            var requestUserTimeline = new HttpRequestMessage(HttpMethod.Get, string.Format("https://api.twitter.com/1.1/statuses/user_timeline.json?count={0}&screen_name={1}&trim_user=0&exclude_replies=0", count, userName));

            requestUserTimeline.Headers.Add("Authorization", "Bearer " + accessToken);
            var httpClient = new HttpClient();
            HttpResponseMessage responseUserTimeLine = httpClient.SendAsync(requestUserTimeline).GetAwaiter().GetResult();//Avoiding asynchronous call
            var serializer = new JavaScriptSerializer();
            dynamic json = serializer.Deserialize<object>(responseUserTimeLine.Content.ReadAsStringAsync().GetAwaiter().GetResult());//Avoiding asynchronous call
            var enumerableTwitts = (json as IEnumerable<dynamic>);

            if (enumerableTwitts == null)
            {
                return null;
            }
            //If IEnumerable received JSON object, filter it for each target fields
            var tweetText = enumerableTwitts.Select(t => (string)(t["text"].ToString()));
            var tweetContent = enumerableTwitts.Select(t => (Dictionary<string, object>)(t["entities"]) as Dictionary<string, object>);
            var retweetCount = enumerableTwitts.Select(t => (int)(t["retweet_count"]));
            var tweetDate = enumerableTwitts.Select(t => (string)(t["created_at"]));
            var user = enumerableTwitts.Select(t => (Dictionary<string, object>)(t["user"]) as Dictionary<string, object>);
            //Creating datatable
            DataTable dtTweets = CreateDatatable();
            //Populating datatable
            for (int i = 0; i < count; i++)
            {
                DataRow dr = dtTweets.NewRow();
                dtTweets.Rows.Add(dr);
                dtTweets.AcceptChanges();
            }

            int counter = 0;
            foreach (var items in tweetText)
            {
                dtTweets.Rows[counter]["TweetText"] = items.ToString();
                counter++;
            }

            counter = 0;
            //Nested loop to iterate through the child elements
            foreach (var items in tweetContent)
            {
                IEnumerable<dynamic> tweetURL = (IEnumerable<dynamic>)items["urls"];
                foreach (var urlItems in tweetURL)
                {
                    //Getting the actual image URL by hitting entities->expanded_url element
                    string imgUrl = GetImage(urlItems["expanded_url"].ToString());

                    dtTweets.Rows[counter]["TweetContent"] = imgUrl;
                    counter++;
                }
            }

            counter = 0;
            foreach (var items in retweetCount)
            {
                dtTweets.Rows[counter]["RetweetCount"] = items.ToString();
                counter++;
            }

            counter = 0;
            foreach (var items in tweetDate)
            {
                dtTweets.Rows[counter]["TweetDate"] = items.ToString().Substring(0, items.ToString().IndexOf("+"));
                counter++;
            }

            counter = 0;
            foreach (var items in user)
            {
                dtTweets.Rows[counter]["UserName"] = items["name"].ToString();
                dtTweets.Rows[counter]["ScreenName"] = "@" + items["screen_name"].ToString() + " - ";
                dtTweets.Rows[counter]["ProfileImage"] = items["profile_image_url_https"].ToString();
                counter++;
            }

            //This is for workaround to handle response JSON problem
            //Last 3-4 tweets are not returning entities->expanded_url which is responsible for getting the tweet image
            //So, increased the target tweet count by 5, now discarding last 5 tweets 
            dtTweets.Rows[dtTweets.Rows.Count - 1].Delete();
            dtTweets.Rows[dtTweets.Rows.Count - 2].Delete();
            dtTweets.Rows[dtTweets.Rows.Count - 3].Delete();
            dtTweets.Rows[dtTweets.Rows.Count - 4].Delete();
            dtTweets.Rows[dtTweets.Rows.Count - 5].Delete();
            dtTweets.AcceptChanges();

            return dtTweets;
        }
        catch
        {
            return null;
        }
    }

    private string GetAccessToken()
    {
        try
        {
            string OAuthConsumerSecret = "pkBhYoyXMMn3MinByk9BE2uJCB0t8rQGBQG2tlZESLuYsQiy8T"; //ConsumerSecret key received from https://apps.twitter.com/
            string OAuthConsumerKey = "XOp4HPDa8r3VtX55tdWLxnNkL"; //ConsumerKey received from https://apps.twitter.com/

            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.twitter.com/oauth2/token ");
            var customerInfo = Convert.ToBase64String(new UTF8Encoding().GetBytes(OAuthConsumerKey + ":" + OAuthConsumerSecret));
            request.Headers.Add("Authorization", "Basic " + customerInfo);
            request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            HttpResponseMessage response = httpClient.SendAsync(request).GetAwaiter().GetResult();//Avoiding asynchronous call

            string json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();//Avoiding asynchronous call
            var serializer = new JavaScriptSerializer();
            dynamic item = serializer.Deserialize<object>(json);
            return (string)item["access_token"];
        }
        catch
        {
            return null;
        }
    }

    private DataTable CreateDatatable()
    {
        DataTable dt = new DataTable();

        dt.Columns.Add("UserName");
        dt.Columns.Add("ScreenName");
        dt.Columns.Add("ProfileImage");
        dt.Columns.Add("TweetText");
        dt.Columns.Add("TweetContent");
        dt.Columns.Add("RetweetCount");
        dt.Columns.Add("TweetDate");

        return dt;
    }

    private string GetImage(string url)
    {
        try
        {
            //Getting the actual image URL by hitting entities->expanded_url element
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            string imgUrl = null;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                imgUrl = readStream.ReadToEnd();
                imgUrl = imgUrl.Substring(imgUrl.IndexOf("og:image") + 19);
                imgUrl = imgUrl.Substring(0, imgUrl.IndexOf(">") - 1);
                if (imgUrl.Contains(".jpg"))
                {
                    imgUrl = imgUrl.Substring(0, imgUrl.IndexOf(".jpg") + 4);
                }
                else if (imgUrl.Contains(".png"))
                {
                    imgUrl = imgUrl.Substring(0, imgUrl.IndexOf(".png") + 4);
                }
                else if (imgUrl.Contains(".gif"))
                {
                    imgUrl = imgUrl.Substring(0, imgUrl.IndexOf(".gif") + 4);
                }
                else if (imgUrl.Contains(".bmp"))
                {
                    imgUrl = imgUrl.Substring(0, imgUrl.IndexOf(".bmp") + 4);
                }
                else
                {
                    return "~/Images/NoImage.jpg";
                }

                response.Close();
                readStream.Close();
            }
            return imgUrl;
        }
        catch
        {
            return "~/Images/NoImage.jpg";
        }
    }
}
