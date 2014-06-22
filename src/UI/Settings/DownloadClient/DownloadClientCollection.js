'use strict';

define([
    'backbone',
    'Settings/DownloadClient/DownloadClientModel'
], function (Backbone, DownloadClientModel) {

    return Backbone.Collection.extend({
        model: DownloadClientModel,
        url  : window.NzbDrone.ApiRoot + '/downloadclient',

        comparator : function(left, right, collection) {

           var result = 0;

           if (left.get('protocol')) {
              result = -left.get('protocol').localeCompare(right.get('protocol'));
           }

           if (result === 0 && left.get('name')) {
              result = left.get('name').localeCompare(right.get('name'));
           }

           if (result === 0) {
              result = left.get('implementation').localeCompare(right.get('implementation'));
           }

           return result;
        }
    });
});
