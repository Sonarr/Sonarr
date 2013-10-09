'use strict';
define(
    [
        'vent',
        'AppLayout',
        'marionette',
        'Series/Edit/EditSeriesView',
        'Series/Delete/DeleteSeriesView',
        'Episode/EpisodeDetailsLayout',
        'History/Details/HistoryDetailsView'
    ], function (vent, AppLayout, Marionette, EditSeriesView, DeleteSeriesView, EpisodeDetailsLayout, HistoryDetailsView) {

        return Marionette.AppRouter.extend({

            initialize: function () {
                vent.on(vent.Commands.CloseModalCommand, this._closeModal, this);
                vent.on(vent.Commands.EditSeriesCommand, this._editSeries, this);
                vent.on(vent.Commands.DeleteSeriesCommand, this._deleteSeries, this);
                vent.on(vent.Commands.ShowEpisodeDetails, this._showEpisode, this);
                vent.on(vent.Commands.ShowHistoryDetails, this._showHistory, this);
            },

            _closeModal: function () {
                AppLayout.modalRegion.closeModal();
            },

            _editSeries: function (options) {
                var view = new EditSeriesView({ model: options.series });
                AppLayout.modalRegion.show(view);
            },

            _deleteSeries: function (options) {
                var view = new DeleteSeriesView({ model: options.series });
                AppLayout.modalRegion.show(view);
            },

            _showEpisode: function (options) {
                var view = new EpisodeDetailsLayout({ model: options.episode, hideSeriesLink: options.hideSeriesLink, openingTab: options.openingTab });
                AppLayout.modalRegion.show(view);
            },

            _showHistory: function (options) {
                var view = new HistoryDetailsView({ model: options.history });
                AppLayout.modalRegion.show(view);
            }
        });
    });
