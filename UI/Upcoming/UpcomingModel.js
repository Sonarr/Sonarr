define(['app'], function (app) {
    NzbDrone.Upcoming.UpcomingModel = Backbone.Model.extend({
        mutators: {

        },
        defaults: {
            status: 0
        }
    });
});
