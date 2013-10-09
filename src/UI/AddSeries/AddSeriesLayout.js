'use strict';
define(
    [
        'vent',
        'AppLayout',
        'marionette',
        'AddSeries/RootFolders/Layout',
        'AddSeries/Existing/AddExistingSeriesCollectionView',
        'AddSeries/AddSeriesView',
        'Quality/QualityProfileCollection',
        'AddSeries/RootFolders/Collection',
        'Series/SeriesCollection'
    ], function (vent,
                 AppLayout,
                 Marionette,
                 RootFolderLayout,
                 ExistingSeriesCollectionView,
                 AddSeriesView,
                 QualityProfileCollection,
                 RootFolderCollection) {

        return Marionette.Layout.extend({
            template: 'AddSeries/AddSeriesLayoutTemplate',

            regions: {
                workspace: '#add-series-workspace'
            },

            events: {
                'click .x-import': '_importSeries',
                'click .x-add-new': '_addSeries'
            },

            attributes: {
                id: 'add-series-screen'
            },

            initialize: function () {
                QualityProfileCollection.fetch();
                RootFolderCollection.fetch();
            },

            onShow: function () {
                this.workspace.show(new AddSeriesView());
            },

            _folderSelected: function (options) {
                vent.trigger(vent.Commands.CloseModalCommand);

                this.workspace.show(new ExistingSeriesCollectionView({model: options.model}));
            },

            _importSeries: function () {
                this.rootFolderLayout = new RootFolderLayout();
                this.rootFolderLayout.on('folderSelected', this._folderSelected, this);
                AppLayout.modalRegion.show(this.rootFolderLayout);
            },

            _addSeries: function () {
                this.workspace.show(new AddSeriesView());
            }
        });
    });

