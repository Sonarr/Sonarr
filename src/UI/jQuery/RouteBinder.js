'use strict';
define(['backbone'],function (Backbone) {
    //This module will automatically route all relative links through backbone router rather than
    //causing links to reload pages.

    var routeBinder = {

        bind: function () {
            var self = this;
            $(document).on('click', 'a[href]', function (event) {
                self._handleClick(event);
            });
        },

        _handleClick: function (event) {
            var $target = $(event.target);

            //check if tab nav
            if ($target.parents('.nav-tabs').length) {
                return;
            }

            if ($target.hasClass('no-router')) {
                return;
            }

            event.preventDefault();

            var href = event.target.getAttribute('href');

            if (!href && $target.parent('a') && $target.parent('a')[0]) {

                var linkElement = $target.parent('a')[0];

                href = linkElement.getAttribute('href');
            }

            if (!href) {
                throw 'couldn\'t find route target';
            }


            if (!href.startsWith('http')) {
                Backbone.history.navigate(href, { trigger: true });
            }

            else {
                //Open in new tab
                window.open(href, '_blank');
            }
        }
    };

    return routeBinder;
});
