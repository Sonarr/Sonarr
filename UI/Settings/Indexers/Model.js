"use strict";
define(['app'], function (app) {
    NzbDrone.Settings.Indexers.Model = Backbone.DeepModel.extend({
        mutators: {
            fields: function () {
                return [
                    { key: 'username', title: 'Username', helpText: 'HALP!', value: 'mark', index: 0 },
                    { key: 'apiKey', title: 'API Key', helpText: 'HALP!', value: '', index: 1 }
                ];
            }
        }
    });
});
