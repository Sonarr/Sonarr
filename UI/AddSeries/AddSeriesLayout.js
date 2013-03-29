"use strict";
define([
    'app',
    'AddSeries/RootFolders/RootFolderCollection',
    'Quality/QualityProfileCollection',
    'AddSeries/RootFolders/RootFolderView',
    'AddSeries/New/AddNewSeriesView',
    'AddSeries/Existing/ImportSeriesView'
],
    function (app, rootFolderCollection, qualityProfileCollection) {
        NzbDrone.AddSeries.AddSeriesLayout = Backbone.Marionette.Layout.extend({
            template: 'AddSeries/addSeriesLayoutTemplate',

            regions: {
                addNew        : '#add-new',
                importExisting: '#import-existing',
                rootFolders   : '#root-folders'
            },

            ui: {
                addNewTab        : '.x-add-new-tab',
                importExistingTab: '.x-import-existing-tab',
                rootFoldersTab   : '.x-root-folders-tab'
            },

            events: {
                'click .x-add-new-tab'        : 'showAddNew',
                'click .x-import-existing-tab': 'showImport',
                'click .x-root-folders-tab'   : 'showRootFolders'
            },

            showAddNew: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.addNewTab.tab('show');
                NzbDrone.Router.navigate('series/add/new');
            },

            showImport: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.importExistingTab.tab('show');
                NzbDrone.Router.navigate('series/add/import');
            },

            showRootFolders: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.rootFoldersTab.tab('show');
                NzbDrone.Router.navigate('series/add/rootfolders');
            },


            initialize: function (context, action, query) {
                if (action) {
                    this.action = action.toLowerCase();
                }

                if (query) {
                    this.query = query.toLowerCase();
                }
            },

            onRender: function () {

                rootFolderCollection.fetch();
                qualityProfileCollection.fetch();

                this.addNew.show(new NzbDrone.AddSeries.New.AddNewSeriesView());
                this.importExisting.show(new NzbDrone.AddSeries.Existing.ImportSeriesView());
                this.rootFolders.show(new NzbDrone.AddSeries.RootDirView());

                this.listenTo(rootFolderCollection, 'add', this.evaluateActions, this);
                this.listenTo(rootFolderCollection, 'remove', this.evaluateActions, this);
                this.listenTo(rootFolderCollection, 'reset', this.evaluateActions, this);
            },

            onShow: function () {
                switch (this.action) {
                    case 'import':
                        this.showImport();
                        break;
                    case 'rootfolders':
                        this.showRootFolders();
                        break;
                    default:
                        this.showAddNew();
                }
            },

            evaluateActions: function () {
                if (rootFolderCollection.length === 0) {
                    this.ui.addNewTab.hide();
                    this.ui.importExistingTab.hide();
                    this.showRootFolders();
                } else {
                    this.ui.addNewTab.show();
                    this.ui.importExistingTab.show();
                }
            }
        });
    });

