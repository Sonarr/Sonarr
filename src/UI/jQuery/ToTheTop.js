'use strict';
define(
    [
        'jquery',
        'bootstrap'
    ], function ($) {
        $(document).ready(function () {

            var _window = $(window);
            var _scrollButton = $('#scroll-up');

            $(window).scroll(function () {
                if (_window.scrollTop() > 100) {
                    _scrollButton.fadeIn();
                }
                else {
                    _scrollButton.fadeOut();
                }
            });

            _scrollButton.click(function () {
                $('html, body').animate({ scrollTop: 0 }, 600);
                return false;
            });
        });
    });
