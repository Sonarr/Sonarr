define(['app', 'Missing/MissingModel'], function () {
    NzbDrone.Missing.MissingCollection = Backbone.Collection.extend({
        url: NzbDrone.Constants.ApiRoot + '/missing',
        model: NzbDrone.Missing.MissingModel,
        comparator: function(model) {
            return model.get('airDate');
        }
    });
});