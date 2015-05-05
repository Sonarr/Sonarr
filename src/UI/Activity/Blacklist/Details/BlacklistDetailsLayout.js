var Marionette = require('marionette');
var BlacklistDetailsView = require('./BlacklistDetailsView');

module.exports = Marionette.Layout.extend({
    template : 'Activity/Blacklist/Details/BlacklistDetailsLayoutTemplate',

    regions : {
        bodyRegion : '.modal-body'
    },

    onShow : function() {
        this.bodyRegion.show(new BlacklistDetailsView({ model : this.model }));
    }
});