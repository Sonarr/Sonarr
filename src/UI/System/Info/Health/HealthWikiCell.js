'use strict';
define(
    [
        'jquery',
        'backgrid'
    ], function ($, Backgrid) {
        return Backgrid.UriCell.extend({

            className: 'wiki-link-cell',

            title: 'Read the Wiki for more information',

            text: 'Wiki',

            render: function () {
                this.$el.empty();
                var rawValue = this.model.get(this.column.get("name"));
                var formattedValue = this.formatter.fromRaw(rawValue, this.model);
                this.$el.append($("<a>", {
                    tabIndex: -1,
                    href: rawValue,
                    title: this.title || formattedValue,
                    target: this.target
                }).text(this.text));
                this.delegateEvents();
                return this;
            }
        });
    });
