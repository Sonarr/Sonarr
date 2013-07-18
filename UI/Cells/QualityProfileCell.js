'use strict';
define(
    [
        'backgrid',
        'Quality/QualityProfileCollection'
    ], function (Backgrid, QualityProfileCollection) {
        return Backgrid.Cell.extend({
            className: 'quality-profile-cell',

            render: function () {

                this.$el.empty();
                var qualityProfileId = this.model.get(this.column.get('name'));

                var profile = _.findWhere(QualityProfileCollection.models, { id: qualityProfileId });

                if (profile) {
                    this.$el.html(profile.get('name'));
                }

                return this;
            }
        });
    });
