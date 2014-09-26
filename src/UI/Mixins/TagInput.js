'use strict';
define(
    [
        'jquery',
        'Tags/TagCollection',
        'bootstrap.tagsinput'
    ], function ($, TagCollection) {

    $.fn.tagInput = function () {
        var test = TagCollection.map(function (tag) {
            return tag.get('label');
        });

        $(this).tagsinput({
            freeInput: true,
//            itemValue : 'id',
//            itemText  : 'label',
//            trimValue : true,
            typeahead : {
//                source : test
                source: ['Amsterdam', 'Washington', 'Sydney', 'Beijing', 'Cairo']
            }
        });
    };
});
