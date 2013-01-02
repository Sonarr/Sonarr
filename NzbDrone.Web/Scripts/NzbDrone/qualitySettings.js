var sliderOptions = {
    min: 0,
    max: 200,
    value: 0,
    step: 1,
    create: function (event, ui) {
        var startingValue = $(this).siblings('.slider-value').val();
        $(this).siblings('.30-minute').text(startingValue * 30);
        $(this).siblings('.60-minute').text(startingValue * 60);
    },
    slide: function (event, ui) {
        $(this).siblings('.slider-value').val(ui.value);
        $(this).siblings('.30-minute').text(ui.value * 30);
        $(this).siblings('.60-minute').text(ui.value * 60);
    },
    change: function (event, ui) {
        $(this).siblings('.slider-value').val(ui.value).trigger('change');
    }
};

$('.quality-selectee').livequery(function () {
    $(this).button();
});

$('.slider').livequery(function () {
    var localOptions = sliderOptions;
    localOptions["value"] = $(this).siblings('.slider-value').val();

    $(this).empty().slider(localOptions);
});
