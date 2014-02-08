'use strict';

define(
    [
        'jquery',
        'handlebars',
        'System/StatusModel'
    ], function ($, Handlebars, StatusModel) {

        var placeholder = StatusModel.get('urlBase') + '/Content/Images/poster-dark.jpg';

        window.NzbDrone.imageError = function (img) {
            if (!img.src.contains(placeholder)) {
                img.src = placeholder;
                $(img).addClass('placeholder-image');
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
