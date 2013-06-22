var statusText = $.ajax({
    type : 'GET',
    url  : '/api/system/status',
    async: false
}).responseText;

window.ServerStatus = JSON.parse(statusText);
