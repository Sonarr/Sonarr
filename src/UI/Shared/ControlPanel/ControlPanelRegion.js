var $ = require('jquery');
var Backbone = require('backbone');
var Marionette = require('marionette');
var region = Marionette.Region.extend({
    el : '#control-panel-region',

    constructor : function() {
        Backbone.Marionette.Region.prototype.constructor.apply(this, arguments);
        this.on('show', this.showPanel, this);
    },

    getEl : function(selector) {
        var $el = $(selector);

        return $el;
    },

    showPanel : function() {
        $('body').addClass('control-panel-visible');
        this.$el.animate({
            'margin-bottom' : 0,
            'opacity'       : 1
        }, {
            queue    : false,
            duration : 300
        });
    },

    closePanel : function() {
        $('body').removeClass('control-panel-visible');
        this.$el.animate({
            'margin-bottom' : -100,
            'opacity'       : 0
        }, {
            queue    : false,
            duration : 300
        });
        this.reset();
    }
});
module.exports = region;