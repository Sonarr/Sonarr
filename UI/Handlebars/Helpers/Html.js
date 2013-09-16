'use strict';

define(
    [
        'handlebars'
    ], function (Handlebars) {

        var placeHolder = '/Content/Images/poster-dark.jpg';

        window.NzbDrone.imageError = function (img) {
            if (!img.src.contains(placeHolder)) {
                img.src = placeHolder;
            }
            img.onerror = null;
        };

        Handlebars.registerHelper('defaultImg', function () {
            return new Handlebars.SafeString('onerror=window.NzbDrone.imageError(this)');
        });
    });
