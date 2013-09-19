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

                this.$el.html('<i class="icon-spinner icon-spin" />');
                var promise = this.model.save();

                promise.done(function () {
                    self.$el.html('<i class="icon-ok" title="Added to downloaded queue" />');
                });

                promise.fail(function () {
                    self.$el.html('<i class="icon-download-alt" title="Add to download queue" />');
                });
            },

            render: function () {
                this.$el.empty();

                if (this.model.get('downloadAllowed'))
                {
                    this.$el.html('<i class="icon-download-alt" title="Add to download queue" />');
                }

                return this;
            }
        });
    });
