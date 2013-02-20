define(['app', 'Upcoming/UpcomingModel'], function () {
    NzbDrone.Upcoming.UpcomingCollection = Backbone.Collection.extend({
        url: NzbDrone.Constants.ApiRoot + '/upcoming',
        model: NzbDrone.Upcoming.UpcomingModel
    });
});