using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class ErrorPage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (!string.IsNullOrEmpty(Session["Error"] as string))
            {
                lblError.Text = Session["Error"] as string;
                Session["Error"] = "";
            }
            else
            {
                lblError.Text = "Something went wrong, please try again later.";
            }
        }
    }
}