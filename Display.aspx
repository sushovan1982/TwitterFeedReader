<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Display.aspx.cs" Inherits="Display" %>



<!DOCTYPE html>



<html xmlns="http://www.w3.org/1999/xhtml">


<head runat="server">


    <title>Twitter Feed Reader</title>


    <script type="text/javascript">


        function blankCheck() {


            var txt = document.getElementById("<%=txtSearch.ClientID%>").value.trim();


            if (txt == "") {


                alert('The search box should not be empty...');


                document.getElementById("<%=txtSearch.ClientID%>").focus();


                return false;


            }


        }


    </script>


</head>


<body>


    <form id="frmDisplay" runat="server" style="background-color: aliceblue">


        <asp:ScriptManager runat="server" EnablePartialRendering="true"></asp:ScriptManager>


        <div style="width: 600px; margin: 0 auto; background-color: white;">


            <asp:UpdatePanel ID="updTweets" runat="server">


                <ContentTemplate>


                    <asp:TextBox ID="txtSearch" runat="server"></asp:TextBox>


                    <asp:Button ID="btnSearch" runat="server" Text="Search Tweets" OnClick="btnSearch_Click" OnClientClick="blankCheck()" />


                    <asp:Timer ID="tmrAutoRefresh" runat="server" Interval="60000" OnTick="tmrAutoRefresh_Tick"></asp:Timer>


                    <br />


                    <asp:Repeater ID="rpttweet" runat="server" OnItemDataBound="rpttweet_ItemDataBound">


                        <ItemTemplate>


                            <table>


                                <tr>


                                    <td>


                                        <asp:Image ID="imgProfilePicture" Height="50" Width="50" ImageAlign="Bottom" runat="server" ImageUrl='<%# Eval("ProfileImage") %>' />


                                    </td>


                                    <td>


                                        <asp:Label ID="lblUserName" runat="server" Font-Bold="true" Text='<%# Eval("UserName") %>'></asp:Label>


                                        <asp:Label ID="lblScreenName" runat="server" Text='<%# Eval("ScreenName") %>'></asp:Label>


                                        <asp:Label ID="lblTweetDate" runat="server" Text='<%# Eval("TweetDate") %>'></asp:Label>


                                    </td>


                                </tr>


                                <tr>


                                    <td>&nbsp</td>


                                    <td>


                                        <asp:Label ID="lblTweetContent" runat="server" Text='<%# Eval("TweetText") %>'></asp:Label>


                                    </td>


                                </tr>


                                <tr>


                                    <td>&nbsp</td>


                                    <td>


                                        <asp:Image ID="imgTweetContent" Height="270" Width="490" runat="server" ImageUrl='<%# Eval("TweetContent") %>' />


                                    </td>


                                </tr>


                                <tr>


                                    <td>&nbsp</td>


                                    <td>


                                        <asp:Image ID="imgReTweetCount" Height="20" Width="20" runat="server" ImageUrl="~/Images/Retweet.png" ImageAlign="Bottom" ToolTip="Retweet" />


                                        <asp:Label ID="lblReTweetCount" runat="server" Text='<%# Eval("RetweetCount") %>'></asp:Label>


                                    </td>


                                </tr>


                                <tr>


                                    <td colspan="2">


                                        <asp:Image ID="imgLine" Width="595" ImageUrl="~/Images/Line.jpg" runat="server" /></td>


                                </tr>


                            </table>


                        </ItemTemplate>


                    </asp:Repeater>


                </ContentTemplate>


            </asp:UpdatePanel>


        </div>


    </form>


</body>


</html>
