'use strict';

define(
    [
        'handlebars',
        'System/StatusModel'
    ], function (Handlebars, StatusModel) {

        var placeHolder = StatusModel.get('urlBase') + '/Content/Images/poster-dark.jpg';

        window.NzbDrone.imageError = function (img) {
            if (!img.src.contains(placeHolder)) {
                img.src = placeHolder;
            }
            img.onerror = null;
        };

        Handlebars.registerHelper('defaultImg', function () {
            return new Handlebars.SafeString('onerror=window.NzbDrone.imageError(this)');
        });

        Handlebars.registerHelper('UrlBase', function () {
            return new Handlebars.SafeString(StatusModel.get('urlBase'));
        });
    });
