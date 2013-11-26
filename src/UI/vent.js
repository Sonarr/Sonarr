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
            CommandComplete: 'command:complete',
            ServerUpdated  : 'server:updated'
        };

        vent.Commands = {
            EditSeriesCommand  : 'EditSeriesCommand',
            DeleteSeriesCommand: 'DeleteSeriesCommand',
            OpenModalCommand   : 'OpenModalCommand',
            CloseModalCommand  : 'CloseModalCommand',
            ShowEpisodeDetails : 'ShowEpisodeDetails',
            ShowHistoryDetails : 'ShowHistoryDetails',
            ShowLogDetails     : 'ShowLogDetails',
            SaveSettings       : 'saveSettings',
            ShowLogFile        : 'showLogFile',
            ShowRenamePreview  : 'showRenamePreview'
        };

        return vent;
    });
