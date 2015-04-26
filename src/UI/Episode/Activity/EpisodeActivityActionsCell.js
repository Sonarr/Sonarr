var $ = require('jquery');
var vent = require('vent');
var Marionette = require('marionette');
var NzbDroneCell = require('../../Cells/NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'episode-actions-cell',

    events : {
        'click .x-failed' : '_markAsFailed'
    },

    render : function() {
        this.$el.empty();

        if (this.model.get('eventType') === 'grabbed') {
            this.$el.html('<i class="icon-sonarr-delete x-failed" title="Mark download as failed"></i>');
        }

        return this;
    },

    _markAsFailed : function() {
        var url = window.NzbDrone.ApiRoot + '/history/failed';
        var data = {
            id : this.model.get('id')
        };

        $.ajax({
            url  : url,
            type : 'POST',
            data : data
        });
    }
});