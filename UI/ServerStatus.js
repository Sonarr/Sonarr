window.ApiRoot = '/api';

var statusText = $.ajax({
    type : 'GET',
    url  : window.ApiRoot + '/system/status',
    async: false
}).responseText;

window.ServerStatus = JSON.parse(statusText);

var footerText = window.ServerStatus.version;

$(document).ready(function () {
    if (window.ServerStatus.branch != 'master') {
        footerText = '</br>' + window.ServerStatus.branch;
    }
    $('#footer-region .version').html(footerText);
});

