"use strict";
define(
    [
        'app',
        'marionette',
        'AddSeries/RootFolders/Layout',
        'AddSeries/Existing/CollectionView',
        'AddSeries/AddSeriesView',
        'Quality/QualityProfileCollection',
        'AddSeries/RootFolders/Collection'
    ], function (App, Marionette, RootFolderLayout, ExistingSeriesCollectionView, AddSeriesView, qualityProfileCollection, rootFolderCollection) {

        return Marionette.Layout.extend({
            template: 'AddSeries/addSeriesLayoutTemplate',

            regions: {
                workspace: '#add-series-workspace'
            },

            events: {
                'click .x-import': '_importSeries'
            },

            initialize: function () {
                this.rootFolderLayout = new RootFolderLayout();
                this.rootFolderLayout.on('folderSelected', this._folderSelected, this);

                qualityProfileCollection.fetch();
                rootFolderCollection.fetch();
            },

            onShow: function () {
                this.workspace.show(new AddSeriesView());
            },

            _folderSelected: function (options) {
                App.modalRegion.closeModal();
                this.workspace.show(new ExistingSeriesCollectionView({model: options.model}));
            },

            _importSeries: function () {
                App.modalRegion.show(this.rootFolderLayout);
            }
        });
    });

