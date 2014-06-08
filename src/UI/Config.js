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
                DefaultProfileId    : 'DefaultProfileId',
                DefaultRootFolderId     : 'DefaultRootFolderId',
                UseSeasonFolder         : 'UseSeasonFolder',
                DefaultSeriesType       : 'DefaultSeriesType',
                AdvancedSettings        : 'advancedSettings'
            },

            getValueBoolean: function (key, defaultValue) {
                defaultValue = defaultValue || false;

                return this.getValue(key, defaultValue.toString()) === 'true';
            },

            getValue: function (key, defaultValue) {
                var storeValue = window.localStorage.getItem(key);

                if (!storeValue) {
                    return defaultValue;
                }

                return storeValue.toString();
            },

            setValue: function (key, value) {

                console.log('Config: [{0}] => [{1}]'.format(key, value));

                if (this.getValue(key) === value.toString()) {
                    return;
                }

                try {
                    window.localStorage.setItem(key, value);
                    vent.trigger(this.Events.ConfigUpdatedEvent, {key: key, value: value});
                }
                catch (error) {
                    console.error('Unable to save config: [{0}] => [{1}]'.format(key, value));
                }
            }
        };
    });
