/*! tableSorter 2.4+ widgets - updated 1/29/2013
 *
 * Column Styles
 * Column Filters
 * Column Resizing
 * Sticky Header
 * UI Theme (generalized)
 * Save Sort
 * ["zebra", "uitheme", "stickyHeaders", "filter", "columns"]
 */
/*jshint browser:true, jquery:true, unused:false, loopfunc:true */
/*global jQuery: false, localStorage: false, navigator: false */
;(function($){
    "use strict";
    $.tablesorter = $.tablesorter || {};

    $.tablesorter.themes = {
        "bootstrap" : {
            table      : 'table table-bordered table-striped',
            header     : 'bootstrap-header', // give the header a gradient background
            footerRow  : '',
            footerCells: '',
            icons      : '', // add "icon-white" to make them white; this icon class is added to the <i> in the header
            sortNone   : 'bootstrap-icon-unsorted',
            sortAsc    : 'icon-chevron-up',
            sortDesc   : 'icon-chevron-down',
            active     : '', // applied when column is sorted
            hover      : '', // use custom css here - bootstrap class may not override it
            filterRow  : '', // filter row class
            even       : '', // even row zebra striping
            odd        : ''  // odd row zebra striping
        },
        "jui" : {
            table      : 'ui-widget ui-widget-content ui-corner-all', // table classes
            header     : 'ui-widget-header ui-corner-all ui-state-default', // header classes
            footerRow  : '',
            footerCells: '',
            icons      : 'ui-icon', // icon class added to the <i> in the header
            sortNone   : 'ui-icon-carat-2-n-s',
            sortAsc    : 'ui-icon-carat-1-n',
            sortDesc   : 'ui-icon-carat-1-s',
            active     : 'ui-state-active', // applied when column is sorted
            hover      : 'ui-state-hover',  // hover class
            filterRow  : '',
            even       : 'ui-widget-content', // even row zebra striping
            odd        : 'ui-state-default'   // odd row zebra striping
        }
    };

    // *** Store data in local storage, with a cookie fallback ***
    /* IE7 needs JSON library for JSON.stringify - (http://caniuse.com/#search=json)
     if you need it, then include https://github.com/douglascrockford/JSON-js

     $.parseJSON is not available is jQuery versions older than 1.4.1, using older
     versions will only allow storing information for one page at a time

     // *** Save data (JSON format only) ***
     // val must be valid JSON... use http://jsonlint.com/ to ensure it is valid
     var val = { "mywidget" : "data1" }; // valid JSON uses double quotes
     // $.tablesorter.storage(table, key, val);
     $.tablesorter.storage(table, 'tablesorter-mywidget', val);

     // *** Get data: $.tablesorter.storage(table, key); ***
     v = $.tablesorter.storage(table, 'tablesorter-mywidget');
     // val may be empty, so also check for your data
     val = (v && v.hasOwnProperty('mywidget')) ? v.mywidget : '';
     alert(val); // "data1" if saved, or "" if not
     */
    $.tablesorter.storage = function(table, key, val){
        var d, k, ls = false, v = {},
            id = table.id || $('.tablesorter').index( $(table) ),
            url = window.location.pathname;
        try { ls = !!(localStorage.getItem); } catch(e) {}
        // *** get val ***
        if ($.parseJSON){
            if (ls){
                v = $.parseJSON(localStorage[key]) || {};
            } else {
                k = document.cookie.split(/[;\s|=]/); // cookie
                d = $.inArray(key, k) + 1; // add one to get from the key to the value
                v = (d !== 0) ? $.parseJSON(k[d]) || {} : {};
            }
        }
        // allow val to be an empty string to
        if ((val || val === '') && window.JSON && JSON.hasOwnProperty('stringify')){
            // add unique identifiers = url pathname > table ID/index on page > data
            if (!v[url]) {
                v[url] = {};
            }
            v[url][id] = val;
            // *** set val ***
            if (ls){
                localStorage[key] = JSON.stringify(v);
            } else {
                d = new Date();
                d.setTime(d.getTime() + (31536e+6)); // 365 days
                document.cookie = key + '=' + (JSON.stringify(v)).replace(/\"/g,'\"') + '; expires=' + d.toGMTString() + '; path=/';
            }
        } else {
            return v && v[url] ? v[url][id] : {};
        }
    };

    // Widget: General UI theme
    // "uitheme" option in "widgetOptions"
    // **************************
    $.tablesorter.addWidget({
                                id: "uitheme",
                                format: function(table){
                                    var time, klass, $el, $tar,
                                        t = $.tablesorter.themes,
                                        $t = $(table),
                                        c = table.config,
                                        wo = c.widgetOptions,
                                        theme = c.theme !== 'default' ? c.theme : wo.uitheme || 'jui', // default uitheme is 'jui'
                                        o = t[ t[theme] ? theme : t[wo.uitheme] ? wo.uitheme : 'jui'],
                                        $h = $(c.headerList),
                                        sh = 'tr.' + (wo.stickyHeaders || 'tablesorter-stickyHeader'),
                                        rmv = o.sortNone + ' ' + o.sortDesc + ' ' + o.sortAsc;
                                    if (c.debug) { time = new Date(); }
                                    if (!$t.hasClass('tablesorter-' + theme) || c.theme === theme || !table.hasInitialized){
                                        // update zebra stripes
                                        if (o.even !== '') { wo.zebra[0] += ' ' + o.even; }
                                        if (o.odd !== '') { wo.zebra[1] += ' ' + o.odd; }
                                        // add table/footer class names
                                        t = $t
                                            // remove other selected themes; use widgetOptions.theme_remove
                                            .removeClass( c.theme === '' ? '' : 'tablesorter-' + c.theme )
                                            .addClass('tablesorter-' + theme + ' ' + o.table) // add theme widget class name
                                            .find('tfoot');
                                        if (t.length) {
                                            t
                                                .find('tr').addClass(o.footerRow)
                                                .children('th, td').addClass(o.footerCells);
                                        }
                                        // update header classes
                                        $h
                                            .addClass(o.header)
                                            .filter(':not(.sorter-false)')
                                            .hover(function(){
                                                       $(this).addClass(o.hover);
                                                   }, function(){
                                                       $(this).removeClass(o.hover);
                                                   });
                                        if (!$h.find('.tablesorter-wrapper').length) {
                                            // Firefox needs this inner div to position the resizer correctly
                                            $h.wrapInner('<div class="tablesorter-wrapper" style="position:relative;height:100%;width:100%"></div>');
                                        }
                                        if (c.cssIcon){
                                            // if c.cssIcon is '', then no <i> is added to the header
                                            $h.find('.' + c.cssIcon).addClass(o.icons);
                                        }
                                        if ($t.hasClass('hasFilters')){
                                            $h.find('.tablesorter-filter-row').addClass(o.filterRow);
                                        }
                                    }
                                    $.each($h, function(i){
                                        $el = $(this);
                                        $tar = (c.cssIcon) ? $el.find('.' + c.cssIcon) : $el;
                                        if (this.sortDisabled){
                                            // no sort arrows for disabled columns!
                                            $el.removeClass(rmv);
                                            $tar.removeClass(rmv + ' tablesorter-icon ' + o.icons);
                                        } else {
                                            t = ($t.hasClass('hasStickyHeaders')) ? $t.find(sh).find('th').eq(i).add($el) : $el;
                                            klass = ($el.hasClass(c.cssAsc)) ? o.sortAsc : ($el.hasClass(c.cssDesc)) ? o.sortDesc : $el.hasClass(c.cssHeader) ? o.sortNone : '';
                                            $el[klass === o.sortNone ? 'removeClass' : 'addClass'](o.active);
                                            $tar.removeClass(rmv).addClass(klass);
                                        }
                                    });
                                    if (c.debug){
                                        $.tablesorter.benchmark("Applying " + theme + " theme", time);
                                    }
                                },
                                remove: function(table, c, wo){
                                    var $t = $(table),
                                        theme = typeof wo.uitheme === 'object' ? 'jui' : wo.uitheme || 'jui',
                                        o = typeof wo.uitheme === 'object' ? wo.uitheme : $.tablesorter.themes[ $.tablesorter.themes.hasOwnProperty(theme) ? theme : 'jui'],
                                        $h = $t.children('thead').children(),
                                        rmv = o.sortNone + ' ' + o.sortDesc + ' ' + o.sortAsc;
                                    $t
                                        .removeClass('tablesorter-' + theme + ' ' + o.table)
                                        .find(c.cssHeader).removeClass(o.header);
                                    $h
                                        .unbind('mouseenter mouseleave') // remove hover
                                        .removeClass(o.hover + ' ' + rmv + ' ' + o.active)
                                        .find('.tablesorter-filter-row').removeClass(o.filterRow);
                                    $h.find('.tablesorter-icon').removeClass(o.icons);
                                }
                            });

    // Widget: Column styles
    // "columns", "columns_thead" (true) and
    // "columns_tfoot" (true) options in "widgetOptions"
    // **************************
    $.tablesorter.addWidget({
                                id: "columns",
                                format: function(table){
                                    var $tb, $tr, $td, $t, time, last, rmv, i, k, l,
                                        $tbl = $(table),
                                        c = table.config,
                                        wo = c.widgetOptions,
                                        b = c.$tbodies,
                                        list = c.sortList,
                                        len = list.length,
                                        css = [ "primary", "secondary", "tertiary" ]; // default options
                                    // keep backwards compatibility, for now
                                    css = (c.widgetColumns && c.widgetColumns.hasOwnProperty('css')) ? c.widgetColumns.css || css :
                                        (wo && wo.hasOwnProperty('columns')) ? wo.columns || css : css;
                                    last = css.length-1;
                                    rmv = css.join(' ');
                                    if (c.debug){
                                        time = new Date();
                                    }
                                    // check if there is a sort (on initialization there may not be one)
                                    for (k = 0; k < b.length; k++ ){
                                        $tb = $.tablesorter.processTbody(table, b.eq(k), true); // detach tbody
                                        $tr = $tb.children('tr');
                                        l = $tr.length;
                                        // loop through the visible rows
                                        $tr.each(function(){
                                            $t = $(this);
                                            if (this.style.display !== 'none'){
                                                // remove all columns class names
                                                $td = $t.children().removeClass(rmv);
                                                // add appropriate column class names
                                                if (list && list[0]){
                                                    // primary sort column class
                                                    $td.eq(list[0][0]).addClass(css[0]);
                                                    if (len > 1){
                                                        for (i = 1; i < len; i++){
                                                            // secondary, tertiary, etc sort column classes
                                                            $td.eq(list[i][0]).addClass( css[i] || css[last] );
                                                        }
                                                    }
                                                }
                                            }
                                        });
                                        $.tablesorter.processTbody(table, $tb, false);
                                    }
                                    // add classes to thead and tfoot
                                    $tr = wo.columns_thead !== false ? 'thead tr' : '';
                                    if (wo.columns_tfoot !== false) {
                                        $tr += ($tr === '' ? '' : ',') + 'tfoot tr';
                                    }
                                    if ($tr.length) {
                                        $t = $tbl.find($tr).children().removeClass(rmv);
                                        if (list && list[0]){
                                            // primary sort column class
                                            $t.filter('[data-column="' + list[0][0] + '"]').addClass(css[0]);
                                            if (len > 1){
                                                for (i = 1; i < len; i++){
                                                    // secondary, tertiary, etc sort column classes
                                                    $t.filter('[data-column="' + list[i][0] + '"]').addClass(css[i] || css[last]);
                                                }
                                            }
                                        }
                                    }
                                    if (c.debug){
                                        $.tablesorter.benchmark("Applying Columns widget", time);
                                    }
                                },
                                remove: function(table, c, wo){
                                    var k, $tb,
                                        b = c.$tbodies,
                                        rmv = (c.widgetOptions.columns || [ "primary", "secondary", "tertiary" ]).join(' ');
                                    c.$headers.removeClass(rmv);
                                    $(table).children('tfoot').children('tr').children('th, td').removeClass(rmv);
                                    for (k = 0; k < b.length; k++ ){
                                        $tb = $.tablesorter.processTbody(table, b.eq(k), true); // remove tbody
                                        $tb.children('tr').each(function(){
                                            $(this).children().removeClass(rmv);
                                        });
                                        $.tablesorter.processTbody(table, $tb, false); // restore tbody
                                    }
                                }
                            });

    /* Widget: filter
     widgetOptions:
     filter_childRows     : false  // if true, filter includes child row content in the search
     filter_columnFilters : true   // if true, a filter will be added to the top of each table column
     filter_cssFilter     : 'tablesorter-filter' // css class name added to the filter row & each input in the row
     filter_functions     : null   // add custom filter functions using this option
     filter_hideFilters   : false  // collapse filter row when mouse leaves the area
     filter_ignoreCase    : true   // if true, make all searches case-insensitive
     filter_reset         : null   // jQuery selector string of an element used to reset the filters
     filter_searchDelay   : 300    // typing delay in milliseconds before starting a search
     filter_startsWith    : false  // if true, filter start from the beginning of the cell contents
     filter_useParsedData : false  // filter all data using parsed content
     filter_serversideFiltering : false // if true, server-side filtering should be performed because client-side filtering will be disabled, but the ui and events will still be used.
     **************************/
    $.tablesorter.addWidget({
                                id: "filter",
                                format: function(table){
                                    if (table.config.parsers && !$(table).hasClass('hasFilters')){
                                        var i, j, k, l, val, ff, x, xi, st, sel, str,
                                            ft, ft2, $th, rg, s, t, dis, col,
                                            last = '', // save last filter search
                                            ts = $.tablesorter,
                                            c = table.config,
                                            $ths = $(c.headerList),
                                            wo = c.widgetOptions,
                                            css = wo.filter_cssFilter || 'tablesorter-filter',
                                            $t = $(table).addClass('hasFilters'),
                                            b = c.$tbodies,
                                            cols = c.parsers.length,
                                            reg = [ // regex used in filter "check" functions
                                                /^\/((?:\\\/|[^\/])+)\/([mig]{0,3})?$/, // 0 = regex to test for regex
                                                new RegExp(c.cssChildRow), // 1 = child row
                                                /undefined|number/, // 2 = check type
                                                /(^[\"|\'|=])|([\"|\'|=]$)/, // 3 = exact match
                                                /[\"\'=]/g, // 4 = replace exact match flags
                                                /[^\w,. \-()]/g, // 5 = replace non-digits (from digit & currency parser)
                                                /[<>=]/g // 6 = replace operators
                                            ],
                                            parsed = $ths.map(function(i){
                                                return (ts.getData) ? ts.getData($ths.filter('[data-column="' + i + '"]:last'), c.headers[i], 'filter') === 'parsed' : $(this).hasClass('filter-parsed');
                                            }).get(),
                                            time, timer,

                                        // dig fer gold
                                            checkFilters = function(filter){
                                                var arry = $.isArray(filter),
                                                    $inpts = $t.find('thead').eq(0).children('tr').find('select.' + css + ', input.' + css),
                                                    v = (arry) ? filter : $inpts.map(function(){
                                                        return $(this).val() || '';
                                                    }).get(),
                                                    cv = (v || []).join(''); // combined filter values
                                                // add filter array back into inputs
                                                if (arry) {
                                                    $inpts.each(function(i,el){
                                                        $(el).val(filter[i] || '');
                                                    });
                                                }
                                                if (wo.filter_hideFilters === true){
                                                    // show/hide filter row as needed
                                                    $t.find('.tablesorter-filter-row').trigger( cv === '' ? 'mouseleave' : 'mouseenter' );
                                                }
                                                // return if the last search is the same; but filter === false when updating the search
                                                // see example-widget-filter.html filter toggle buttons
                                                if (last === cv && filter !== false) { return; }
                                                $t.trigger('filterStart', [v]);
                                                if (c.showProcessing) {
                                                    // give it time for the processing icon to kick in
                                                    setTimeout(function(){
                                                        findRows(filter, v, cv);
                                                        return false;
                                                    }, 30);
                                                } else {
                                                    findRows(filter, v, cv);
                                                    return false;
                                                }
                                            },
                                            findRows = function(filter, v, cv){
                                                var $tb, $tr, $td, cr, r, l, ff, time, arry;
                                                if (c.debug) { time = new Date(); }

                                                for (k = 0; k < b.length; k++ ){
                                                    $tb = $.tablesorter.processTbody(table, b.eq(k), true);
                                                    $tr = $tb.children('tr');
                                                    l = $tr.length;
                                                    if (cv === '' || wo.filter_serversideFiltering){
                                                        $tr.show().removeClass('filtered');
                                                    } else {
                                                        // loop through the rows
                                                        for (j = 0; j < l; j++){
                                                            // skip child rows
                                                            if (reg[1].test($tr[j].className)) { continue; }
                                                            r = true;
                                                            cr = $tr.eq(j).nextUntil('tr:not(.' + c.cssChildRow + ')');
                                                            // so, if "table.config.widgetOptions.filter_childRows" is true and there is
                                                            // a match anywhere in the child row, then it will make the row visible
                                                            // checked here so the option can be changed dynamically
                                                            t = (cr.length && (wo && wo.hasOwnProperty('filter_childRows') &&
                                                                               typeof wo.filter_childRows !== 'undefined' ? wo.filter_childRows : true)) ? cr.text() : '';
                                                            t = wo.filter_ignoreCase ? t.toLocaleLowerCase() : t;
                                                            $td = $tr.eq(j).children('td');
                                                            for (i = 0; i < cols; i++){
                                                                // ignore if filter is empty or disabled
                                                                if (v[i]){
                                                                    // check if column data should be from the cell or from parsed data
                                                                    if (wo.filter_useParsedData || parsed[i]){
                                                                        x = c.cache[k].normalized[j][i];
                                                                    } else {
                                                                        // using older or original tablesorter
                                                                        x = $.trim($td.eq(i).text());
                                                                    }
                                                                    xi = !reg[2].test(typeof x) && wo.filter_ignoreCase ? x.toLocaleLowerCase() : x;
                                                                    ff = r; // if r is true, show that row
                                                                    // val = case insensitive, v[i] = case sensitive
                                                                    val = wo.filter_ignoreCase ? v[i].toLocaleLowerCase() : v[i];
                                                                    if (wo.filter_functions && wo.filter_functions[i]){
                                                                        if (wo.filter_functions[i] === true){
                                                                            // default selector; no "filter-select" class
                                                                            ff = ($ths.filter('[data-column="' + i + '"]:last').hasClass('filter-match')) ? xi.search(val) >= 0 : v[i] === x;
                                                                        } else if (typeof wo.filter_functions[i] === 'function'){
                                                                            // filter callback( exact cell content, parser normalized content, filter input value, column index )
                                                                            ff = wo.filter_functions[i](x, c.cache[k].normalized[j][i], v[i], i);
                                                                        } else if (typeof wo.filter_functions[i][v[i]] === 'function'){
                                                                            // selector option function
                                                                            ff = wo.filter_functions[i][v[i]](x, c.cache[k].normalized[j][i], v[i], i);
                                                                        }
                                                                        // Look for regex
                                                                    } else if (reg[0].test(val)){
                                                                        rg = reg[0].exec(val);
                                                                        try {
                                                                            ff = new RegExp(rg[1], rg[2]).test(xi);
                                                                        } catch (err){
                                                                            ff = false;
                                                                        }
                                                                        // Look for quotes or equals to get an exact match
                                                                    } else if (reg[3].test(val) && xi === val.replace(reg[4], '')){
                                                                        ff = true;
                                                                        // Look for a not match
                                                                    } else if (/^\!/.test(val)){
                                                                        val = val.replace('!','');
                                                                        s = xi.search($.trim(val));
                                                                        ff = val === '' ? true : !(wo.filter_startsWith ? s === 0 : s >= 0);
                                                                        // Look for operators >, >=, < or <=
                                                                    } else if (/^[<>]=?/.test(val)){
                                                                        // xi may be numeric - see issue #149
                                                                        rg = isNaN(xi) ? $.tablesorter.formatFloat(xi.replace(reg[5], ''), table) : $.tablesorter.formatFloat(xi, table);
                                                                        s = $.tablesorter.formatFloat(val.replace(reg[5], '').replace(reg[6],''), table);
                                                                        if (/>/.test(val)) { ff = />=/.test(val) ? rg >= s : rg > s; }
                                                                        if (/</.test(val)) { ff = /<=/.test(val) ? rg <= s : rg < s; }
                                                                        // Look for wild card: ? = single, or * = multiple
                                                                    } else if (/[\?|\*]/.test(val)){
                                                                        ff = new RegExp( val.replace(/\?/g, '\\S{1}').replace(/\*/g, '\\S*') ).test(xi);
                                                                        // Look for match, and add child row data for matching
                                                                    } else {
                                                                        x = (xi + t).indexOf(val);
                                                                        ff  = ( (!wo.filter_startsWith && x >= 0) || (wo.filter_startsWith && x === 0) );
                                                                    }
                                                                    r = (ff) ? (r ? true : false) : false;
                                                                }
                                                            }
                                                            $tr[j].style.display = (r ? '' : 'none');
                                                            $tr.eq(j)[r ? 'removeClass' : 'addClass']('filtered');
                                                            if (cr.length) { cr[r ? 'show' : 'hide'](); }
                                                        }
                                                    }
                                                    $.tablesorter.processTbody(table, $tb, false);
                                                }

                                                last = cv; // save last search
                                                if (c.debug){
                                                    ts.benchmark("Completed filter widget search", time);
                                                }
                                                $t.trigger('applyWidgets'); // make sure zebra widget is applied
                                                $t.trigger('filterEnd');
                                            },
                                            buildSelect = function(i, updating){
                                                var o, arry = [];
                                                i = parseInt(i, 10);
                                                o = '<option value="">' + ($ths.filter('[data-column="' + i + '"]:last').attr('data-placeholder') || '') + '</option>';
                                                for (k = 0; k < b.length; k++ ){
                                                    l = c.cache[k].row.length;
                                                    // loop through the rows
                                                    for (j = 0; j < l; j++){
                                                        // get non-normalized cell content
                                                        if (wo.filter_useParsedData){
                                                            arry.push( '' + c.cache[k].normalized[j][i] );
                                                        } else {
                                                            t = c.cache[k].row[j][0].cells[i];
                                                            if (t){
                                                                arry.push( $.trim(c.supportsTextContent ? t.textContent : $(t).text()) );
                                                            }
                                                        }
                                                    }
                                                }

                                                // get unique elements and sort the list
                                                // if $.tablesorter.sortText exists (not in the original tablesorter),
                                                // then natural sort the list otherwise use a basic sort
                                                arry = $.grep(arry, function(v, k){
                                                    return $.inArray(v ,arry) === k;
                                                });
                                                arry = (ts.sortText) ? arry.sort(function(a,b){ return ts.sortText(table, a, b, i); }) : arry.sort(true);

                                                // build option list
                                                for (k = 0; k < arry.length; k++){
                                                    o += '<option value="' + arry[k] + '">' + arry[k] + '</option>';
                                                }
                                                $t.find('thead').find('select.' + css + '[data-column="' + i + '"]')[ updating ? 'html' : 'append' ](o);
                                            },
                                            buildDefault = function(updating){
                                                // build default select dropdown
                                                for (i = 0; i < cols; i++){
                                                    t = $ths.filter('[data-column="' + i + '"]:last');
                                                    // look for the filter-select class; build/update it if found
                                                    if ((t.hasClass('filter-select') || wo.filter_functions && wo.filter_functions[i] === true) && !t.hasClass('filter-false')){
                                                        if (!wo.filter_functions) { wo.filter_functions = {}; }
                                                        wo.filter_functions[i] = true; // make sure this select gets processed by filter_functions
                                                        buildSelect(i, updating);
                                                    }
                                                }
                                            };

                                        if (c.debug){
                                            time = new Date();
                                        }
                                        wo.filter_ignoreCase = wo.filter_ignoreCase !== false; // set default filter_ignoreCase to true
                                        wo.filter_useParsedData = wo.filter_useParsedData === true; // default is false
                                        // don't build filter row if columnFilters is false or all columns are set to "filter-false" - issue #156
                                        if (wo.filter_columnFilters !== false && $ths.filter('.filter-false').length !== $ths.length){
                                            t = '<tr class="tablesorter-filter-row">'; // build filter row
                                            for (i = 0; i < cols; i++){
                                                dis = false;
                                                $th = $ths.filter('[data-column="' + i + '"]:last'); // assuming last cell of a column is the main column
                                                sel = (wo.filter_functions && wo.filter_functions[i] && typeof wo.filter_functions[i] !== 'function') || $th.hasClass('filter-select');
                                                t += '<td>';
                                                if (sel){
                                                    t += '<select data-column="' + i + '" class="' + css;
                                                } else {
                                                    t += '<input type="search" placeholder="' + ($th.attr('data-placeholder') || "") + '" data-column="' + i + '" class="' + css;
                                                }
                                                // use header option - headers: { 1: { filter: false } } OR add class="filter-false"
                                                if (ts.getData){
                                                    dis = ts.getData($th[0], c.headers[i], 'filter') === 'false';
                                                    // get data from jQuery data, metadata, headers option or header class name
                                                    t += dis ? ' disabled" disabled' : '"';
                                                } else {
                                                    dis = (c.headers[i] && c.headers[i].hasOwnProperty('filter') && c.headers[i].filter === false) || $th.hasClass('filter-false');
                                                    // only class names and header options - keep this for compatibility with tablesorter v2.0.5
                                                    t += (dis) ? ' disabled" disabled' : '"';
                                                }
                                                t += (sel ? '></select>' : '>') + '</td>';
                                            }
                                            $t.find('thead').eq(0).append(t += '</tr>');
                                        }
                                        $t
                                            // add .tsfilter namespace to all BUT search
                                            .bind('addRows updateCell update appendCache search'.split(' ').join('.tsfilter '), function(e, filter){
                                                      if (e.type !== 'search'){
                                                          buildDefault(true);
                                                      }
                                                      checkFilters(e.type === 'search' ? filter : '');
                                                      return false;
                                                  })
                                            .find('input.' + css).bind('keyup search', function(e, filter){
                                                                           // ignore arrow and meta keys; allow backspace
                                                                           if ((e.which < 32 && e.which !== 8) || (e.which >= 37 && e.which <=40)) { return; }
                                                                           // skip delay
                                                                           if (typeof filter !== 'undefined'){
                                                                               checkFilters(filter);
                                                                               return false;
                                                                           }
                                                                           // delay filtering
                                                                           clearTimeout(timer);
                                                                           timer = setTimeout(function(){
                                                                               checkFilters();
                                                                           }, wo.filter_searchDelay || 300);
                                                                       });

                                        // reset button/link
                                        if (wo.filter_reset && $(wo.filter_reset).length){
                                            $(wo.filter_reset).bind('click', function(){
                                                $t.find('.' + css).val('');
                                                checkFilters();
                                                return false;
                                            });
                                        }
                                        if (wo.filter_functions){
                                            // i = column # (string)
                                            for (col in wo.filter_functions){
                                                if (wo.filter_functions.hasOwnProperty(col) && typeof col === 'string'){
                                                    t = $ths.filter('[data-column="' + col + '"]:last');
                                                    ff = '';
                                                    if (wo.filter_functions[col] === true && !t.hasClass('filter-false')){
                                                        buildSelect(col);
                                                    } else if (typeof col === 'string' && !t.hasClass('filter-false')){
                                                        // add custom drop down list
                                                        for (str in wo.filter_functions[col]){
                                                            if (typeof str === 'string'){
                                                                ff += ff === '' ? '<option value="">' + (t.attr('data-placeholder') || '') + '</option>' : '';
                                                                ff += '<option value="' + str + '">' + str + '</option>';
                                                            }
                                                        }
                                                        $t.find('thead').find('select.' + css + '[data-column="' + col + '"]').append(ff);
                                                    }
                                                }
                                            }
                                        }
                                        // not really updating, but if the column has both the "filter-select" class & filter_functions set to true,
                                        // it would append the same options twice.
                                        buildDefault(true);

                                        $t.find('select.' + css).bind('change search', function(){
                                            checkFilters();
                                        });

                                        if (wo.filter_hideFilters === true){
                                            $t
                                                .find('.tablesorter-filter-row')
                                                .addClass('hideme')
                                                .bind('mouseenter mouseleave', function(e){
                                                          // save event object - http://bugs.jquery.com/ticket/12140
                                                          var all, evt = e;
                                                          ft = $(this);
                                                          clearTimeout(st);
                                                          st = setTimeout(function(){
                                                              if (/enter|over/.test(evt.type)){
                                                                  ft.removeClass('hideme');
                                                              } else {
                                                                  // don't hide if input has focus
                                                                  // $(':focus') needs jQuery 1.6+
                                                                  if ($(document.activeElement).closest('tr')[0] !== ft[0]){
                                                                      // get all filter values
                                                                      all = $t.find('.' + (wo.filter_cssFilter || 'tablesorter-filter')).map(function(){
                                                                          return $(this).val() || '';
                                                                      }).get().join('');
                                                                      // don't hide row if any filter has a value
                                                                      if (all === ''){
                                                                          ft.addClass('hideme');
                                                                      }
                                                                  }
                                                              }
                                                          }, 200);
                                                      })
                                                .find('input, select').bind('focus blur', function(e){
                                                                                ft2 = $(this).closest('tr');
                                                                                clearTimeout(st);
                                                                                st = setTimeout(function(){
                                                                                    // don't hide row if any filter has a value
                                                                                    if ($t.find('.' + (wo.filter_cssFilter || 'tablesorter-filter')).map(function(){ return $(this).val() || ''; }).get().join('') === ''){
                                                                                        ft2[ e.type === 'focus' ? 'removeClass' : 'addClass']('hideme');
                                                                                    }
                                                                                }, 200);
                                                                            });
                                        }

                                        // show processing icon
                                        if (c.showProcessing) {
                                            $t.bind('filterStart filterEnd', function(e, v) {
                                                var fc = (v) ? $t.find('.' + c.cssHeader).filter('[data-column]').filter(function(){
                                                    return v[$(this).data('column')] !== '';
                                                }) : '';
                                                ts.isProcessing($t[0], e.type === 'filterStart', v ? fc : '');
                                            });
                                        }

                                        if (c.debug){
                                            ts.benchmark("Applying Filter widget", time);
                                        }
                                        // filter widget initialized
                                        $t.trigger('filterInit');
                                    }
                                },
                                remove: function(table, c, wo){
                                    var k, $tb,
                                        $t = $(table),
                                        b = c.$tbodies;
                                    $t
                                        .removeClass('hasFilters')
                                        // add .tsfilter namespace to all BUT search
                                        .unbind('addRows updateCell update appendCache search'.split(' ').join('.tsfilter'))
                                        .find('.tablesorter-filter-row').remove();
                                    for (k = 0; k < b.length; k++ ){
                                        $tb = $.tablesorter.processTbody(table, b.eq(k), true); // remove tbody
                                        $tb.children().removeClass('filtered').show();
                                        $.tablesorter.processTbody(table, $tb, false); // restore tbody
                                    }
                                    if (wo.filterreset) { $(wo.filter_reset).unbind('click'); }
                                }
                            });

    // Widget: Sticky headers
    // based on this awesome article:
    // http://css-tricks.com/13465-persistent-headers/
    // and https://github.com/jmosbech/StickyTableHeaders by Jonas Mosbech
    // **************************
    $.tablesorter.addWidget({
                                id: "stickyHeaders",
                                format: function(table){
                                    if ($(table).hasClass('hasStickyHeaders')) { return; }
                                    var $table = $(table).addClass('hasStickyHeaders'),
                                        c = table.config,
                                        wo = c.widgetOptions,
                                        win = $(window),
                                        header = $(table).children('thead:first'), //.add( $(table).find('caption') ),
                                        hdrCells = header.children('tr:not(.sticky-false)').children(),
                                        css = wo.stickyHeaders || 'tablesorter-stickyHeader',
                                        innr = '.tablesorter-header-inner',
                                        firstRow = hdrCells.eq(0).parent(),
                                        tfoot = $table.find('tfoot'),
                                        t2 = wo.$sticky = $table.clone(), // clone table, but don't remove id... the table might be styled by css
                                    // clone the entire thead - seems to work in IE8+
                                        stkyHdr = t2.children('thead:first')
                                            .addClass(css)
                                            .css({
                                                     width      : header.outerWidth(true),
                                                     position   : 'fixed',
                                                     margin     : 0,
                                                     top        : 0,
                                                     visibility : 'hidden',
                                                     zIndex     : 1
                                                 }),
                                        stkyCells = stkyHdr.children('tr:not(.sticky-false)').children(), // issue #172
                                        laststate = '',
                                        spacing = 0,
                                        resizeHdr = function(){
                                            var bwsr = navigator.userAgent;
                                            spacing = 0;
                                            // yes, I dislike browser sniffing, but it really is needed here :(
                                            // webkit automatically compensates for border spacing
                                            if ($table.css('border-collapse') !== 'collapse' && !/(webkit|msie)/i.test(bwsr)) {
                                                // Firefox & Opera use the border-spacing
                                                // update border-spacing here because of demos that switch themes
                                                spacing = parseInt(hdrCells.eq(0).css('border-left-width'), 10) * 2;
                                            }
                                            stkyHdr.css({
                                                            left : header.offset().left - win.scrollLeft() - spacing,
                                                            width: header.outerWidth()
                                                        });
                                            stkyCells
                                                .each(function(i){
                                                          var $h = hdrCells.eq(i);
                                                          $(this).css({
                                                                          width: $h.width() - spacing,
                                                                          height: $h.height()
                                                                      });
                                                      })
                                                .find(innr).each(function(i){
                                                                     var hi = hdrCells.eq(i).find(innr),
                                                                         w = hi.width(); // - ( parseInt(hi.css('padding-left'), 10) + parseInt(hi.css('padding-right'), 10) );
                                                                     $(this).width(w);
                                                                 });
                                        };
                                    // clear out cloned table, except for sticky header
                                    t2.find('thead:gt(0),tr.sticky-false,tbody,tfoot,caption').remove();
                                    t2.css({ height:0, width:0, padding:0, margin:0, border:0 });
                                    // remove rows you don't want to be sticky
                                    stkyHdr.find('tr.sticky-false').remove();
                                    // remove resizable block
                                    stkyCells.find('.tablesorter-resizer').remove();
                                    // update sticky header class names to match real header after sorting
                                    $table
                                        .bind('sortEnd.tsSticky', function(){
                                                  hdrCells.each(function(i){
                                                      var t = stkyCells.eq(i);
                                                      t.attr('class', $(this).attr('class'));
                                                      if (c.cssIcon){
                                                          t
                                                              .find('.' + c.cssIcon)
                                                              .attr('class', $(this).find('.' + c.cssIcon).attr('class'));
                                                      }
                                                  });
                                              })
                                        .bind('pagerComplete.tsSticky', function(){
                                                  resizeHdr();
                                              });
                                    // set sticky header cell width and link clicks to real header
                                    hdrCells.find('*').andSelf().filter(c.selectorSort).each(function(i){
                                        var t = $(this);
                                        stkyCells.eq(i)
                                            // clicking on sticky will trigger sort
                                            .bind('mouseup', function(e){
                                                      t.trigger(e, true); // external mouseup flag (click timer is ignored)
                                                  })
                                            // prevent sticky header text selection
                                            .bind('mousedown', function(){
                                                      this.onselectstart = function(){ return false; };
                                                      return false;
                                                  });
                                    });
                                    // add stickyheaders AFTER the table. If the table is selected by ID, the original one (first) will be returned.
                                    $table.after( t2 );
                                    // make it sticky!
                                    win
                                        .bind('scroll.tsSticky', function(){
                                                  var offset = firstRow.offset(),
                                                      sTop = win.scrollTop(),
                                                      tableHt = $table.height() - (stkyHdr.height() + (tfoot.height() || 0)),
                                                      vis = (sTop > offset.top) && (sTop < offset.top + tableHt) ? 'visible' : 'hidden';
                                                  stkyHdr
                                                      .css({
                                                               // adjust when scrolling horizontally - fixes issue #143
                                                               left : header.offset().left - win.scrollLeft() - spacing,
                                                               visibility : vis
                                                           });
                                                  if (vis !== laststate){
                                                      // make sure the column widths match
                                                      resizeHdr();
                                                      laststate = vis;
                                                  }
                                              })
                                        .bind('resize.tsSticky', function(){
                                                  resizeHdr();
                                              });
                                },
                                remove: function(table, c, wo){
                                    var $t = $(table),
                                        css = wo.stickyHeaders || 'tablesorter-stickyHeader';
                                    $t
                                        .removeClass('hasStickyHeaders')
                                        .unbind('sortEnd.tsSticky pagerComplete.tsSticky')
                                        .find('.' + css).remove();
                                    if (wo.$sticky) { wo.$sticky.remove(); } // remove cloned thead
                                    $(window).unbind('scroll.tsSticky resize.tsSticky');
                                }
                            });

    // Add Column resizing widget
    // this widget saves the column widths if
    // $.tablesorter.storage function is included
    // **************************
    $.tablesorter.addWidget({
                                id: "resizable",
                                format: function(table){
                                    if ($(table).hasClass('hasResizable')) { return; }
                                    $(table).addClass('hasResizable');
                                    var $t, t, i, j, s, $c, $cols, w, tw,
                                        $tbl = $(table),
                                        c = table.config,
                                        wo = c.widgetOptions,
                                        position = 0,
                                        $target = null,
                                        $next = null,
                                        fullWidth = Math.abs($tbl.parent().width() - $tbl.width()) < 20,
                                        stopResize = function(){
                                            if ($.tablesorter.storage && $target){
                                                s[$target.index()] = $target.width();
                                                s[$next.index()] = $next.width();
                                                $target.width( s[$target.index()] );
                                                $next.width( s[$next.index()] );
                                                if (wo.resizable !== false){
                                                    $.tablesorter.storage(table, 'tablesorter-resizable', s);
                                                }
                                            }
                                            position = 0;
                                            $target = $next = null;
                                            $(window).trigger('resize'); // will update stickyHeaders, just in case
                                        };
                                    s = ($.tablesorter.storage && wo.resizable !== false) ? $.tablesorter.storage(table, 'tablesorter-resizable') : {};
                                    // process only if table ID or url match
                                    if (s){
                                        for (j in s){
                                            if (!isNaN(j) && j < c.headerList.length){
                                                $(c.headerList[j]).width(s[j]); // set saved resizable widths
                                            }
                                        }
                                    }
                                    $t = $tbl.children('thead:first').children('tr');
                                    // add resizable-false class name to headers (across rows as needed)
                                    $t.children().each(function(){
                                        t = $(this);
                                        i = t.attr('data-column');
                                        j = $.tablesorter.getData( t, c.headers[i], 'resizable') === "false";
                                        $t.children().filter('[data-column="' + i + '"]').toggleClass('resizable-false', j);
                                    });
                                    // add wrapper inside each cell to allow for positioning of the resizable target block
                                    $t.each(function(){
                                        $c = $(this).children(':not(.resizable-false)');
                                        if (!$(this).find('.tablesorter-wrapper').length) {
                                            // Firefox needs this inner div to position the resizer correctly
                                            $c.wrapInner('<div class="tablesorter-wrapper" style="position:relative;height:100%;width:100%"></div>');
                                        }
                                        $c = $c.slice(0,-1); // don't include the last column of the row
                                        $cols = $cols ? $cols.add($c) : $c;
                                    });
                                    $cols
                                        .each(function(){
                                                  $t = $(this);
                                                  j = parseInt($t.css('padding-right'), 10) + 10; // 8 is 1/2 of the 16px wide resizer grip
                                                  t = '<div class="tablesorter-resizer" style="cursor:w-resize;position:absolute;z-index:1;right:-' + j +
                                                      'px;top:0;height:100%;width:20px;"></div>';
                                                  $t
                                                      .find('.tablesorter-wrapper')
                                                      .append(t);
                                              })
                                        .bind('mousemove.tsresize', function(e){
                                                  // ignore mousemove if no mousedown
                                                  if (position === 0 || !$target) { return; }
                                                  // resize columns
                                                  w = e.pageX - position;
                                                  tw = $target.width();
                                                  $target.width( tw + w );
                                                  if ($target.width() !== tw && fullWidth){
                                                      $next.width( $next.width() - w );
                                                  }
                                                  position = e.pageX;
                                              })
                                        .bind('mouseup.tsresize', function(){
                                                  stopResize();
                                              })
                                        .find('.tablesorter-resizer,.tablesorter-resizer-grip')
                                        .bind('mousedown', function(e){
                                                  // save header cell and mouse position; closest() not supported by jQuery v1.2.6
                                                  $target = $(e.target).closest('th');
                                                  t = c.$headers.filter('[data-column="' + $target.attr('data-column') + '"]');
                                                  if (t.length > 1) { $target = $target.add(t); }
                                                  // if table is not as wide as it's parent, then resize the table
                                                  $next = e.shiftKey ? $target.parent().find('th:not(.resizable-false)').filter(':last') : $target.nextAll(':not(.resizable-false)').eq(0);
                                                  position = e.pageX;
                                              });
                                    $tbl.find('thead:first')
                                        .bind('mouseup.tsresize mouseleave.tsresize', function(e){
                                                  stopResize();
                                              })
                                        // right click to reset columns to default widths
                                        .bind('contextmenu.tsresize', function(){
                                                  $.tablesorter.resizableReset(table);
                                                  // $.isEmptyObject() needs jQuery 1.4+
                                                  var rtn = $.isEmptyObject ? $.isEmptyObject(s) : s === {}; // allow right click if already reset
                                                  s = {};
                                                  return rtn;
                                              });
                                },
                                remove: function(table, c, wo){
                                    $(table)
                                        .removeClass('hasResizable')
                                        .find('thead')
                                        .unbind('mouseup.tsresize mouseleave.tsresize contextmenu.tsresize')
                                        .find('tr').children()
                                        .unbind('mousemove.tsresize mouseup.tsresize')
                                        // don't remove "tablesorter-wrapper" as uitheme uses it too
                                        .find('.tablesorter-resizer,.tablesorter-resizer-grip').remove();
                                    $.tablesorter.resizableReset(table);
                                }
                            });
    $.tablesorter.resizableReset = function(table){
        $(table.config.headerList).filter(':not(.resizable-false)').css('width','');
        if ($.tablesorter.storage) { $.tablesorter.storage(table, 'tablesorter-resizable', {}); }
    };

    // Save table sort widget
    // this widget saves the last sort only if the
    // saveSort widget option is true AND the
    // $.tablesorter.storage function is included
    // **************************
    $.tablesorter.addWidget({
                                id: 'saveSort',
                                init: function(table, thisWidget){
                                    // run widget format before all other widgets are applied to the table
                                    thisWidget.format(table, true);
                                },
                                format: function(table, init){
                                    var sl, time, c = table.config,
                                        wo = c.widgetOptions,
                                        ss = wo.saveSort !== false, // make saveSort active/inactive; default to true
                                        sortList = { "sortList" : c.sortList };
                                    if (c.debug){
                                        time = new Date();
                                    }
                                    if ($(table).hasClass('hasSaveSort')){
                                        if (ss && table.hasInitialized && $.tablesorter.storage){
                                            $.tablesorter.storage( table, 'tablesorter-savesort', sortList );
                                            if (c.debug){
                                                $.tablesorter.benchmark('saveSort widget: Saving last sort: ' + c.sortList, time);
                                            }
                                        }
                                    } else {
                                        // set table sort on initial run of the widget
                                        $(table).addClass('hasSaveSort');
                                        sortList = '';
                                        // get data
                                        if ($.tablesorter.storage){
                                            sl = $.tablesorter.storage( table, 'tablesorter-savesort' );
                                            sortList = (sl && sl.hasOwnProperty('sortList') && $.isArray(sl.sortList)) ? sl.sortList : '';
                                            if (c.debug){
                                                $.tablesorter.benchmark('saveSort: Last sort loaded: "' + sortList + '"', time);
                                            }
                                        }
                                        // init is true when widget init is run, this will run this widget before all other widgets have initialized
                                        // this method allows using this widget in the original tablesorter plugin; but then it will run all widgets twice.
                                        if (init && sortList && sortList.length > 0){
                                            c.sortList = sortList;
                                        } else if (table.hasInitialized && sortList && sortList.length > 0){
                                            // update sort change
                                            $(table).trigger('sorton', [sortList]);
                                        }
                                    }
                                },
                                remove: function(table, c, wo){
                                    // clear storage
                                    if ($.tablesorter.storage) { $.tablesorter.storage( table, 'tablesorter-savesort', '' ); }
                                }
                            });

})(jQuery);