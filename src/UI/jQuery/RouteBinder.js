var Backbone = require('backbone');
var $ = require('jquery');
var StatusModel = require('../System/StatusModel');

//This module will automatically route all relative links through backbone router rather than
//causing links to reload pages.

var routeBinder = {

    bind : function() {
        var self = this;
        $(document).on('click contextmenu', 'a[href]', function(event) {
            self._handleClick(event);
        });
    },

    _handleClick : function(event) {
        var $target = $(event.target);

        //check if tab nav
        if ($target.parents('.nav-tabs').length) {
            return;
        }

        var linkElement = $target.closest('a').first();
        var href = linkElement.attr('href');

        if (href && href.startsWith('http')) {
            // Set noreferrer for external links.
            if (!linkElement.attr('rel')) {
                linkElement.attr('rel', 'noreferrer');
            }
            // Open all external links in new windows.
            if (!linkElement.attr('target')) {
                linkElement.attr('target', '_blank');
            }
        }

        if (linkElement.hasClass('no-router') || event.type !== 'click') {
            return;
        }

        if (!href) {
            throw 'couldn\'t find route target';
        }

        if (!href.startsWith('http')) {
            event.preventDefault();

            if (event.ctrlKey) {
                window.open(href, '_blank');
            }

            else {
                var relativeHref = href.replace(StatusModel.get('urlBase'), '');

                Backbone.history.navigate(relativeHref, { trigger : true });
            }
        }
    }
};

module.exports = routeBinder;