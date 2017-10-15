var _ = require('underscore');
var Marionette = require('marionette');
var LanguageCollection = require('../Language/LanguageCollection');
var Config = require('../../../Config');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');
require('../../../Mixins/TagInput');
require('bootstrap');
require('bootstrap.tagsinput');

var view = Marionette.ItemView.extend({
    template : 'Settings/Profile/Edit/EditProfileViewTemplate',

    ui : { cutoff : '.x-cutoff',
 					preferred : '.x-preferred'},

    onRender : function() {
        this.ui.preferred.tagsinput({
            trimValue : true,
            allowDuplicates: true,
            tagClass  : 'label label-success'
        });
    },

    templateHelpers : function() {
        return {
            languages : LanguageCollection.toJSON()
        };
    },

    getCutoff : function() {
        var self = this;

        return _.findWhere(_.pluck(this.model.get('items'), 'quality'), { id : parseInt(self.ui.cutoff.val(), 10) });
    }
});

AsValidatedView.call(view);

module.exports = AsModelBoundView.call(view);