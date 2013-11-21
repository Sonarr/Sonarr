'use strict';
define(
    [
        'vent'
    ], function (vent) {
        return {
            Events: {
                ConfigUpdatedEvent: 'ConfigUpdatedEvent'
            },
            Keys  : {
                DefaultQualityProfileId: 'DefaultQualityProfileId',
                DefaultRootFolderId: 'DefaultRootFolderId'
            },

            getValueBoolean: function (key, defaultValue) {
                return this.getValue(key, defaultValue) === 'true';
            },

            getValue: function (key, defaultValue) {


                test = "";

                var storeValue = window.localStorage.getItem(key);

                if (!storeValue) {
                    return defaultValue;
                }

                return storeValue.toString();
            },

            setValue: function (key, value) {

                console.log('Config: [{0}] => [{1}] '.format(key, value));

                if (this.getValue(key) === value.toString()) {
                    return;
                }

                window.localStorage.setItem(key, value);
                vent.trigger(this.Events.ConfigUpdatedEvent, {key: key, value: value});

            }
        };
    });
