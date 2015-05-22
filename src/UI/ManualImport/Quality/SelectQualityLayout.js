var _ = require('underscore');
var vent = require('../../vent');
var Marionette = require('marionette');
var LoadingView = require('../../Shared/LoadingView');
var ProfileSchemaCollection = require('../../Settings/Profile/ProfileSchemaCollection');
var SelectQualityView = require('./SelectQualityView');

module.exports = Marionette.Layout.extend({
    template  : 'ManualImport/Quality/SelectQualityLayoutTemplate',

    regions : {
        quality : '.x-quality'
    },

    events : {
        'click .x-select' : '_selectQuality'
    },

    initialize : function() {
        this.profileSchemaCollection = new ProfileSchemaCollection();
        this.profileSchemaCollection.fetch();

        this.listenTo(this.profileSchemaCollection, 'sync', this._showQuality);
    },

    onRender : function() {
        this.quality.show(new LoadingView());
    },

    _showQuality : function () {
        var qualities = _.map(this.profileSchemaCollection.first().get('items'), function (quality) {
            return quality.quality;
        });

        this.selectQualityView = new SelectQualityView({ qualities: qualities });
        this.quality.show(this.selectQualityView);
    },

    _selectQuality : function () {
        this.trigger('manualimport:selected:quality', { quality: this.selectQualityView.selectedQuality() });
        vent.trigger(vent.Commands.CloseModal2Command);
    }
});