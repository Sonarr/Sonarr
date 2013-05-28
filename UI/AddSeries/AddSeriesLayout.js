"use strict";
define([
    'app',
    'Quality/QualityProfileCollection',
    'AddSeries/RootFolders/RootFolderCollection',
    'AddSeries/RootFolders/RootFolderView',
    'AddSeries/AddSeriesView',
    'AddSeries/Existing/ImportSeriesView'
],
    function (app, qualityProfileCollection, rootFolderCollection) {
        NzbDrone.AddSeries.AddSeriesLayout = Backbone.Marionette.Layout.extend({
            template: 'AddSeries/addSeriesLayoutTemplate',

            regions: {
                workspace: '#add-series-workspace'
            },

            events: {
                'click .x-import': '_importSeries'
            },

            initialize: function () {
                this.rootFolderLayout = new NzbDrone.AddSeries.RootFolders.Layout();
                this.rootFolderLayout.on('folderSelected', this._folderSelected, this);

            },

            _folderSelected: function (options) {
                NzbDrone.modalRegion.closeModal();
                this.workspace.show(new NzbDrone.AddSeries.Existing.ListView({model: options.model}));
            },

            onRender: function () {
                qualityProfileCollection.fetch();
                rootFolderCollection.fetch();

                this.workspace.show(new NzbDrone.AddSeries.AddSeriesView());
            },

            _importSeries: function () {
                NzbDrone.modalRegion.show(this.rootFolderLayout);
            }
        });
    });

