'use strict';
define(
    [
        'vent',
        'AppLayout',
        'marionette',
        'AddSeries/RootFolders/RootFolderLayout',
        'AddSeries/Existing/AddExistingSeriesCollectionView',
        'AddSeries/AddSeriesView',
        'Profile/ProfileCollection',
        'AddSeries/RootFolders/RootFolderCollection',
        'Series/SeriesCollection'
    ], function (vent,
                 AppLayout,
                 Marionette,
                 RootFolderLayout,
                 ExistingSeriesCollectionView,
                 AddSeriesView,
                 ProfileCollection,
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
                ProfileCollection.fetch();
                RootFolderCollection.fetch()
                    .done(function () {
                        RootFolderCollection.synced = true;
                    });
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
                this.listenTo(this.rootFolderLayout, 'folderSelected', this._folderSelected);
                AppLayout.modalRegion.show(this.rootFolderLayout);
            },

            _addSeries: function () {
                this.workspace.show(new AddSeriesView());
            }
        });
    });

