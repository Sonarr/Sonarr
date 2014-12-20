'use strict';
define(
    [
        'jquery',
        'backbone',
        'Shared/FileBrowser/FileBrowserModel'
    ], function ($, Backbone, FileBrowserModel) {

        return Backbone.Collection.extend({
            model: FileBrowserModel,
            url  : window.NzbDrone.ApiRoot + '/filesystem',

            parse: function(response) {
                var contents = [];

                if (response.parent || response.parent === '') {

                    var type = 'parent';
                    var name = '...';

                    if (response.parent === '') {
                        type = 'computer';
                        name = 'My Computer';
                    }

                    contents.push({
                        type : type,
                        name : name,
                        path : response.parent
                    });
                }

                $.merge(contents, response.directories);
                $.merge(contents, response.files);

                return contents;
            }
        });
    });
