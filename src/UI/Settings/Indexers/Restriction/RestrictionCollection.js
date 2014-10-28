'use strict';
define([
    'backbone',
    'Settings/Indexers/Restriction/RestrictionModel'
], function (Backbone, RestrictionModel) {

    return Backbone.Collection.extend({
        model : RestrictionModel,
        url   : window.NzbDrone.ApiRoot + '/Restriction'
    });
});
