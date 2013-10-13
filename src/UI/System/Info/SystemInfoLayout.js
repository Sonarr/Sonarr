'use strict';
define(
    [
        'backbone',
        'marionette',
        'System/Info/About/AboutView',
        'System/Info/DiskSpace/DiskSpaceLayout'
    ], function (Backbone,
                 Marionette,
                 AboutView,
                 DiskSpaceLayout) {
        return Marionette.Layout.extend({
            template: 'System/Info/SystemInfoLayoutTemplate',

            regions: {
                about    : '#about',
                diskSpace: '#diskspace'
            },

            onRender: function () {
                this.about.show(new AboutView());
                this.diskSpace.show(new DiskSpaceLayout());
            }
        });
    });

