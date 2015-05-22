var _ = require('underscore');
var vent = require('vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var FileBrowserCollection = require('./FileBrowserCollection');
var EmptyView = require('./EmptyView');
var FileBrowserRow = require('./FileBrowserRow');
var FileBrowserTypeCell = require('./FileBrowserTypeCell');
var FileBrowserNameCell = require('./FileBrowserNameCell');
var RelativeDateCell = require('../../Cells/RelativeDateCell');
var FileSizeCell = require('../../Cells/FileSizeCell');
var LoadingView = require('../LoadingView');
require('../../Mixins/DirectoryAutoComplete');

module.exports = Marionette.Layout.extend({
    template : 'Shared/FileBrowser/FileBrowserLayoutTemplate',

    regions : {
        browser : '#x-browser'
    },

    ui : {
        path      : '.x-path',
        indicator : '.x-indicator'
    },

    events : {
        'typeahead:selected .x-path'      : '_pathChanged',
        'typeahead:autocompleted .x-path' : '_pathChanged',
        'keyup .x-path'                   : '_inputChanged',
        'click .x-ok'                     : '_selectPath'
    },

    initialize : function(options) {
        this.collection = new FileBrowserCollection();
        this.collection.showFiles = options.showFiles || false;
        this.collection.showLastModified = options.showLastModified || false;
        this.input = options.input;
        this._setColumns();
        this.listenTo(this.collection, 'sync', this._showGrid);
        this.listenTo(this.collection, 'filebrowser:row:folderselected', this._rowSelected);
        this.listenTo(this.collection, 'filebrowser:row:fileselected', this._fileSelected);
    },

    onRender : function() {
        this.browser.show(new LoadingView());
        this.ui.path.directoryAutoComplete();
        this._fetchCollection(this.input.val());
        this._updatePath(this.input.val());
    },

    _setColumns : function() {
        this.columns = [
            {
                name     : 'type',
                label    : '',
                sortable : false,
                cell     : FileBrowserTypeCell
            },
            {
                name     : 'name',
                label    : 'Name',
                sortable : false,
                cell     : FileBrowserNameCell
            }
        ];
        if (this.collection.showLastModified) {
            this.columns.push({
                name     : 'lastModified',
                label    : 'Last Modified',
                sortable : false,
                cell     : RelativeDateCell
            });
        }
        if (this.collection.showFiles) {
            this.columns.push({
                name     : 'size',
                label    : 'Size',
                sortable : false,
                cell     : FileSizeCell
            });
        }
    },

    _fetchCollection : function(path) {
        this.ui.indicator.show();
        var data = { includeFiles : this.collection.showFiles };
        if (path) {
            data.path = path;
        }
        this.collection.fetch({ data : data });
    },

    _showGrid : function() {
        this.ui.indicator.hide();
        if (this.collection.models.length === 0) {
            this.browser.show(new EmptyView());
            return;
        }
        var grid = new Backgrid.Grid({
            row        : FileBrowserRow,
            collection : this.collection,
            columns    : this.columns,
            className  : 'table table-hover'
        });
        this.browser.show(grid);
    },

    _rowSelected : function(model) {
        var path = model.get('path');

        this._updatePath(path);
        this._fetchCollection(path);
    },

    _fileSelected : function(model) {
        var path = model.get('path');
        var type = model.get('type');

        this.input.val(path);
        this.input.trigger('change');

        this.input.trigger('filebrowser:fileselected', {
            type : type,
            path : path
        });

        vent.trigger(vent.Commands.CloseFileBrowser);
    },

    _pathChanged : function(e, path) {
        this._fetchCollection(path.value);
        this._updatePath(path.value);
    },

    _inputChanged : function() {
        var path = this.ui.path.val();
        if (path === '' || path.endsWith('\\') || path.endsWith('/')) {
            this._fetchCollection(path);
        }
    },

    _updatePath : function(path) {
        if (path !== undefined || path !== null) {
            this.ui.path.val(path);
        }
    },

    _selectPath : function() {
        var path = this.ui.path.val();

        this.input.val(path);
        this.input.trigger('change');

        this.input.trigger('filebrowser:folderselected', {
            type: 'folder',
            path: path
        });

        vent.trigger(vent.Commands.CloseFileBrowser);
    }
});
