var Marionette = require('marionette');

module.exports = Marionette.ItemView.extend({
    template : 'Series/Details/InfoViewTemplate',

    initialize : function(options) {
        this.episodeFileCollection = options.episodeFileCollection;

        this.listenTo(this.model, 'change', this.render);
        this.listenTo(this.episodeFileCollection, 'sync', this.render);
    },

    templateHelpers : function() {
        return {
            fileCount : this.episodeFileCollection.length
        };
    }
});