﻿'use strict';
define(
    [
        'Settings/SettingsModelBase'
    ], function (SettingsModelBase) {
        return SettingsModelBase.extend({

            url           : window.NzbDrone.ApiRoot + '/config/ui',
            successMessage: 'UI settings saved',
            errorMessage  : 'Failed to save UI settings'
        });
    });
