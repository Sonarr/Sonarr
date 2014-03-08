'use strict';
define(
    [
        'backbone',
        'marionette',
        'System/Info/About/AboutView',
        'System/Info/DiskSpace/DiskSpaceLayout',
        'System/Info/Health/HealthLayout'
    ], function (Backbone,
                 Marionette,
                 AboutView,
                 DiskSpaceLayout,
                 HealthLayout) {
        return Marionette.Layout.extend({
            template: 'System/Info/SystemInfoLayoutTemplate',

            regions: {
                about    : '#about',
                diskSpace: '#diskspace',
                health   : '#health'
            },

            onRender: function () {
                this.about.show(new AboutView());
                this.diskSpace.show(new DiskSpaceLayout());
                this.health.show(new HealthLayout());
            }
        });
    });

