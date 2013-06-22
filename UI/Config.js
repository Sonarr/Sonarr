'use strict';
define(
    [
        'app'
    ], function () {

        NzbDrone.Config = {
            Events: {
                ConfigUpdatedEvent: 'ConfigUpdatedEvent'
            },
            Keys  : {
                DefaultQualityProfileId: 'DefaultQualityProfileId'
            }
        };

        NzbDrone.Config.GetValue = function (key, defaultValue) {

            var storeValue = localStorage.getItem(key);

            if (!storeValue) {
                return defaultValue;
            }

            return storeValue.toString();
        };

        NzbDrone.Config.SetValue = function (key, value) {

            console.log('Config: [{0}] => [{1}] '.format(key, value));

            if (NzbDrone.Config.GetValue(key) === value.toString()) {
                return;
            }

            localStorage.setItem(key, value);
            NzbDrone.vent.trigger(NzbDrone.Config.Events.ConfigUpdatedEvent, {key: key, value: value});

        };

        return NzbDrone.Config;

    });
