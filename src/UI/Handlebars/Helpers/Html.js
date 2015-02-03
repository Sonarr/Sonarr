var $ = require('jquery');
var Handlebars = require('handlebars');
var StatusModel = require('../../System/StatusModel');

module.exports = (function(){
    var placeholder = StatusModel.get('urlBase') + '/Content/Images/poster-dark.png';
    window.NzbDrone.imageError = function(img){
        if(!img.src.contains(placeholder)) {
            img.src = placeholder;
            $(img).addClass('placeholder-image');
        }
        img.onerror = null;
    };
    Handlebars.registerHelper('defaultImg', function(src, size){
        if(!src) {
            return new Handlebars.SafeString('onerror="window.NzbDrone.imageError(this);"');
        }
        if(size) {
            src = src.replace(/\.jpg($|\?)/g, '-' + size + '.jpg$1');
        }
        return new Handlebars.SafeString('src="{0}" onerror="window.NzbDrone.imageError(this);"'.format(src));
    });
    Handlebars.registerHelper('UrlBase', function(){
        return new Handlebars.SafeString(StatusModel.get('urlBase'));
    });
}).call(this);