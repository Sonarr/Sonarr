'use strict';
define(
    [
        'backgrid',
        'Profile/ProfileCollection',
        'underscore'
    ], function (Backgrid, ProfileCollection,_) {
        return Backgrid.Cell.extend({
            className: 'profile-cell',

            render: function () {

                this.$el.empty();
                var profileId = this.model.get(this.column.get('name'));

                var profile = _.findWhere(ProfileCollection.models, { id: profileId });

                if (profile) {
                    this.$el.html(profile.get('name'));
                }

                return this;
            }
        });
    });
