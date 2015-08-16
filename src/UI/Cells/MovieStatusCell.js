var reqres = require('../reqres');
var Backbone = require('backbone');
var NzbDroneCell = require('./NzbDroneCell');
var QueueCollection = require('../Activity/Queue/QueueCollection');
var moment = require('moment');
var FormatHelpers = require('../Shared/FormatHelpers');

module.exports = NzbDroneCell.extend({
    className : 'movie-status-cell',

    render : function() {
        this.listenTo(QueueCollection, 'sync', this._renderCell);

        this._renderCell();

        return this;
    },

    _renderCell : function() {

        this.$el.empty();

        if (this.model) {

            this.movieFile = this.model;

            if (this.movieFile) {
                this.listenTo(this.movieFile, 'change', this._refresh);

                var quality = this.movieFile.get('quality');
                var revision = quality.revision;
                var size = FormatHelpers.bytes(this.movieFile.get('size'));
                var title = 'Movie downloaded';

                if (revision.real && revision.real > 0) {
                    title += '[REAL]';
                }

                if (revision.version && revision.version > 1) {
                    title += ' [PROPER]';
                }

                if (size !== '') {
                    title += ' - {0}'.format(size);
                }

                if (this.movieFile.get('qualityCutoffNotMet')) {
                    this.$el.html('<span class="badge badge-inverse" title="{0}">{1}</span>'.format(title, quality.quality.name));
                } else {
                    this.$el.html('<span class="badge" title="{0}">{1}</span>'.format(title, quality.quality.name));
                }
            }
        }
    }
});