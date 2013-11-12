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
                AppLayout.mainRegion.show(new AddSeriesLayout({action: action}));
            },

            calendar: function () {
                this.setTitle('Calendar');
                AppLayout.mainRegion.show(new CalendarLayout());
            },

            settings: function (action) {
                this.setTitle('Settings');
                AppLayout.mainRegion.show(new SettingsLayout({ action: action }));
            },

            missing: function () {
                this.setTitle('Missing');

                AppLayout.mainRegion.show(new MissingLayout());
            },

            history: function (action) {
                this.setTitle('History');

                AppLayout.mainRegion.show(new HistoryLayout({ action: action }));
            },

            rss: function () {
                this.setTitle('RSS');
                AppLayout.mainRegion.show(new ReleaseLayout());
            },

            system: function (action) {
                this.setTitle('System');
                AppLayout.mainRegion.show(new SystemLayout({ action: action }));
            },

            seasonPass: function () {
                this.setTitle('Season Pass');
                AppLayout.mainRegion.show(new SeasonPassLayout());
            },

            update: function () {
                this.setTitle('Updates');
                AppLayout.mainRegion.show(new UpdateLayout());
            }
        });
    });

