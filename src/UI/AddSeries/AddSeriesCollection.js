'use strict';
define(
    [
        'backbone',
        'Series/SeriesModel',
        'underscore'
    ], function (Backbone, SeriesModel, _) {
        return Backbone.Collection.extend({
            url  : window.NzbDrone.ApiRoot + '/series/lookup',
            model: SeriesModel,

            parse: function (response) {

                var self = this;

                _.each(response, function (model) {
                    model.id = undefined;

                    if (self.unmappedFolderModel) {
                        model.path = self.unmappedFolderModel.get('folder').path;
                    }
                });

                return response;
            }
        });
    });
