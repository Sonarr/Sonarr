var Marionette = require('marionette');

module.exports = Marionette.ItemView.extend({
    template : 'Movie/Details/InfoViewTemplate',

    initialize : function(options) {
        this.movieFileCollection = options.movieFileCollection;
        this.quality = null;
        this.sizeOnDisk = 0;

        this.listenTo(this.model, 'change', this.render);
        this.listenTo(this.movieFileCollection, 'sync', this.syncDone);
    },

    setQuality : function () {
        this.movieFile = this.movieFileCollection.get(this.model.get('movieFileId'));
        if (this.movieFile) {
            this.quality = this.movieFile.get('quality');
            this.sizeOnDisk = this.movieFile.get('size');
            this.listenTo(this.movieFile, 'change', this.syncDone);
        }
    },

    syncDone : function () {
        this.setQuality();
        this.render();
    },

    templateHelpers : function() {
        return {
            fileCount : this.movieFileCollection.length,
            quality : this.quality,
            sizeOnDisk : this.sizeOnDisk
        };
    }
});