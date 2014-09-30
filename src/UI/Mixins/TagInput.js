'use strict';
define(
    [
        'jquery',
        'underscore',
        'Tags/TagCollection',
        'Tags/TagModel',
        'bootstrap.tagsinput'
    ], function ($, _, TagCollection, TagModel) {

        $.fn.tagInput = function () {
            var input = this;
            var tagValues = $(this).val();
            var tags = getExistingTags(tagValues);

            $(this).attr('placeholder', 'Enter tag');
            $(this).val(null);

            var tagInput = $(this).tagsinput({
                freeInput: true,
                itemValue : 'id',
                itemText  : 'label',
                trimValue : true,
                typeaheadjs : {
                    name: 'tags',
                    displayKey: 'label',
                    source: substringMatcher()
                }
            });

            //Override the free input being set to false because we're using objects
            $(tagInput)[0].options.freeInput = true;

            var proto = $(tagInput)[0].__proto__;
            var originalAdd = proto.add;
            proto.add = function (item, dontPushVal) {
                if (typeof item === 'string') {
                    //create a new tag

                    var newTag = new TagModel();
                    newTag.set({ label: item });
                    TagCollection.add(newTag);

                    newTag.save().done(function () {
                        item = newTag.toJSON();
                        originalAdd.call($(tagInput)[0], item, dontPushVal);
                    });
                }

                else {
                    originalAdd.call($(tagInput)[0], item, dontPushVal);
                }
            };

            $(this).tagsinput('removeAll');

            _.each(tags, function(tag) {
                $(input).tagsinput('add', tag);
            });
        };

        var substringMatcher = function() {
            return function findMatches(q, cb) {
                // regex used to determine if a string contains the substring `q`
                var substrRegex = new RegExp(q, 'i');

                var matches = _.select(TagCollection.toJSON(), function (tag) {
                    return substrRegex.test(tag.label);
                });

                cb(matches);
            };
        };

        var getExistingTags = function(tagValues) {
            var tags = _.select(TagCollection.toJSON(), function (tag) {
                return _.contains(tagValues, tag.id.toString());
            });

            return tags;
        };
    });
