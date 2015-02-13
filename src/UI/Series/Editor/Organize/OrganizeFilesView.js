var _ = require('underscore');
var vent = require('vent');
var Backbone = require('backbone');
var Marionette = require('marionette');
var CommandController = require('../../../Commands/CommandController');

module.exports = Marionette.ItemView.extend({
    template : 'Series/Editor/Organize/OrganizeFilesViewTemplate',

    events : {
        'click .x-confirm-organize' : '_organize'
    },

    initialize : function(options) {
        this.series = options.series;
        this.templateHelpers = {
            numberOfSeries : this.series.length,
            series         : new Backbone.Collection(this.series).toJSON()
        };
    },

    _organize : function() {
        var seriesIds = _.pluck(this.series, 'id');

        CommandController.Execute('renameSeries', {
            name      : 'renameSeries',
            seriesIds : seriesIds
        });

        this.trigger('organizingFiles');
        vent.trigger(vent.Commands.CloseModalCommand);
    }
});