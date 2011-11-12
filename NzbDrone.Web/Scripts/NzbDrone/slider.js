$(document).ready(function () {
    $(".sliderButton").live('click', function () {
        sliderToggle(this);
    });
});

function sliderToggle(sliderButton) {
    //Get sliderContent
    var sliderContent = $(sliderButton).siblings('.sliderContent');
    
    //Open the slider
    sliderContent.slideToggle('slow');
    
    //Change the slider Image
    $(sliderButton).children('.sliderImage').toggleClass('sliderOpened sliderClosed');

    //Clear the search box
    $(sliderContent).children('.localSeriesLookup').val('');
    
    //Focus in the search box
    $(sliderContent).children('.localSeriesLookup').focus();

    //Hide the sliders
    hideSliders(sliderContent);

    //Prevent the Address Bar from changing
    return false;
}

function hideSliders(newlyOpenedSlider) {
    $('.sliderContent').each(function (index, value) {
        var newlyOpenedSliderId = $(newlyOpenedSlider).parent('.top-slider').attr('id');
        var id = $(this).parent('.top-slider').attr('id');

        //If the ID's of the top-sliders don't match then hide it
        if (id != newlyOpenedSliderId)
            $(this).slideUp();
    });
}

//Hide slider when text box loses focus
$('.localSeriesLookup').live('blur', function () {
    $('.sliderContent').each(function (index, value) {
         $(this).slideUp();
    });
});
