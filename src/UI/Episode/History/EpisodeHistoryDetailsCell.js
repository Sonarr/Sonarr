var $ = require('jquery');
var vent = require('vent');
var Marionette = require('marionette');
var NzbDroneCell = require('../../Cells/NzbDroneCell');
var HistoryDetailsView = require('../../Activity/History/Details/HistoryDetailsView');
require('bootstrap');

module.exports = NzbDroneCell.extend({
    className : 'episode-history-details-cell',

    render : function() {
        this.$el.empty();
        this.$el.html('<i class="icon-sonarr-form-info"></i>');

        var html = new HistoryDetailsView({ model : this.model }).render().$el;

        this.$el.popover({
            content   : html,
            html      : true,
            trigger   : 'hover',
            title     : 'Details',
            placement : 'left',
            container : this.$el
        });

        return this;
    }
});