var _ = require('underscore');
var ToggleCell = require('./ToggleCell');
var SeriesCollection = require('../Series/SeriesCollection');
var Messenger = require('../Shared/Messenger');

module.exports = ToggleCell.extend({
    className : 'toggle-cell episode-monitored',

    _originalOnClick : ToggleCell.prototype._onClick,

    _onClick : function(e) {

        var series = SeriesCollection.get(this.model.get('seriesId'));

        if (!series.get('monitored')) {

            Messenger.show({
                message : 'Unable to change monitored state when series is not monitored',
                type    : 'error'
            });

            return;
        }

        if (e.shiftKey && this.model.episodeCollection.lastToggled) {
            this._selectRange();

            return;
        }

        this._originalOnClick.apply(this, arguments);
        this.model.episodeCollection.lastToggled = this.model;
    },

    _selectRange : function() {
        var episodeCollection = this.model.episodeCollection;
        var lastToggled = episodeCollection.lastToggled;

        var currentIndex = episodeCollection.indexOf(this.model);
        var lastIndex = episodeCollection.indexOf(lastToggled);

        var low = Math.min(currentIndex, lastIndex);
        var high = Math.max(currentIndex, lastIndex);
        var range = _.range(low + 1, high);

        _.each(range, function(index) {
            var model = episodeCollection.at(index);

            model.set('monitored', lastToggled.get('monitored'));
            model.save();
        });

        this.model.set('monitored', lastToggled.get('monitored'));
        this.model.save();
        this.model.episodeCollection.lastToggled = undefined;
    }
});