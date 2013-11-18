window.NzbDrone.ApiRoot = '/api';

var statusText = window.$.ajax({
    type : 'GET',
    url  : window.NzbDrone.ApiRoot + '/system/status',
    async: false,
    headers: {
        Authorization: window.NzbDrone.ApiKey
    }
}).responseText;

window.NzbDrone.ServerStatus = JSON.parse(statusText);

var footerText = window.NzbDrone.ServerStatus.version;

window.$(document).ready(function () {
    if (window.NzbDrone.ServerStatus.branch !== 'master') {
        footerText += '</br>' + window.NzbDrone.ServerStatus.branch;
    }
    window.$('#footer-region .version').html(footerText);
});




