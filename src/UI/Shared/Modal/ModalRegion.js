'use strict';
define(
    [
        'jquery',
        'backbone',
        'marionette',
        'bootstrap'
    ], function ($,Backbone, Marionette) {
        var region = Marionette.Region.extend({
            el: '#modal-region',

            constructor: function () {
                Backbone.Marionette.Region.prototype.constructor.apply(this, arguments);
                this.on('show', this.showModal, this);
            },

            getEl: function (selector) {
                var $el = $(selector);
                $el.on('hidden', this.close);
                return $el;
            },

            showModal: function () {
                this.$el.addClass('modal hide fade');

                //need tab index so close on escape works
                //https://github.com/twitter/bootstrap/issues/4663
                this.$el.attr('tabindex', '-1');
                this.$el.modal({
                    'show'    : true,
                    'keyboard': true,
                    'backdrop': 'static'});
            },

            closeModal: function () {
                $(this.el).modal('hide');
                this.reset();
            }

        });

        return region;
    });
