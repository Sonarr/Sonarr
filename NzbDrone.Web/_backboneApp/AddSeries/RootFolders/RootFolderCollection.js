define(['app', 'AddSeries/RootFolders/RootFolderModel'], function () {

    NzbDrone.AddSeries.RootFolders.RootFolderCollection = Backbone.Collection.extend({
        url: NzbDrone.Constants.ApiRoot + '/rootdir',
        model: NzbDrone.AddSeries.RootFolders.RootFolderModel,
    });

});


