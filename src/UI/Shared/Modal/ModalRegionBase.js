var _ = require('underscore');
var $ = require('jquery');
var Backbone = require('backbone');
var Marionette = require('marionette');
require('bootstrap');
var region = Marionette.Region.extend({
    el : '#modal-region',

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
        this.trigger('modal:beforeShow');
        this.$el.addClass('modal fade');

        //need tab index so close on escape works
        //https://github.com/twitter/bootstrap/issues/4663
        this.$el.attr('tabindex', '-1');
        this.$el.modal({
            show     : true,
            keyboard : true,
            backdrop : true
        });

        this.$el.on('hide.bs.modal', $.proxy(this._closing, this));
        this.$el.on('hidden.bs.modal', $.proxy(this._closed, this));

        this.currentView.$el.addClass('modal-dialog');

        this.$el.on('shown.bs.modal', _.bind(function() {
            this.trigger('modal:afterShow');
            this.currentView.trigger('modal:afterShow');
        }, this));
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
    },

    _closed: function () {
        if (this.$el) {
            this.$el.off('hidden.bs.modal');
        }
    }
});

module.exports = region;