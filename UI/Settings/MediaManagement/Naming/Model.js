'use strict';
define(
    [
        'Settings/SettingsModelBase'
    ], function (ModelBase) {
        return  ModelBase.extend({
            url           : window.NzbDrone.ApiRoot + '/config/naming',
            successMessage: 'MediaManagement settings saved',
            errorMessage  : 'Couldn\'t save naming settings'
        });

    });
