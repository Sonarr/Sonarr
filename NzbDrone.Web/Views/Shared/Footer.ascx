<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

<script type="text/javascript">
    jQuery(document).ready(function () {

        document.getElementById('syncTimer').style.display = 'inline'; //Show the timer after the page loads, prevents FOUC (Flash of Unstyled Content)

        $('#syncTimer').tgcCountdown({
            counter: '<div style="color: #065EFE;">[H]:[M]:[S]</div>',
            counter_warning: '<div style="color: #065EFE;">[H]:[M]:[S]</div>',
            counter_expired: '<div style="color: #FFFFFF;">00:00:00</div>',
            interval: 1000,
            warnonminutesleft: 1     
    });
});
</script>

<%--<div>RSS Sync: <span style="display:none" id="syncTimer" class="timer"><%: ViewData["RssTimer"] %></span></div>--%>
<div>RSS Sync: </div>
<div style="display:none" id="syncTimer" class="timer"><%:ViewData["RssTimer"]%></div>