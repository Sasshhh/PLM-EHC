<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="C8.eServices.Mvc.DataAccessLayer" %>
<%@ Import Namespace="C8.eServices.Mvc.Helpers" %>
<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            using (var db = new eServicesDbContext())
            {
                MatchingHelper.RenewalNotificationAtEndOfTime(db);
                Response.Write("SUCCESS: Scanner ran successfully.");
            }
        }
        catch (Exception ex)
        {
            Response.Write("ERROR: " + ex.Message + "<br/>" + ex.StackTrace);
        }
    }
</script>
