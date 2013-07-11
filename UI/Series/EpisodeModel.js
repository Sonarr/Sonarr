'use strict';
define(
    [
        'backbone',
        'Series/SeriesModel'
    ], function (Backbone, SeriesModel) {
        return Backbone.Model.extend({

            mutators: {
                paddedEpisodeNumber: function () {
                    return this.get('episodeNumber').pad(2);
                },
                day                : function () {
                    return Date.create(this.get('airDate')).format('{dd}');
                },
                month              : function () {
                    return Date.create(this.get('airDate')).format('{Mon}');
                },
                startTime          : function () {
                    var start = Date.create(this.get('airDate'));

                    if (start.format('{mm}') === '00') {
                        return start.format('{h}{tt}');
                    }

                    return start.format('{h}.{mm}{tt}');
                },
                end                : function () {

                    if (this.has('series')) {
                        var start = Date.create(this.get('airDate'));
                        var runtime = this.get('series').get('runtime');

                        return start.addMinutes(runtime);
                    }

                    return undefined;
                },
                statusLevel        : function () {
                    var hasFile = this.get('hasFile');
                    var currentTime = Date.create();
                    var start = Date.create(this.get('airDate'));
                    var end = Date.create(this.get('end'));

                    if (currentTime.isBetween(start, end)) {
                        return 'warning';
                    }

                    if (start.isBefore(currentTime) && !hasFile) {
                        return 'danger';
                    }

                    if (hasFile) {
                        return 'success';
                    }

                    return 'primary';
                },
                hasAired           : function () {
                    return Date.create(this.get('airDate')).isBefore(Date.create());
                }
            },


            parse: function (model) {
                model.series = new SeriesModel(model.series);

                return model;
            },

            toJSON: function () {
                var json = _.clone(this.attributes);

                _.each(this.mutators, _.bind(function (mutator, name) {
                    // check if we have some getter mutations
                    if (_.isObject(this.mutators[name]) === true && _.isFunction(this.mutators[name].get)) {
                        json[name] = _.bind(this.mutators[name].get, this)();
                    }
                    else {
                        json[name] = _.bind(this.mutators[name], this)();
                    }
                }, this));

                if (this.has('series')) {
                    json.series = this.get('series').toJSON();
                }
                return json;
            },

            defaults: {
                seasonNumber: 0,
                status      : 0
            }
        });
    });
