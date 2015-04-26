var $ = require('jquery');
var vent = require('vent');

$(document).ajaxSuccess(function(event, xhr) {
    var version = xhr.getResponseHeader('X-ApplicationVersion');
    if (!version || !window.NzbDrone || !window.NzbDrone.Version) {
        return;
    }

    if (version !== window.NzbDrone.Version) {
        vent.trigger(vent.Events.ServerUpdated);
    }
});
