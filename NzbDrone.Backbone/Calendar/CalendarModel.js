define(['app'], function (app) {
    NzbDrone.Calendar.CalendarModel = Backbone.Model.extend({
        mutators: {
            title: function () {
                return this.get('seriesTitle');
            },
            allDay: function(){
                return false;
            },
            day: function() {
                return Date.create(this.get('start')).format('{dd}');
            },
            month: function(){
                return Date.create(this.get('start')).format('{MON}');
            },
            startTime: function(){
                var start = Date.create(this.get('start'));

                if (start.format('{mm}') === '00')
                    return start.format('{h}{tt}');

                return start.format('{h}.{mm}{tt}');
            },
            paddedEpisodeNumber: function(){
                return this.get('episodeNumber');
            },
            statusLevel: function() {
                var status = this.get('status');
                var currentTime = Date.create();
                var start = Date.create(this.get('start'));
                var end = Date.create(this.get('end'));

                if (currentTime.isBetween(start, end))
                    return 'warning';

                if (start.isBefore(currentTime) || status === 'Missing')
                    return 'danger';

                if (status === 'Ready') return 'success';

                return 'primary';
            },
            bestDateString: function () {
                return bestDateString(this.get('start'));
            },
        },
        defaults: {
            status: 0
        }
    });
});
