var $ = require('jquery');
var _ = require('underscore');

$(document).ready(function() {
    var _window = $(window);
    var _scrollButton = $('#scroll-up');

    var _scrollHandler = function() {
        if (_window.scrollTop() > 100) {
            _scrollButton.fadeIn();
        } else {
            _scrollButton.fadeOut();
        }
    };

    $(window).scroll(_.throttle(_scrollHandler, 500));
    _scrollButton.click(function() {
        $('html, body').animate({ scrollTop : 0 }, 600);
        return false;
    });
});

