'use strict';

define(
    [
        'handlebars'
    ], function (Handlebars) {
        Handlebars.registerHelper('defaultImg', function () {
            return new Handlebars.SafeString('onerror=this.src=\'/Content/Images/poster-dark.jpg\';');
        });
    });
