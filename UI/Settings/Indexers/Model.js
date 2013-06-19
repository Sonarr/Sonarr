"use strict";
define([
    'Settings/SettingsModelBase'], function (ModelBase) {
    return ModelBase.extend({

        successMessage: 'Indexer Saved',
        errorMessage  : 'Couldn\'t save indexer'


    });
});
