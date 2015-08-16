var Backbone = require('backbone');

module.exports = Backbone.Model.extend({
    urlRoot  : window.NzbDrone.ApiRoot + '/rootfolder',
    defaults : {
        freeSpace : 0
    }
});