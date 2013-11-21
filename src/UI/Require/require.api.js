define([], function () {
        'use strict';
        return {
            load: function (name, parentRequire, onload, config) {

                if (config.isBuild) {
                    return onload();
                }

                var resource = name.split('!')[0];
                var url = window.NzbDrone.ApiRoot + '/' + resource;

                window.$.ajax({
                    url: url
                }).done(function (data) {
                        onload(data);
                    }).error(function (xhr, status, error) {
                        onload.error({
                            xhr   : xhr,
                            status: status,
                            error : error});
                    });
            }
        };
    });
