$(document).ready(function () {
    $(".sliderButtonContainer").live('click', function () {
        sliderToggle();
    });
});

function sliderToggle() {
    $('.sliderContent').slideToggle('slow');
    $(".sliderButtonContainer").children('.sliderImage').toggleClass('sliderOpened sliderClosed');

    //Prevent the Address Bar from changing
    return false;
}