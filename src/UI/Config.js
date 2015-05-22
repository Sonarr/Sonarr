var $ = require('jquery');
var vent = require('./vent');

module.exports = {
    Events : {
        ConfigUpdatedEvent : 'ConfigUpdatedEvent'
    },

    Keys : {
        DefaultProfileId    : 'DefaultProfileId',
        DefaultRootFolderId : 'DefaultRootFolderId',
        UseSeasonFolder     : 'UseSeasonFolder',
        DefaultSeriesType   : 'DefaultSeriesType',
        MonitorEpisodes     : 'MonitorEpisodes',
        AdvancedSettings    : 'advancedSettings'
    },

    getValueJson : function (key, defaultValue) {
        defaultValue = defaultValue || {};

        var storeValue = window.localStorage.getItem(key);

        if (!storeValue) {
            return defaultValue;
        }

        return $.parseJSON(storeValue);
    },

    getValueBoolean : function(key, defaultValue) {
        defaultValue = defaultValue || false;

        return this.getValue(key, defaultValue.toString()) === 'true';
    },

    getValue : function(key, defaultValue) {
        var storeValue = window.localStorage.getItem(key);

        if (!storeValue) {
            return defaultValue;
        }

        return storeValue.toString();
    },

    setValueJson : function(key, value) {
        return this.setValue(key, JSON.stringify(value));
    },

    setValue : function(key, value) {

        console.log('Config: [{0}] => [{1}]'.format(key, value));

        if (this.getValue(key) === value.toString()) {
            return;
        }

        try {
            window.localStorage.setItem(key, value);
            vent.trigger(this.Events.ConfigUpdatedEvent, {
                key   : key,
                value : value
            });
        }
        catch (error) {
            console.error('Unable to save config: [{0}] => [{1}]'.format(key, value));
        }
    }
};
