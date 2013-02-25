define(['app'], function (app) {
    NzbDrone.Calendar.CalendarModel = Backbone.Model.extend({
        mutators: {
            title: function () {
                return this.get('seriesTitle') + ' - ' + this.get('seasonNumber') + 'x' + this.get('episodeNumber').pad(2);
            },
            allDay: function(){
                return false;
            }
        },
        defaults: {
            status: 0
        }
    });
});
