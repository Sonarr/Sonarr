define(
    [
        'moment',
        'backgrid',
        'Shared/UiSettingsModel',
        'Shared/FormatHelpers'
    ], function (moment, Backgrid, UiSettings, FormatHelpers) {
        return Backgrid.Cell.extend({

            className: 'age-cell',

            render: function () {
                var age = this.model.get('age');
                var ageHours = this.model.get('ageHours');
                var published = moment(this.model.get('publishDate'));
                var publishedFormatted = published.format('{0} LTS'.format(UiSettings.get('shortDateFormat')));
                var formatted = age;
                var suffix = this.plural(age, 'day');

                if (age === 0) {
                    formatted = ageHours.toFixed(1);
                    suffix = this.plural(Math.round(ageHours), 'hour');
                }

                this.$el.html('<div title="{2}">{0} {1}</div>'.format(formatted, suffix, publishedFormatted));

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

'use strict';
