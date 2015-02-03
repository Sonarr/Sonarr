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