'use strict';

define(
    [
        'backgrid'
    ], function (Backgrid) {
        return Backgrid.Cell.extend({

            className: 'download-report-cell',

            events: {
                'click': '_onClick'
            },

            _onClick: function () {

                var self = this;

                this.$el.html('<i class =\'icon-spinner icon-spin\' />');
                this.model.save().always(function () {
                    self.$el.html('<i class =\'icon-download-alt\' title=\'Add to download queue\' />');
                });
            },

            render: function () {

                this.$el.html('<i class =\'icon-download-alt\' title=\'Add to download queue\' />');
                return this;

            }
        });
    });
