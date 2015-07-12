var _ = require('underscore');
var Marionette = require('marionette');
var Config = require('../../../Config');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');

var view = Marionette.ItemView.extend({
    template : 'Settings/LanguageProfile/Edit/EditLanguageProfileViewTemplate',

    ui : { cutoff : '.x-cutoff'
    },

    getCutoff : function() {
        var self = this;

        return _.findWhere(_.pluck(this.model.get('languages'), 'language'), { id : parseInt(self.ui.cutoff.val(), 10) });
    }
});

AsValidatedView.call(view);

module.exports = AsModelBoundView.call(view);