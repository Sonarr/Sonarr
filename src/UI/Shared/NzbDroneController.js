var vent = require('../vent');
var AppLayout = require('../AppLayout');
var Marionette = require('marionette');
var NotFoundView = require('./NotFoundView');

module.exports = Marionette.AppRouter.extend({
    initialize       : function(){
        this.listenTo(vent, vent.Events.ServerUpdated, this._onServerUpdated);
    },
    showNotFound     : function(){
        this.setTitle('Not Found');
        this.showMainRegion(new NotFoundView(this));
    },
    setTitle         : function(title){
        title = title;
        if(title === 'Sonarr') {
            document.title = 'Sonarr';
        }
        else {
            document.title = title + ' - Sonarr';
        }
        if(window.NzbDrone.Analytics && window.Piwik) {
            try {
                var piwik = window.Piwik.getTracker('http://piwik.nzbdrone.com/piwik.php', 1);
                piwik.setReferrerUrl('');
                piwik.setCustomUrl('http://local' + window.location.pathname);
                piwik.setCustomVariable(1, 'version', window.NzbDrone.Version, 'page');
                piwik.setCustomVariable(2, 'branch', window.NzbDrone.Branch, 'page');
                piwik.trackPageView(title);
            }
            catch (e) {
                console.error(e);
            }
        }
    },
    _onServerUpdated : function(){
        this.pendingUpdate = true;
    },
    showMainRegion   : function(view){
        if(this.pendingUpdate) {
            window.location.reload();
        }
        else {
            AppLayout.mainRegion.show(view);
        }
    }
});