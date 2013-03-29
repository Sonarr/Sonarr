define(['app', 'Calendar/CalendarModel'], function () {
    NzbDrone.Calendar.CalendarCollection = Backbone.Collection.extend({
        url       : NzbDrone.Constants.ApiRoot + '/calendar',
        model     : NzbDrone.Calendar.CalendarModel,
        comparator: function (model) {
            return model.get('start');
        }
    });
});