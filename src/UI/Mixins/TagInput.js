'use strict';
define(
    [
        'jquery',
        'underscore',
        'Tags/TagCollection',
        'Tags/TagModel',
        'bootstrap.tagsinput'
    ], function ($, _, TagCollection, TagModel) {

        var originalAdd = $.fn.tagsinput.Constructor.prototype.add;

        $.fn.tagsinput.Constructor.prototype.add = function (item, dontPushVal) {
            var self = this;

            if (typeof item === 'string') {

                if (item === null || item === '') {
                    return;
                }

                var existing = _.find(TagCollection.toJSON(), { label: item });

                if (existing) {
                    originalAdd.call(this, existing, dontPushVal);
                    return;
                }

                var newTag = new TagModel();
                newTag.set({ label: item.toLowerCase() });
                TagCollection.add(newTag);

                newTag.save().done(function () {
                    item = newTag.toJSON();
                    originalAdd.call(self, item, dontPushVal);
                });
            }

            else {
                originalAdd.call(this, item, dontPushVal);
            }
        };

        $.fn.tagInput = function (options) {
            var input = this;
            var model = options.model;
            var property = options.property;
            var tags = getExistingTags(model.get(property));

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

            //Remove any existing tags and re-add them
            $(this).tagsinput('removeAll');

            _.each(tags, function(tag) {
                $(input).tagsinput('add', tag);
            });

            $(this).tagsinput('refresh');

            $(this).on('itemAdded', function(event) {
                var tags = model.get(property);
                tags.push(event.item.id);
                model.set(property, tags);
            });

            $(this).on('itemRemoved', function(event) {
                if (!event.item) {
                    return;
                }

                var tags = _.without(model.get(property), event.item.id);

                model.set(property, tags);
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
            return _.select(TagCollection.toJSON(), function (tag) {
                return _.contains(tagValues, tag.id);
            });
        };
    });
