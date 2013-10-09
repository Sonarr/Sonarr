'use strict';
define(
    [
        'Settings/SettingsModelBase'
    ], function (SettingsModelBase) {
        return SettingsModelBase.extend({
            url           : window.NzbDrone.ApiRoot + '/settings',
            successMessage: 'Settings saved',
            errorMessage  : 'Failed to save settings'
        });
    });
