define(['app'], function (app) {
    NzbDrone.Calendar.CalendarModel = Backbone.Model.extend({
        mutators: {

        },
        defaults: {
            status: 0
        }
    });
});
