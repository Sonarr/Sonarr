'use strict';
define(
    [
        'app'
    ], function (App) {
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

                var storeValue = localStorage.getItem(key);

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

                localStorage.setItem(key, value);
                App.vent.trigger(this.Events.ConfigUpdatedEvent, {key: key, value: value});

            }
        };
    });
