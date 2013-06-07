"use strict";
define(['app'], function () {

    //This module will automatically route all links through backbone router rather than
    //causing links to reload pages.

    var routeBinder = {

        bind: function () {
            $(document).on('click', 'a[href]', this._handleClick);
        },

        _isInTab: function (element) {
            return;
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

            console.log('click');
            event.preventDefault();

            var href = event.target.getAttribute('href');

            if (!href && $target.parent('a') && $target.parent('a')[0]) {

                var linkElement = $target.parent('a')[0];

                href = linkElement.getAttribute('href');
            }

            if (!href) {
                throw 'couldnt find route target';
            }


            if (!href.startsWith('http')) {
                NzbDrone.Router.navigate(href, { trigger: true });
            }
        }
    };

    return routeBinder;
});
