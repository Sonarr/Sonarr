define(['bootstrap'], function () {

    $.fn.folderAutoComplete = function () {
        $(this).typeahead({
            source: function (query, process) {
                $.ajax({
                    url: '/api/directories',
                    dataType: "json",
                    type: "POST",
                    data: { query: query },
                    success: function (data) {
                        process(data);
                    }
                });
            },
            minLength: 3
        });
    };

});
