var Backbone = require('backbone');
var $ = require('jquery');
var StatusModel = require('../System/StatusModel');

var routeBinder = {
    bind         : function(){
        var self = this;
        $(document).on('click', 'a[href]', function(event){
            self._handleClick(event);
        });
    },
    _handleClick : function(event){
        var $target = $(event.target);
        if($target.parents('.nav-tabs').length) {
            return;
        }
        if($target.hasClass('no-router')) {
            return;
        }
        var href = event.target.getAttribute('href');
        if(!href && $target.closest('a') && $target.closest('a')[0]) {
            var linkElement = $target.closest('a')[0];
            if($(linkElement).hasClass('no-router')) {
                return;
            }
            href = linkElement.getAttribute('href');
        }
        event.preventDefault();
        if(!href) {
            throw 'couldn\'t find route target';
        }
        if(!href.startsWith('http')) {
            if(event.ctrlKey) {
                window.open(href, '_blank');
            }
            else {
                var relativeHref = href.replace(StatusModel.get('urlBase'), '');
                Backbone.history.navigate(relativeHref, {trigger : true});
            }
        }
        else if(href.contains('#')) {
            window.open(href, '_blank');
        }
        else {
            window.open('http://www.dereferer.org/?' + encodeURI(href), '_blank');
        }
    }
};

module.exports = routeBinder;
