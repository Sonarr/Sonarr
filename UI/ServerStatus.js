window.NzbDrone = {};
window.NzbDrone.ApiRoot = '/api';

var statusText = $.ajax({
    type : 'GET',
    url  : window.NzbDrone.ApiRoot + '/system/status',
    async: false
}).responseText;

window.NzbDrone.ServerStatus = JSON.parse(statusText);

var footerText = window.NzbDrone.ServerStatus.version;

$(document).ready(function () {
    if (window.NzbDrone.ServerStatus.branch != 'master') {
        footerText += '</br>' + window.NzbDrone.ServerStatus.branch;
    }
    $('#footer-region .version').html(footerText);
});

