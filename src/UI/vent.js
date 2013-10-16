define(
    [
        'marionette',
        'backbone'
    ], function (Marionette, Backbone) {
        'use strict';

        var vent = new Backbone.Wreqr.EventAggregator();

        vent.Events = {
            SeriesAdded    : 'series:added',
            SeriesDeleted  : 'series:deleted',
            SeasonRenamed  : 'season:renamed',
            CommandComplete: 'command:complete'
        };

        vent.Commands = {
            EditSeriesCommand  : 'EditSeriesCommand',
            DeleteSeriesCommand: 'DeleteSeriesCommand',
            CloseModalCommand  : 'CloseModalCommand',
            ShowEpisodeDetails : 'ShowEpisodeDetails',
            ShowHistoryDetails : 'ShowHistoryDetails',
            SaveSettings       : 'saveSettings',
            ShowLogFile        : 'showLogFile'
        };

        return vent;
    });
