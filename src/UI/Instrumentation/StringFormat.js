'use strict';

String.prototype.format = function() {
    var args = arguments;

    return this.replace(/{(\d+)}/g, function(match, number) {
        if (typeof args[number] !== 'undefined') {
            return args[number];
        } else {
            return match;
        }
    });
};