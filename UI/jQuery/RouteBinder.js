'use strict';
define(function () {
    //This module will automatically route all relative links through backbone router rather than
    //causing links to reload pages.

    var routeBinder = {

        bind: function (router) {
            var self = this;
            $(document).on('click', 'a[href]', function (event) {
                self._handleClick(event, router);
            });
        },

        _handleClick: function (event, router) {
            var $target = $(event.target);

            //check if tab nav
            if ($target.parents('.nav-tabs').length) {
                return;
            }

            if ($target.hasClass('no-router')) {
                return;
            }

            console.log('click');
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
                router.navigate(href, { trigger: true });
            }

            else {
                //Open in new tab
                window.open(href, '_blank');
            }
        }
    };

    return routeBinder;
});
