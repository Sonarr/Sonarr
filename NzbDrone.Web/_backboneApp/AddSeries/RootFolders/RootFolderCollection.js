define(['app', 'AddSeries/RootFolders/RootFolderModel'], function () {

    var rootFolderCollection = Backbone.Collection.extend({
        url: NzbDrone.Constants.ApiRoot + '/rootdir',
        model: NzbDrone.AddSeries.RootFolders.RootFolderModel
    });

    return new rootFolderCollection();
});


