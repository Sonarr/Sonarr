'use strict';
define(['backbone', 'System/DiskSpace/DiskSpaceModel'],
function(Backbone, DiskSpaceModel) {
    return Backbone.Collection.extend({
        url:window.NzbDrone.ApiRoot +'/diskspace',
        model: DiskSpaceModel
    });
});