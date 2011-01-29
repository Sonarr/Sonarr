<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

<script type="text/javascript">
    jQuery(document).ready(function () {

        document.getElementById('syncTimer').style.display = 'block'; //Show the timer after the page loads, prevents FOUC (Flash of Unstyled Content)

        $('#syncTimer').tgcCountdown({
            counter: '<span style="color: #065EFE;">[H]:[M]:[S]</span>',
            counter_warning: '<span style="color: #065EFE;">[H]:[M]:[S]</span>',
            counter_expired: '<span style="color: #FFFFFF;">00:00:00</span>',
            interval: 1000,
            warnonminutesleft: 1     
    });
});
</script>

<div>RSS Sync:</div>
<div style="display:none" id="syncTimer" class="timer"><%: ViewData["RssTimer"] %></div>


