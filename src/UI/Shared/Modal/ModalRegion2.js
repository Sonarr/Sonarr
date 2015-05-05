var $ = require('jquery');
var Backbone = require('backbone');
var Marionette = require('marionette');
require('bootstrap');

var region = Marionette.Region.extend({
    el : '#modal-region2',

    constructor : function() {
        Backbone.Marionette.Region.prototype.constructor.apply(this, arguments);
        this.on('show', this.showModal, this);
    },

    getEl : function(selector) {
        var $el = $(selector);
        $el.on('hidden', this.close);
        return $el;
    },

    showModal : function() {
        this.$el.addClass('modal fade');
        this.$el.attr('tabindex', '-1');
        this.$el.css('z-index', '1060');
        this.$el.modal({
            show     : true,
            keyboard : true,
            backdrop : true
        });
        this.$el.on('hide.bs.modal', $.proxy(this._closing, this));
        this.$el.on('shown.bs.modal', function() {
            $('.modal-backdrop:last').css('z-index', 1059);
        });
        this.currentView.$el.addClass('modal-dialog');
    },

    closeModal : function() {
        $(this.el).modal('hide');
        this.reset();
    },

    _closing : function() {
        if (this.$el) {
            this.$el.off('hide.bs.modal');
            this.$el.off('shown.bs.modal');
        }
        this.reset();
    }
});

module.exports = region;