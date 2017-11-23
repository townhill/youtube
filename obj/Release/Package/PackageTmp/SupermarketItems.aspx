<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SupermarketItems.aspx.cs" Inherits="youtube.SupermarketItems" %>
<script runat="server">
protected void Page_Load(object sender, EventArgs e)
{
    Response.Redirect(string.Format("http://{0}/Supermarkets/Search/{1}", Request.Url.Host, Request.QueryString.Get("search")));
}
</script>