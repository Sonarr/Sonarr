function bestDateString(sourceDate) {
    if (!sourceDate) {
        return '';
    }

    var date = Date.create(sourceDate);

    if (date.isYesterday()) {
        return 'Yesterday';
    }
    if (date.isToday()) {
        return 'Today';
    }
    if (date.isTomorrow()) {
        return 'Tomorrow';
    }
    if (date.isAfter(Date.create('tomorrow')) && date.isBefore(Date.create().addDays(7))) {
        return date.format('{Weekday}');
    }

    return date.format('{MM}/{dd}/{yyyy}');
}