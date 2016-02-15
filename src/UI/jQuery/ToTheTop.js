var $ = require('jquery');
var _ = require('underscore');

$(document).ready(function() {
    var _window = $(window);
    var _scrollContainer = $('#scroll-up');
    var _scrollButton = $('#scroll-up i');

    var _scrollHandler = function() {
        if (_window.scrollTop() > 400) {
            _scrollContainer.fadeIn();
        } else {
            _scrollContainer.fadeOut();
        }
    };

    $(window).scroll(_.throttle(_scrollHandler, 500));
    _scrollButton.click(function() {
        $('html, body').animate({ scrollTop : 0 }, 600);
        return false;
    });
});

