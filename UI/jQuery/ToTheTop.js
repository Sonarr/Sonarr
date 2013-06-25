'use strict';
define(
    [
        'bootstrap'
    ], function () {
        $(document).ready(function () {

            $(window).scroll(function () {
                if ($(this).scrollTop() > 100) {
                    $('#scroll-up').fadeIn();
                }
                else {
                    $('#scroll-up').fadeOut();
                }
            });

            $('#scroll-up').click(function () {
                $("html, body").animate({ scrollTop: 0 }, 600);
                return false;
            });

        });
    });
