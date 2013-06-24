'use strict';
define({
        Execute: function (name, properties) {
            var data = { command: name };

            if (properties) {
                $.extend(data, properties);
            }

            return $.ajax({
                type: 'POST',
                url : window.ApiRoot + '/command',
                data: JSON.stringify(data)
            });
        }
    });
