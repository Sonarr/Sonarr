'use strict';
define(
    [
        'Settings/SettingsModelBase'
    ], function (SettingsModelBase) {
        return SettingsModelBase.extend({
            url           : window.NzbDrone.ApiRoot + '/config/mediamanagement',
            successMessage: 'Media management settings saved',
            errorMessage  : 'Failed to save media managemnent settings'
        });
    });
