var $ = require('jquery');
var Handlebars = require('handlebars');
var StatusModel = require('../../System/StatusModel');

var placeholder = StatusModel.get('urlBase') + '/Content/Images/poster-dark.png';

window.NzbDrone.imageError = function(img) {
    if (!img.src.contains(placeholder)) {
        img.src = placeholder;
        img.srcset = "";
        $(img).addClass('placeholder-image');
    }

    img.onerror = null;
};

Handlebars.registerHelper('defaultImg', function(src, size) {
    var endOfPath = /\.jpg($|\?)/g;
    var errorAttr = 'onerror="window.NzbDrone.imageError(this);"';
    var srcsetAttr = '';
    var oneX = src, twoX;

    if (!src) {
        return new Handlebars.SafeString(errorAttr);
    }

    if (size) {
        oneX = src.replace(endOfPath, '-' + size + '.jpg$1');
        twoX = src.replace(endOfPath, '-' + size * 2 + '.jpg$1');
        srcsetAttr = 'srcset="{0} 1x, {1} 2x"'.format(oneX, twoX);
    }

    return new Handlebars.SafeString(
        'src="{0}" {1} {2}'.format(oneX, srcsetAttr, errorAttr)
    );
});

Handlebars.registerHelper('UrlBase', function() {
    return new Handlebars.SafeString(StatusModel.get('urlBase'));
});