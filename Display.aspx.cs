using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

public partial class Display : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            try
            {
                //Calling Twitter API to get most recent 10 tweets during initial load
                CallGetTweets();
            }
            catch
            {
                Response.Redirect("ErrorPage.aspx");
            }
        }
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        try
        {
            DataTable dt = Session["Tweets"] as DataTable;
            //If initial load had tweets, filter it with input text 
            if (dt.Rows.Count > 0 && txtSearch.Text.Trim().Length > 0)
            {
                DataTable dtResult = dt.Clone(); //Creating empty datatable with similar structure of the original one
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["TweetText"].ToString().ToLower().Contains(txtSearch.Text.Trim().ToLower())) //Matching input text irrespective of case
                    {
                        dtResult.Rows.Add(dr.ItemArray); //If match found, importing matched row to the resulting datatable
                        dtResult.AcceptChanges();
                    }
                }
                if (dtResult.Rows.Count > 0) //If searched text found, assigning the resulted datatable to the repeater control 
                {
                    Session["Tweets"] = dtResult;
                    rpttweet.DataSource = dtResult;
                    rpttweet.DataBind();
                }
            }
        }
        catch
        {
            Response.Redirect("ErrorPage.aspx");
        }
    }

    protected void tmrAutoRefresh_Tick(object sender, EventArgs e)
    {
        txtSearch.Text = "";
        //Calling Twitter API every 60 seconds (defined in design page) to get most recent 10 tweets, this is for auto refresh
        try
        {
            //Calling Twitter API to get most recent 10 tweets during initial load
            CallGetTweets();
        }
        catch
        {
            Response.Redirect("ErrorPage.aspx");
        }
    }

    private void CallGetTweets()
    {
        //Function to get tweets
        TwitterController twitter = new TwitterController();
        DataTable dtRepeater = twitter.GetTwitts("salesforce", 10);
        if (dtRepeater != null)
        {
            Session["Tweets"] = dtRepeater; //Storing the tweet datatable into session for filtering later during search
            rpttweet.DataSource = dtRepeater;
            rpttweet.DataBind();
        }
        else
        {
            Session["Error"] = "No tweet found for user @salesforce";
            Response.Redirect("ErrorPage.aspx");
        }
    }

    protected void rpttweet_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        //Changing font of data bound items
        foreach (RepeaterItem items in rpttweet.Items)
        {
            Label lblUserName = (Label)items.FindControl("lblUserName");
            Label lblScreenName = (Label)items.FindControl("lblScreenName");
            Label lblTweetDate = (Label)items.FindControl("lblTweetDate");
            Label lblTweetContent = (Label)items.FindControl("lblTweetContent");
            Label lblReTweetCount = (Label)items.FindControl("lblReTweetCount");

            lblUserName.Attributes.Add("style", "font-family:Calibri;");
            lblScreenName.Attributes.Add("style", "font-family:Calibri;");
            lblTweetDate.Attributes.Add("style", "font-family:Calibri;");
            lblTweetContent.Attributes.Add("style", "font-family:Calibri;");
            lblReTweetCount.Attributes.Add("style", "font-family:Calibri;");
        }
    }
}
