'use strict';
define(
    [
        'Settings/SettingsModelBase'
    ], function (SettingsModelBase) {
        return SettingsModelBase.extend({
            url           : window.NzbDrone.ApiRoot + '/config/downloadclient',
            successMessage: 'Download client settings saved',
            errorMessage  : 'Failed to save download client settings'
        });
    });
