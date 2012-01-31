var deleteQualityProfileUrl = '../../Settings/DeleteQualityProfile';

$(document).ready(function () {
    setupSliders();
});

$("#addItem").live('click', function () {
    $.ajax({
        url: this.href,
        cache: false,
        success: function (html) {
            $("#profiles").append(html);

        }
    });
    return false;
});

function deleteProfile(id) {
    sendToServer(id);
    var profileDiv = '#profile_' + id;
    $(profileDiv).remove();
}

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

function sendToServer(id) {
    $.ajax({
        type: "POST",
        url: deleteQualityProfileUrl,
        data: jQuery.param({ profileId: id }),
        error: function (req, status, error) {
            alert("Sorry! We could not delete your Profile at this time. " + error);
        },
        success: function (data, textStatus, jqXHR) {
            if (data == "ok") {
                $("#profile_" + id).remove();
                removeOption(id);
            }

            else {
                alert(data);
            }
        }
    });
}

function getProfileId(obj) {
    var parentProfileSection = $(obj).parents('.profileSection');
    return parentProfileSection.children('.qualityProfileId').val();
}

function getCleanId(obj) {
    var parentProfileSection = $(obj).parents('.profileSection');
    return parentProfileSection.children('.cleanId').val();
}

$(".profileName_textbox").live('keyup', function () {
    var value = $(this).val();
    var profileId = getProfileId(this);
    $("#title_" + profileId).text(value);
    renameOption(value, profileId);
}).keyup();

$('.quality-selectee').live('click', function () {
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