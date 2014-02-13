'use strict';
define(
    [
        'backbone',
        'Settings/Metadata/MetadataModel'
    ], function (Backbone, MetadataModel) {

        return Backbone.Collection.extend({
            model: MetadataModel,
            url  : window.NzbDrone.ApiRoot + '/metadata'
        });
    });
