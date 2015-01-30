'use strict';
define(
    [
        'backbone',
        'marionette',
        'System/Info/About/AboutView',
        'System/Info/DiskSpace/DiskSpaceLayout',
        'System/Info/Health/HealthLayout',
        'System/Info/MoreInfo/MoreInfoView'
    ], function (Backbone,
                 Marionette,
                 AboutView,
                 DiskSpaceLayout,
                 HealthLayout,
                 MoreInfoView) {
        return Marionette.Layout.extend({
            template: 'System/Info/SystemInfoLayoutTemplate',

            regions: {
                about     : '#about',
                diskSpace : '#diskspace',
                health    : '#health',
                moreInfo  : '#more-info'
            },

            onRender: function () {
                this.health.show(new HealthLayout());
                this.diskSpace.show(new DiskSpaceLayout());
                this.about.show(new AboutView());
                this.moreInfo.show(new MoreInfoView());
            }
        });
    });

