'use strict';
define(
    [
        'jquery',
        'bootstrap'
    ], function ($) {
        $(document).ready(function () {

            var _window = $(window);
            var _scrollButton = $('#scroll-up');
            var _showFanartButton = $('#maximus-fanart');
            var _hideFanartButton = $('#minimus-fanart');

            _hideFanartButton.addClass('hidden');
            _showFanartButton.removeClass('hidden');
            $('div#page div.page-container div.container').removeClass('hidden');
            $('div#page div.footer').removeClass('hidden');

            $(window).scroll(function () {
                if (_window.scrollTop() > 100) {
                    _scrollButton.fadeIn();
                    _showFanartButton.fadeIn();
                    _hideFanartButton.fadeIn();
                }
                else {
                    _scrollButton.fadeOut();
                    _showFanartButton.fadeOut();
                    _hideFanartButton.fadeOut();
                }
            });

            _scrollButton.click(function () {
                $('html, body').animate({ scrollTop: 0 }, 600);
                return false;
            });

            _showFanartButton.click(function () {
                //$("div:not(:has(span.badclass))").addClass("addthisclass");

                $('#maximus-fanart').addClass('hidden');
                $('#minimus-fanart').removeClass('hidden');
                $('div#page div.page-container div.container').addClass('hidden');
                $('div#page div.footer').addClass('hidden');
                return false;
            });

            _hideFanartButton.click(function () {
                $('#minimus-fanart').addClass('hidden');
                $('#maximus-fanart').removeClass('hidden');
                $('div#page div.page-container div.container').removeClass('hidden');
                $('div#page div.footer').removeClass('hidden');
                return false;
            });
        });
    });
