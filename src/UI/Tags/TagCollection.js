'use strict';
define(
    [
        'backbone',
        'Tags/TagModel',
        'api!tag'
    ], function (Backbone, TagModel, TagData) {
        var Collection = Backbone.Collection.extend({
            url  : window.NzbDrone.ApiRoot + '/tag',
            model: TagModel
        });

        return new Collection(TagData);
    });
