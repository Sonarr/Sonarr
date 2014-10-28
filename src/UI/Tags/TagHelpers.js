'use strict';
define(
    [
        'underscore',
        'handlebars',
        'Tags/TagCollection'
    ], function (_, Handlebars, TagCollection) {

        Handlebars.registerHelper('tagDisplay', function (tags) {

            var tagLabels = _.map(TagCollection.filter(function (tag) {
                return _.contains(tags, tag.get('id'));
            }), function (tag){
                return '<span class="label label-info">{0}</span>'.format(tag.get('label'));
            });

            return new Handlebars.SafeString(tagLabels.join(' '));
        });

        Handlebars.registerHelper('genericTagDisplay', function (tags, classes) {

            if (!tags) {
                return new Handlebars.SafeString('');
            }

            var tagLabels = _.map(tags.split(','), function (tag) {
                return '<span class="{0}">{1}</span>'.format(classes, tag);
            });

            return new Handlebars.SafeString(tagLabels.join(' '));
        });
    });
