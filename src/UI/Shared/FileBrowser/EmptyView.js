'use strict';

define(
    [
        'marionette'
    ], function (Marionette) {

        return Marionette.CompositeView.extend({
            template: 'Shared/FileBrowser/EmptyViewTemplate'
        });
    });
