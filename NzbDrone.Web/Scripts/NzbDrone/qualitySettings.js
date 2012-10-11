var deleteQualityProfileUrl = '../../Settings/DeleteQualityProfile';

$(document).on("click", "#addProfile", function (event) {
    $.ajax({
        url: this.href,
        cache: false,
        success: function (html) {
            $("#profiles").append(html);
        }
    });

    event.preventDefault();
});

$(document).on('click', '.delete-profile', function (e) {
    var container = $(this).closest('.profileSection');
    var id = $(container).attr('data-profile-id');

    $.ajax({
        type: "POST",
        url: deleteQualityProfileUrl,
        data: jQuery.param({ profileId: id }),
        success: function (data, textStatus, jqXHR) {
            $(container).remove();
            removeOption(id);
        }
    });
    
    e.preventDefault();
});

function renameOption(text, value) {
    $("#DefaultQualityProfileId option[value='" + value + "']").html(text);
}

function addOption(text, value) {
    var myCombo = $('#DefaultQualityProfileId');

    var exists = $("#DefaultQualityProfileId option[value='" + value + "']");

    if (exists.length == 0)
        myCombo.append($('\<option\> \</option\>').val(value).html(text));
}

function removeOption(value) {
    $("#DefaultQualityProfileId option[value='" + value + "']").remove();
}

function getProfileId(obj) {
    var parentProfileSection = $(obj).closest('.profileSection');
    return parentProfileSection.attr('data-profile-id');
}

function getCleanId(obj) {
    var parentProfileSection = $(obj).parents('.profileSection');
    return parentProfileSection.children('.cleanId').val();
}

$(document).on('keyup', '.profileName_textbox', function () {
    var value = $(this).val();

    $(this).closest('.profileSection').find('.titleText').text(value);
    var profileId = getProfileId(this);
    
    renameOption(value, profileId);
}).keyup();

$(document).on('click', '.quality-selectee', function () {
    var id = $(this).attr('id');
    var cleanId = getCleanId(this);
    var cutoff = '#' + cleanId + '_Cutoff';
    var name = jQuery('[for="' + id + '"]').children('.ui-button-text').text();

    //Remove 'Unknown'
    $(cutoff + ' option').each(function () { if ($(this).text().indexOf('Unknown') > -1) $(cutoff + ' option').remove(':contains("' + $(this).text() + '")'); });

    //Add option to cutoff SelectList
    if ($(this).attr('checked')) {
        $('<option>' + name + '</option>').appendTo(cutoff);
    }

    //Remove option from cutoff SelectList
    else {
        $(cutoff + ' option').each(function () {
            if ($(this).text().indexOf(name) > -1)
                $(cutoff + ' option').remove(':contains("' + $(this).text() + '")');
        });
    }
});

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
    }
};

function setupSliders() {
    $(".slider").each(function () {
        var localOptions = sliderOptions;
        localOptions["value"] = $(this).siblings('.slider-value').val();

        $(this).empty().slider(localOptions);
    });
}