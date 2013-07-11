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

            GetValue: function (key, defaultValue) {

                var storeValue = localStorage.getItem(key);

                if (!storeValue) {
                    return defaultValue;
                }

                return storeValue.toString();
            },

            SetValue: function (key, value) {

                console.log('Config: [{0}] => [{1}] '.format(key, value));

                if (this.GetValue(key) === value.toString()) {
                    return;
                }

                localStorage.setItem(key, value);
                App.vent.trigger(this.Events.ConfigUpdatedEvent, {key: key, value: value});

            }

        };
    });
