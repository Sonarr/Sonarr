'use strict';
define(
    [
        'Shared/NzbDroneController',
        'AppLayout',
        'marionette',
        'History/HistoryLayout',
        'Settings/SettingsLayout',
        'AddSeries/AddSeriesLayout',
        'Missing/MissingLayout',
        'Calendar/CalendarLayout',
        'Release/ReleaseLayout',
        'System/SystemLayout',
        'SeasonPass/SeasonPassLayout',
        'System/Update/UpdateLayout'
    ], function (NzbDroneController,
                 AppLayout,
                 Marionette,
                 HistoryLayout,
                 SettingsLayout,
                 AddSeriesLayout,
                 MissingLayout,
                 CalendarLayout,
                 ReleaseLayout,
                 SystemLayout,
                 SeasonPassLayout,
                 UpdateLayout) {
        return NzbDroneController.extend({

            addSeries: function (action) {
                this.setTitle('Add Series');
                this.showMainRegion(new AddSeriesLayout({action: action}));
            },

            calendar: function () {
                this.setTitle('Calendar');
                this.showMainRegion(new CalendarLayout());
            },

            settings: function (action) {
                this.setTitle('Settings');
                this.showMainRegion(new SettingsLayout({ action: action }));
            },

            missing: function () {
                this.setTitle('Missing');

                this.showMainRegion(new MissingLayout());
            },

            history: function (action) {
                this.setTitle('History');

                this.showMainRegion(new HistoryLayout({ action: action }));
            },

            rss: function () {
                this.setTitle('RSS');
                this.showMainRegion(new ReleaseLayout());
            },

            system: function (action) {
                this.setTitle('System');
                this.showMainRegion(new SystemLayout({ action: action }));
            },

            seasonPass: function () {
                this.setTitle('Season Pass');
                this.showMainRegion(new SeasonPassLayout());
            },

            update: function () {
                this.setTitle('Updates');
                this.showMainRegion(new UpdateLayout());
            }
        });
    });

