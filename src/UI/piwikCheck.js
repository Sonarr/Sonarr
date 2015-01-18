'use strict';
if(window.NzbDrone.Analytics) {
    var d = document;
    var g = d.createElement('script');
    var s = d.getElementsByTagName('script')[0];
    g.type = 'text/javascript';
    g.async = true;
    g.defer = true;
    g.src = 'http://piwik.nzbdrone.com/piwik.js';
    s.parentNode.insertBefore(g, s);
}

// piwik is used to send anonymous information about your browser and which parts of the web interface you use to Sonarr servers.
// We use this information to prioritize features and browser support.
// We will NEVER include any personal information or any information that could identify you.
//
// You can opt out of this in general settings