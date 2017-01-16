var vent = require('vent');
var AppLayout = require('../../AppLayout');
var Marionette = require('marionette');
var EditSeriesView = require('../../Series/Edit/EditSeriesView');
var DeleteSeriesView = require('../../Series/Delete/DeleteSeriesView');
var EpisodeDetailsLayout = require('../../Episode/EpisodeDetailsLayout');
var HistoryDetailsLayout = require('../../Activity/History/Details/HistoryDetailsLayout');
var LogDetailsView = require('../../System/Logs/Table/Details/LogDetailsView');
var RenamePreviewLayout = require('../../Rename/RenamePreviewLayout');
var ManualImportLayout = require('../../ManualImport/ManualImportLayout');
var FileBrowserLayout = require('../FileBrowser/FileBrowserLayout');

module.exports = Marionette.AppRouter.extend({
    initialize : function() {
        vent.on(vent.Commands.OpenModalCommand, this._openModal, this);
        vent.on(vent.Commands.CloseModalCommand, this._closeModal, this);
        vent.on(vent.Commands.OpenModal2Command, this._openModal2, this);
        vent.on(vent.Commands.CloseModal2Command, this._closeModal2, this);
        vent.on(vent.Commands.EditSeriesCommand, this._editSeries, this);
        vent.on(vent.Commands.DeleteSeriesCommand, this._deleteSeries, this);
        vent.on(vent.Commands.ShowEpisodeDetails, this._showEpisode, this);
        vent.on(vent.Commands.ShowHistoryDetails, this._showHistory, this);
        vent.on(vent.Commands.ShowLogDetails, this._showLogDetails, this);
        vent.on(vent.Commands.ShowRenamePreview, this._showRenamePreview, this);
        vent.on(vent.Commands.ShowManualImport, this._showManualImport, this);
        vent.on(vent.Commands.ShowFileBrowser, this._showFileBrowser, this);
        vent.on(vent.Commands.CloseFileBrowser, this._closeFileBrowser, this);
    },

    _openModal : function(view) {
        AppLayout.modalRegion.show(view);
    },

    _closeModal : function() {
        AppLayout.modalRegion.closeModal();
    },

    _openModal2 : function(view) {
        AppLayout.modalRegion2.show(view);
    },

    _closeModal2 : function() {
        AppLayout.modalRegion2.closeModal();
    },

    _editSeries : function(options) {
        var view = new EditSeriesView({ model : options.series });
        AppLayout.modalRegion.show(view);
    },

    _deleteSeries : function(options) {
        var view = new DeleteSeriesView({ model : options.series });
        AppLayout.modalRegion.show(view);
    },

    _showEpisode : function(options) {
        var view = new EpisodeDetailsLayout({
            model          : options.episode,
            hideSeriesLink : options.hideSeriesLink,
            openingTab     : options.openingTab
        });
        AppLayout.modalRegion.show(view);
    },

    _showHistory : function(options) {
        var view = new HistoryDetailsLayout({ model : options.model });
        AppLayout.modalRegion.show(view);
    },

    _showLogDetails : function(options) {
        var view = new LogDetailsView({ model : options.model });
        AppLayout.modalRegion.show(view);
    },

    _showRenamePreview : function(options) {
        var view = new RenamePreviewLayout(options);
        AppLayout.modalRegion.show(view);
    },

    _showManualImport : function(options) {
        var view = new ManualImportLayout(options);
        AppLayout.modalRegion.show(view);
    },

    _showFileBrowser : function(options) {
        var view = new FileBrowserLayout(options);
        AppLayout.modalRegion2.show(view);
    },

    _closeFileBrowser : function() {
        AppLayout.modalRegion2.closeModal();
    }
});