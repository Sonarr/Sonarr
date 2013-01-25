/// <reference path="../../app.js" />
NzbDrone.AddSeries.RootDirModel = Backbone.Model.extend({
    url: NzbDrone.Constants.ApiRoot + 'rootdir/',
});
