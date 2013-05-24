"use strict";
define(['app'], function () {

    NzbDrone.Config.GetValue = function (key, defaultValue) {

        var storeValue = localStorage.getItem(key);

        if (!storeValue) {
            return defaultValue;
        }

        return storeValue;
    };

    NzbDrone.Config.SetValue = function (key, value) {
        console.log('Config: [{0}] => [{1}] '.format(key, value));
        localStorage.setItem(key, value);
    };

});
