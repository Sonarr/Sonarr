'use strict';

define(
    [
        'backgrid',
        'Shared/FormatHelpers'
    ], function (Backgrid, FormatHelpers) {
        return Backgrid.Cell.extend({

            className: 'age-cell',

            render: function () {
                var age = this.model.get('age');
                var ageHours = this.model.get('ageHours');

                if (age === 0) {
                    this.$el.html('{0} {1}'.format(ageHours.toFixed(1), this.plural(Math.round(ageHours), 'hour')));
                }

                else {
                    this.$el.html('{0} {1}'.format(age, this.plural(age, 'day')));
                }

                this.delegateEvents();
                return this;
            },

            plural: function (input, unit) {
                if (input === 1) {
                    return unit;
                }

                return unit + 's';
            }
        });
    });
