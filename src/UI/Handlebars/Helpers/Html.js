var $ = require('jquery');
var Handlebars = require('handlebars');
var StatusModel = require('../../System/StatusModel');

var placeholder = StatusModel.get('urlBase') + '/Content/Images/poster-dark.png';

window.NzbDrone.imageError = function(img) {
    if (!img.src.contains(placeholder)) {
        img.src = placeholder;
        $(img).addClass('placeholder-image');
    }

    img.onerror = null;
};

Handlebars.registerHelper('defaultImg', function(src, size) {
    var endOfPath = /\.jpg($|\?)/g, 
        oneX = src, twoX, srcset;
    
    if (!src) {
        return new Handlebars.SafeString('onerror="window.NzbDrone.imageError(this);"');
    }

    if (size) {
        oneX = src.replace(endOfPath, '-' + size + '.jpg$1');
        twoX = src.replace(endOfPath, '-' + size * 2 + '.jpg$1');
        srcset = 'srcset="{0} 1x, {1} 2x"'.format(oneX, twoX);
    }

    return new Handlebars.SafeString(
        'src="{0}" {1} onerror="window.NzbDrone.imageError(this);"'.format(oneX, srcset)
    );
});

Handlebars.registerHelper('UrlBase', function() {
    return new Handlebars.SafeString(StatusModel.get('urlBase'));
});