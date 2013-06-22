'use strict';
define(['app',
    'Settings/SettingsModelBase'], function (App, ModelBase) {
    return  ModelBase.extend({
        url           : App.Constants.ApiRoot + '/config/naming',
        successMessage: 'Naming settings saved',
        errorMessage  : 'Couldn\'t save naming settings'
    });

});
