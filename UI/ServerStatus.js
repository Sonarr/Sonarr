window.NzbDrone.ApiRoot = '/api';

var statusText = $.ajax({
    type : 'GET',
    url  : window.NzbDrone.ApiRoot + '/system/status',
    async: false,
    headers: {
        Authorization: window.NzbDrone.ApiKey
    }
}).responseText;

window.NzbDrone.ServerStatus = JSON.parse(statusText);

var footerText = window.NzbDrone.ServerStatus.version;

$(document).ready(function () {
    if (window.NzbDrone.ServerStatus.branch != 'master') {
        footerText += '</br>' + window.NzbDrone.ServerStatus.branch;
    }
    
    var len = window.NzbDrone.ServerStatus.freeSpace.length;

    var freespacestring = new String();
    for (var i = 0; i < len; i++) {
        var diskspace = window.NzbDrone.ServerStatus.freeSpace[i];
        freespacestring += diskspace.driveLetter + ' ' + diskspace.freeSpace + ' ';
    }

    $('#footer-region .freespace').html(freespacestring);


    $('#footer-region .version').html(footerText);
});

