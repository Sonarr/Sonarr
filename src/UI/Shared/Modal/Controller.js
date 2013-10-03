'use strict';
define(
    [
        'app',
        'marionette',
        'Series/Edit/EditSeriesView',
        'Series/Delete/DeleteSeriesView',
        'Episode/EpisodeDetailsLayout',
        'History/Details/HistoryDetailsView'
    ], function (App, Marionette, EditSeriesView, DeleteSeriesView, EpisodeDetailsLayout, HistoryDetailsView) {

        var router = Marionette.AppRouter.extend({

            initialize: function () {
                App.vent.on(App.Commands.CloseModalCommand, this._closeModal, this);
                App.vent.on(App.Commands.EditSeriesCommand, this._editSeries, this);
                App.vent.on(App.Commands.DeleteSeriesCommand, this._deleteSeries, this);
                App.vent.on(App.Commands.ShowEpisodeDetails, this._showEpisode, this);
                App.vent.on(App.Commands.ShowHistoryDetails, this._showHistory, this);
            },

            _closeModal: function () {
                App.modalRegion.closeModal();
            },

            _editSeries: function (options) {
                var view = new EditSeriesView({ model: options.series });
                App.modalRegion.show(view);
            },

            _deleteSeries: function (options) {
                var view = new DeleteSeriesView({ model: options.series });
                App.modalRegion.show(view);
            },

            _showEpisode: function (options) {
                var view = new EpisodeDetailsLayout({ model: options.episode, hideSeriesLink: options.hideSeriesLink, openingTab: options.openingTab });
                App.modalRegion.show(view);
            },

            _showHistory: function (options) {
                var view = new HistoryDetailsView({ model: options.history });
                App.modalRegion.show(view);
            }
        });

        return new router();
    });
