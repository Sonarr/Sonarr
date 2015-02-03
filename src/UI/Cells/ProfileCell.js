var Backgrid = require('backgrid');
var ProfileCollection = require('../Profile/ProfileCollection');
var _ = require('underscore');

module.exports = Backgrid.Cell.extend({
    className : 'profile-cell',
    render    : function(){
        this.$el.empty();
        var profileId = this.model.get(this.column.get('name'));
        var profile = _.findWhere(ProfileCollection.models, {id : profileId});
        if(profile) {
            this.$el.html(profile.get('name'));
        }
        return this;
    }
});