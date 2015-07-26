var _ = require('underscore');
var $ = require('jquery');
var Marionette = require('marionette');
var vent = require('vent');
var RootFolders = require('../AddSeries/RootFolders/RootFolderCollection');

module.exports = Marionette.ItemView.extend({
    template : 'SeasonPass/SeasonPassFooterViewTemplate',

    ui : {
        seriesMonitored : '.x-series-monitored',
        monitor         : '.x-monitor',
        selectedCount   : '.x-selected-count',
        container       : '.series-editor-footer',
        actions         : '.x-action',
        indicator       : '.x-indicator',
        indicatorIcon   : '.x-indicator-icon'
    },

    events : {
        'click .x-update' : '_update'
    },

    initialize : function(options) {
        this.seriesCollection = options.collection;

        RootFolders.fetch().done(function() {
            RootFolders.synced = true;
        });

        this.editorGrid = options.editorGrid;
        this.listenTo(this.seriesCollection, 'backgrid:selected', this._updateInfo);
    },

    onRender : function() {
        this._updateInfo();
    },

    _update : function() {
        var self = this;
        var selected = this.editorGrid.getSelectedModels();
        var seriesMonitored = this.ui.seriesMonitored.val();
        var monitoringOptions;

        _.each(selected, function(model) {
            if (seriesMonitored === 'true') {
                model.set('monitored', true);
            } else if (seriesMonitored === 'false') {
                model.set('monitored', false);
            }

            monitoringOptions = self._getMonitoringOptions(model);
            model.set('addOptions', monitoringOptions);
        });

        var promise = $.ajax({
            url  : window.NzbDrone.ApiRoot + '/seasonpass',
            type : 'POST',
            data : JSON.stringify({
                series            : _.map(selected, function (model) {
                    return model.toJSON();
                }),
                monitoringOptions : monitoringOptions
            })
        });

        this.ui.indicator.show();

        promise.always(function () {
            self.ui.indicator.hide();
        });

        promise.done(function () {
            self.seriesCollection.trigger('seasonpass:saved');
        });
    },

    _updateInfo : function() {
        var selected = this.editorGrid.getSelectedModels();
        var selectedCount = selected.length;

        this.ui.selectedCount.html('{0} series selected'.format(selectedCount));

        if (selectedCount === 0) {
            this.ui.actions.attr('disabled', 'disabled');
        } else {
            this.ui.actions.removeAttr('disabled');
        }
    },

    _getMonitoringOptions : function(model) {
        var monitor = this.ui.monitor.val();
        var lastSeason = _.max(model.get('seasons'), 'seasonNumber');
        var firstSeason = _.min(_.reject(model.get('seasons'), { seasonNumber : 0 }), 'seasonNumber');

        if (monitor === 'noChange') {
            return null;
        }

        model.setSeasonPass(firstSeason.seasonNumber);

        var options = {
            ignoreEpisodesWithFiles    : false,
            ignoreEpisodesWithoutFiles : false
        };

        if (monitor === 'all') {
            return options;
        }

        else if (monitor === 'future') {
            options.ignoreEpisodesWithFiles = true;
            options.ignoreEpisodesWithoutFiles = true;
        }

        else if (monitor === 'latest') {
            model.setSeasonPass(lastSeason.seasonNumber);
        }

        else if (monitor === 'first') {
            model.setSeasonPass(lastSeason.seasonNumber + 1);
            model.setSeasonMonitored(firstSeason.seasonNumber);
        }

        else if (monitor === 'missing') {
            options.ignoreEpisodesWithFiles = true;
        }

        else if (monitor === 'existing') {
            options.ignoreEpisodesWithoutFiles = true;
        }

        else if (monitor === 'none') {
            model.setSeasonPass(lastSeason.seasonNumber + 1);
        }

        return options;
    }
});