window.ApiRoot = '/api';

var statusText = $.ajax({
    type : 'GET',
    url  : window.ApiRoot + '/system/status',
    async: false
}).responseText;

window.ServerStatus = JSON.parse(statusText);
