'use strict';
define({
    register: function (Handlebars) {
        Handlebars.registerHelper("debug", function (optionalValue) {

            console.group('Handlebar context');

            console.log(this);
            if (optionalValue) {

                console.group('optional values');
                console.log('optinal values');
                console.groupEnd();
            }
            console.groupEnd();
        });
    }
});
