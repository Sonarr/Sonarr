/// <reference path="../../app.js" />
/// <reference path="RootDirModel.js" />

NzbDrone.AddSeries.RootDirCollection = Backbone.Collection.extend({
    url: NzbDrone.Constants.ApiRoot + '/rootdir',
    model: NzbDrone.AddSeries.RootDirModel,
});

