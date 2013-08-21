'use strict';
define(
    [
        'app',
        'marionette',
        'Series/Edit/EditSeriesView',
        'Series/Delete/DeleteSeriesView',
        'Episode/Layout'

    ], function (App, Marionette, EditSeriesView, DeleteSeriesView, EpisodeLayout) {

        var router = Marionette.AppRouter.extend({

            initialize: function () {
                App.vent.on(App.Commands.CloseModalCommand, this._closeModal, this);
                App.vent.on(App.Commands.EditSeriesCommand, this._editSeries, this);
                App.vent.on(App.Commands.DeleteSeriesCommand, this._deleteSeries, this);
                App.vent.on(App.Commands.ShowEpisodeDetails, this._showEpisode, this);
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
                var view = new EpisodeLayout({ model: options.episode, hideSeriesLink: options.hideSeriesLink });
                App.modalRegion.show(view);
            }
        });

        return new router();
    });
