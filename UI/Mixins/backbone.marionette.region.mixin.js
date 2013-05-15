"use strict";

(function () {

    var _originalRegionClose = Marionette.Region.prototype.close;

    Marionette.Region.prototype.open = function (view) {
        var self = this;

        this.$el.fadeOut(200, function () {
            _originalRegionClose.apply(this, arguments);
            self.$el.html(view.el);
            self.$el.fadeIn(150);
        });
    };

    Marionette.Region.prototype.close = function () {
        //do nothing. we close the region as part of open so we can chain the animation
    };
}());
