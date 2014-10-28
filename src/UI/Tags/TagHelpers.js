'use strict';
define(
    [
        'handlebars',
        'Tags/TagCollection'
    ], function (Handlebars, TagCollection) {

        Handlebars.registerHelper('tagInput', function () {

            var unit = 'days';
            var age = this.age;

            if (age < 2) {
                unit = 'hours';
                age = parseFloat(this.ageHours).toFixed(1);
            }

            return new Handlebars.SafeString('<dt>Age (when grabbed):</dt><dd>{0} {1}</dd>'.format(age, unit));
        });
    });
