var Backbone = require('backbone');
var Marionette = require('marionette');
var AboutView = require('./About/AboutView');
var DiskSpaceLayout = require('./DiskSpace/DiskSpaceLayout');
var HealthLayout = require('./Health/HealthLayout');
var MoreInfoView = require('./MoreInfo/MoreInfoView');

module.exports = Marionette.Layout.extend({
    template : 'System/Info/SystemInfoLayoutTemplate',
    regions  : {
        about     : '#about',
        diskSpace : '#diskspace',
        health    : '#health',
        moreInfo  : '#more-info'
    },
    onRender : function(){
        this.health.show(new HealthLayout());
        this.diskSpace.show(new DiskSpaceLayout());
        this.about.show(new AboutView());
        this.moreInfo.show(new MoreInfoView());
    }
});