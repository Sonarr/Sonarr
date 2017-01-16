var SettingsModelBase = require('../SettingsModelBase');

module.exports = SettingsModelBase.extend({
    url            : window.NzbDrone.ApiRoot + '/config/host',
    successMessage : 'General settings saved',
    errorMessage   : 'Failed to save general settings'
});