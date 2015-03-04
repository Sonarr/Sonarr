var _ = require('underscore');
var vent = require('vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var CommandController = require('../Commands/CommandController');
var EmptyView = require('./EmptyView');
var SelectFolderView = require('./Folder/SelectFolderView');
var LoadingView = require('../Shared/LoadingView');
var ManualImportRow = require('./ManualImportRow');
var SelectAllCell = require('../Cells/SelectAllCell');
var PathCell = require('./Cells/PathCell');
var SeriesCell = require('./Cells/SeriesCell');
var SeasonCell = require('./Cells/SeasonCell');
var EpisodesCell = require('./Cells/EpisodesCell');
var QualityCell = require('./Cells/QualityCell');
var FileSizeCell = require('../Cells/FileSizeCell');
var ApprovalStatusCell = require('../Cells/ApprovalStatusCell');
var ManualImportCollection = require('./ManualImportCollection');

module.exports = Marionette.Layout.extend({
    className : 'modal-lg',
    template  : 'ManualImport/ManualImportLayoutTemplate',

    regions : {
        workspace  : '.x-workspace'
    },

    ui : {
        importButton : '.x-import'
    },

    events : {
        'click .x-import' : '_import'
    },

    columns : [
        {
            name       : '',
            cell       : SelectAllCell,
            headerCell : 'select-all',
            sortable   : false
        },
        {
            name       : 'relativePath',
            label      : 'Relative Path',
            cell       : PathCell,
            sortable   : true
        },
        {
            name       : 'series',
            label      : 'Series',
            cell       : SeriesCell,
            sortable   : true
        },
        {
            name       : 'seasonNumber',
            label      : 'Season',
            cell       : SeasonCell,
            sortable   : true
        },
        {
            name       : 'episodes',
            label      : 'Episode(s)',
            cell       : EpisodesCell,
            sortable   : false
        },
        {
            name       : 'quality',
            label      : 'Quality',
            cell       : QualityCell,
            sortable   : true

        },
        {
            name       : 'size',
            label      : 'Size',
            cell       : FileSizeCell,
            sortable   : true
        },
        {
            name       : 'rejections',
            label      : '<i class="icon-sonarr-header-rejections" />',
            tooltip    : 'Rejections',
            cell       : ApprovalStatusCell,
            sortable   : false,
            sortType   : 'fixed',
            direction  : 'ascending'
        }
    ],

    initialize : function(options) {
        this.folder = options.folder;
        this.downloadId = options.downloadId;
        this.title = options.title;

        //TODO: remove (just for testing)
        this.folder = 'C:\\Test';
//        this.folder = 'E:\\X-Server';

        this.templateHelpers = {
            title : this.title || this.folder
        };
    },

    onRender : function() {

        if (this.folder || this.downloadId) {
            this._showLoading();
            this._loadCollection();
        }

        else {
            this._showSelectFolder();
            this.ui.importButton.hide();
        }
    },

    _showLoading : function () {
        this.workspace.show(new LoadingView());
    },

    _loadCollection : function () {
        this.manualImportCollection = new ManualImportCollection({ folder: this.folder, downloadId: this.downloadId });
        this.manualImportCollection.fetch();

        this.listenTo(this.manualImportCollection, 'sync', this._showTable);
        this.listenTo(this.manualImportCollection, 'backgrid:selected', this._updateButtons);
    },

    _showTable : function () {
        if (this.manualImportCollection.length === 0) {
            this.workspace.show(new EmptyView());
            return;
        }

        this.fileView = new Backgrid.Grid({
            columns    : this.columns,
            collection : this.manualImportCollection,
            className  : 'table table-hover',
            row        : ManualImportRow
        });

        this.workspace.show(this.fileView);
        this._updateButtons();
    },

    _showSelectFolder : function () {
        this.selectFolderView = new SelectFolderView();
        this.workspace.show(this.selectFolderView);

        this.listenTo(this.selectFolderView, 'manualImport', this._manualImport);
        this.listenTo(this.selectFolderView, 'automaticImport', this._automaticImport);
    },

    _manualImport : function (e) {
        this.folder = e.folder;
        this.templateHelpers.title = this.folder;
        this.render();
    },

    _automaticImport : function (e) {
        CommandController.Execute('downloadedEpisodesScan', {
            name : 'downloadedEpisodesScan',
            path : e.folder
        });

        vent.trigger(vent.Commands.CloseModalCommand);
    },

    _import : function () {
        var selected = this.fileView.getSelectedModels();

        if (selected.length === 0) {
            return;
        }

        CommandController.Execute('manualImport', {
            name  : 'manualImport',
            files : _.map(selected, function (file) {
                return {
                    path       : file.get('path'),
                    seriesId   : file.get('series').id,
                    episodeIds : _.map(file.get('episodes'), 'id'),
                    quality    : file.get('quality'),
                    downloadId : file.get('downloadId')
                };
            })
        });

        vent.trigger(vent.Commands.CloseModalCommand);
    },

    _updateButtons : function (model, selected) {
        if (!this.fileView) {
            this.ui.importButton.attr('disabled', 'disabled');
            return;
        }

        if (!model) {
            return;
        }

        var selectedModels = this.fileView.getSelectedModels();
        var selectedCount = 0;

        if (selected) {
            selectedCount = _.any(selectedModels, { id : model.id }) ? selectedModels.length : selectedModels.length + 1;
        }

        else {
            selectedCount = _.any(selectedModels, { id : model.id }) ? selectedModels.length - 1 : selectedModels.length;
        }

        if (selectedCount === 0) {
            this.ui.importButton.attr('disabled', 'disabled');
        }

        else {
            this.ui.importButton.removeAttr('disabled');
        }
    }
});