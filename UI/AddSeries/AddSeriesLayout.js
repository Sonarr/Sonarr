"use strict";
define([
    'app',
    'AddSeries/RootFolders/RootFolderCollection',
    'Quality/QualityProfileCollection',
    'AddSeries/RootFolders/RootFolderView',
    'AddSeries/AddSeriesView',
    'AddSeries/Existing/ImportSeriesView'
],
    function (app, rootFolderCollection, qualityProfileCollection) {
        NzbDrone.AddSeries.AddSeriesLayout = Backbone.Marionette.Layout.extend({
            template: 'AddSeries/addSeriesLayoutTemplate',

            regions: {
                workspace: '#add-series-workspace'
            },

            events: {
                'click .x-import': '_importSeries'
            },

            onRender: function () {

                /*         rootFolderCollection.fetch({success: function () {
                 self.importExisting.show(new NzbDrone.AddSeries.Existing.RootDirListView({model: rootFolderCollection.at(0)}));
                 }});*/
                qualityProfileCollection.fetch();
                rootFolderCollection.fetch();

                this.workspace.show(new NzbDrone.AddSeries.AddSeriesView());
            },


            _importSeries: function () {
                NzbDrone.modalRegion.show(new NzbDrone.AddSeries.RootFolders.Layout());
            }
        });
    });

