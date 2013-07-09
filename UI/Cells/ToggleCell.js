'use strict';

define(
    [
        'backgrid'
    ], function (Backgrid) {
        return Backgrid.Cell.extend({

            className: 'toggle-cell clickable',

            events: {
                'click': '_onClick'
            },


            _onClick: function () {
                var name = this.column.get('name');
                this.model.set(name, !this.model.get(name));
                this.render();

                this.model.save();
            },

            render: function () {
                this.$el.empty();
                this.$el.html('<i />');

                var name = this.column.get('name');

                if (this.model.get(name)) {
                    this.$('i').addClass(this.column.get('trueClass'));
                }
                else {
                    this.$('i').addClass(this.column.get('falseClass'));
                }

                var tooltip = this.column.get('tooltip');

                if (tooltip) {
                    this.$('i').attr('title', tooltip);
                }

                return this;
            }
        });
    });
