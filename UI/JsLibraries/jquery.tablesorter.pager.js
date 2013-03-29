/*!
 * tablesorter pager plugin
 * updated 2/20/2013
 */
/*jshint browser:true, jquery:true, unused:false */
;(function($) {
    "use strict";
    /*jshint supernew:true */
    $.extend({ tablesorterPager: new function() {

        this.defaults = {
            // target the pager markup
            container: null,

            // use this format: "http://mydatabase.com?page={page}&size={size}&{sortList:col}&{filterList:fcol}"
            // where {page} is replaced by the page number, {size} is replaced by the number of records to show,
            // {sortList:col} adds the sortList to the url into a "col" array, and {filterList:fcol} adds
            // the filterList to the url into an "fcol" array.
            // So a sortList = [[2,0],[3,0]] becomes "&col[2]=0&col[3]=0" in the url
            // and a filterList = [[2,Blue],[3,13]] becomes "&fcol[2]=Blue&fcol[3]=13" in the url
            ajaxUrl: null,

            // process ajax so that the following information is returned:
            // [ total_rows (number), rows (array of arrays), headers (array; optional) ]
            // example:
            // [
            //   100,  // total rows
            //   [
            //     [ "row1cell1", "row1cell2", ... "row1cellN" ],
            //     [ "row2cell1", "row2cell2", ... "row2cellN" ],
            //     ...
            //     [ "rowNcell1", "rowNcell2", ... "rowNcellN" ]
            //   ],
            //   [ "header1", "header2", ... "headerN" ] // optional
            // ]
            ajaxProcessing: function(ajax){ return [ 0, [], null ]; },

            // output default: '{page}/{totalPages}'
            // possible variables: {page}, {totalPages}, {filteredPages}, {startRow}, {endRow}, {filteredRows} and {totalRows}
            output: '{startRow} to {endRow} of {totalRows} rows', // '{page}/{totalPages}'

            // apply disabled classname to the pager arrows when the rows at either extreme is visible
            updateArrows: true,

            // starting page of the pager (zero based index)
            page: 0,

            // Number of visible rows
            size: 10,

            // if true, the table will remain the same height no matter how many records are displayed. The space is made up by an empty
            // table row set to a height to compensate; default is false
            fixedHeight: false,

            // remove rows from the table to speed up the sort of large tables.
            // setting this to false, only hides the non-visible rows; needed if you plan to add/remove rows with the pager enabled.
            removeRows: false, // removing rows in larger tables speeds up the sort

            // css class names of pager arrows
            cssFirst: '.first', // go to first page arrow
            cssPrev: '.prev', // previous page arrow
            cssNext: '.next', // next page arrow
            cssLast: '.last', // go to last page arrow
            cssGoto: '.gotoPage', // go to page selector - select dropdown that sets the current page
            cssPageDisplay: '.pagedisplay', // location of where the "output" is displayed
            cssPageSize: '.pagesize', // page size selector - select dropdown that sets the "size" option
            cssErrorRow: 'tablesorter-errorRow', // error information row

            // class added to arrows when at the extremes (i.e. prev/first arrows are "disabled" when on the first page)
            cssDisabled: 'disabled', // Note there is no period "." in front of this class name

            // stuff not set by the user
            totalRows: 0,
            totalPages: 0,
            filteredRows: 0,
            filteredPages: 0

        };

        var $this = this,

        // hide arrows at extremes
            pagerArrows = function(c, disable) {
                var a = 'addClass',
                    r = 'removeClass',
                    d = c.cssDisabled,
                    dis = !!disable,
                    tp = Math.min( c.totalPages, c.filteredPages );
                if ( c.updateArrows ) {
                    $(c.cssFirst + ',' + c.cssPrev, c.container)[ ( dis || c.page === 0 ) ? a : r ](d);
                    $(c.cssNext + ',' + c.cssLast, c.container)[ ( dis || c.page === tp - 1 ) ? a : r ](d);
                }
            },

            updatePageDisplay = function(table, c) {
                var i, p, s, t, out, f = $(table).hasClass('hasFilters') && !c.ajaxUrl;
                c.filteredRows = (f) ? table.config.$tbodies.children('tr:not(.filtered,.remove-me)').length : c.totalRows;
                c.filteredPages = (f) ? Math.ceil( c.filteredRows / c.size ) : c.totalPages;
                if ( Math.min( c.totalPages, c.filteredPages ) > 0 ) {
                    t = (c.size * c.page > c.filteredRows);
                    c.startRow = (t) ? 1 : ( c.size * c.page ) + 1;
                    c.page = (t) ? 0 : c.page;
                    c.endRow = Math.min( c.filteredRows, c.totalRows, c.size * ( c.page + 1 ) );
                    out = $(c.cssPageDisplay, c.container);
                    // form the output string
                    s = c.output.replace(/\{(page|filteredRows|filteredPages|totalPages|startRow|endRow|totalRows)\}/gi, function(m){
                        return {
                            '{page}'            : c.page + 1,
                            '{filteredRows}'    : c.filteredRows,
                            '{filteredPages}'   : c.filteredPages,
                            '{totalPages}'      : c.totalPages,
                            '{startRow}'        : c.startRow,
                            '{endRow}'          : c.endRow,
                            '{totalRows}'       : c.totalRows
                        }[m];
                    });
                    if (out[0]) {
                        out[ (out[0].tagName === 'INPUT') ? 'val' : 'html' ](s);
                        if ( $(c.cssGoto, c.container).length ) {
                            t = '';
                            p = Math.min( c.totalPages, c.filteredPages );
                            for ( i = 1; i <= p; i++ ) {
                                t += '<option>' + i + '</option>';
                            }
                            $(c.cssGoto, c.container).html(t).val(c.page + 1);
                        }
                    }
                }
                pagerArrows(c);
                if (c.initialized) { $(table).trigger('pagerComplete', c); }
            },

            fixHeight = function(table, c) {
                var d, h, $b = $(table.tBodies[0]);
                if (c.fixedHeight) {
                    $b.find('tr.pagerSavedHeightSpacer').remove();
                    h = $.data(table, 'pagerSavedHeight');
                    if (h) {
                        d = h - $b.height();
                        if ( d > 5 && $.data(table, 'pagerLastSize') === c.size && $b.children('tr:visible').length < c.size ) {
                            $b.append('<tr class="pagerSavedHeightSpacer ' + table.config.selectorRemove.replace(/(tr)?\./g,'') + '" style="height:' + d + 'px;"></tr>');
                        }
                    }
                }
            },

            changeHeight = function(table, c) {
                var $b = $(table.tBodies[0]);
                $b.find('tr.pagerSavedHeightSpacer').remove();
                $.data(table, 'pagerSavedHeight', $b.height());
                fixHeight(table, c);
                $.data(table, 'pagerLastSize', c.size);
            },

            hideRows = function(table, c){
                if (!c.ajaxUrl) {
                    var i,
                        rows = $(table.tBodies).children('tr:not(.' + table.config.cssChildRow + ')'),
                        l = rows.length,
                        s = ( c.page * c.size ),
                        e =  s + c.size,
                        j = 0; // size counter
                    for ( i = 0; i < l; i++ ){
                        if (!/filtered/.test(rows[i].className)) {
                            rows[i].style.display = ( j >= s && j < e ) ? '' : 'none';
                            j++;
                        }
                    }
                }
            },

            hideRowsSetup = function(table, c){
                c.size = parseInt( $(c.cssPageSize, c.container).find('option[selected]').val(), 10 ) || c.size;
                $.data(table, 'pagerLastSize', c.size);
                pagerArrows(c);
                if ( !c.removeRows ) {
                    hideRows(table, c);
                    $(table).bind('sortEnd.pager filterEnd.pager', function(){
                        hideRows(table, c);
                    });
                }
            },

            renderAjax = function(data, table, c, exception){
                // process data
                if ( typeof(c.ajaxProcessing) === "function" ) {
                    // ajaxProcessing result: [ total, rows, headers ]
                    var i, j, hsh, $f, $sh,
                        $t = $(table),
                        tc = table.config,
                        hl = $t.find('thead th').length, tds = '',
                        err = '<tr class="' + c.cssErrorRow + ' ' + tc.selectorRemove.replace(/(tr)?\./g,'') + '"><td style="text-align: center;" colspan="' + hl + '">' +
                            (exception ? exception.message + ' (' + exception.name + ')' : 'No rows found') + '</td></tr>',
                        result = c.ajaxProcessing(data) || [ 0, [] ],
                        d = result[1] || [],
                        l = d.length,
                        th = result[2];
                    if ( l > 0 ) {
                        for ( i = 0; i < l; i++ ) {
                            tds += '<tr>';
                            for ( j = 0; j < d[i].length; j++ ) {
                                // build tbody cells
                                tds += '<td>' + d[i][j] + '</td>';
                            }
                            tds += '</tr>';
                        }
                    }
                    // only add new header text if the length matches
                    if ( th && th.length === hl ) {
                        hsh = $t.hasClass('hasStickyHeaders');
                        $sh = $t.find('.' + ((tc.widgetOptions && tc.widgetOptions.stickyHeaders) || 'tablesorter-stickyheader'));
                        $f = $t.find('tfoot tr:first').children();
                        $t.find('th.' + tc.cssHeader).each(function(j){
                            var $t = $(this), icn;
                            // add new test within the first span it finds, or just in the header
                            if ( $t.find('.' + tc.cssIcon).length ) {
                                icn = $t.find('.' + tc.cssIcon).clone(true);
                                $t.find('.tablesorter-header-inner').html( th[j] ).append(icn);
                                if ( hsh && $sh.length ) {
                                    icn = $sh.find('th').eq(j).find('.' + tc.cssIcon).clone(true);
                                    $sh.find('th').eq(j).find('.tablesorter-header-inner').html( th[j] ).append(icn);
                                }
                            } else {
                                $t.find('.tablesorter-header-inner').html( th[j] );
                                $sh.find('th').eq(j).find('.tablesorter-header-inner').html( th[j] );
                            }
                            $f.eq(j).html( th[j] );
                        });
                    }

                    $t.find('thead tr.' + c.cssErrorRow).remove(); // Clean up any previous error.
                    if ( exception ) {
                        // add error row to thead instead of tbody, or clicking on the header will result in a parser error
                        $t.find('thead').append(err);
                    } else {
                        $(table.tBodies[0]).html( tds ); // add rows to first tbody
                    }
                    if (tc.showProcessing) {
                        $.tablesorter.isProcessing(table); // remove loading icon
                    }
                    $t.trigger('update');
                    c.totalRows = result[0] || 0;
                    c.totalPages = Math.ceil( c.totalRows / c.size );
                    updatePageDisplay(table, c);
                    fixHeight(table, c);
                    if (c.initialized) { $t.trigger('pagerChange', c); }
                }
                if (!c.initialized) {
                    c.initialized = true;
                    $(table).trigger('pagerInitialized', c);
                }
            },

            getAjax = function(table, c){
                var url = getAjaxUrl(table, c),
                    tc = table.config;
                if ( url !== '' ) {
                    if (tc.showProcessing) {
                        $.tablesorter.isProcessing(table, true); // show loading icon
                    }
                    $(document).bind('ajaxError.pager', function(e, xhr, settings, exception) {
                        if (settings.url === url) {
                            renderAjax(null, table, c, exception);
                            $(document).unbind('ajaxError.pager');
                        }
                    });
                    $.getJSON(url, function(data) {
                        renderAjax(data, table, c);
                        $(document).unbind('ajaxError.pager');
                    });
                }
            },

            getAjaxUrl = function(table, c) {
                var url = (c.ajaxUrl) ? c.ajaxUrl.replace(/\{page\}/g, c.page).replace(/\{size\}/g, c.size) : '',
                    sl = table.config.sortList,
                    fl = c.currentFilters || [],
                    sortCol = url.match(/\{sortList[\s+]?:[\s+]?([^}]*)\}/),
                    filterCol = url.match(/\{filterList[\s+]?:[\s+]?([^}]*)\}/),
                    arry = [];
                if (sortCol) {
                    sortCol = sortCol[1];
                    $.each(sl, function(i,v){
                        arry.push(sortCol + '[' + v[0] + ']=' + v[1]);
                    });
                    // if the arry is empty, just add the col parameter... "&{sortList:col}" becomes "&col"
                    url = url.replace(/\{sortList[\s+]?:[\s+]?([^\}]*)\}/g, arry.length ? arry.join('&') : sortCol );
                }
                if (filterCol) {
                    filterCol = filterCol[1];
                    $.each(fl, function(i,v){
                        if (v) {
                            arry.push(filterCol + '[' + i + ']=' + encodeURIComponent(v));
                        }
                    });
                    // if the arry is empty, just add the fcol parameter... "&{filterList:fcol}" becomes "&fcol"
                    url = url.replace(/\{filterList[\s+]?:[\s+]?([^\}]*)\}/g, arry.length ? arry.join('&') : filterCol );
                }

                return url;
            },

            renderTable = function(table, rows, c) {
                c.isDisabled = false; // needed because sorting will change the page and re-enable the pager
                var i, j, o,
                    f = document.createDocumentFragment(),
                    l = rows.length,
                    s = ( c.page * c.size ),
                    e = ( s + c.size );
                if ( l < 1 ) { return; } // empty table, abort!
                if (c.initialized) { $(table).trigger('pagerChange', c); }
                if ( !c.removeRows ) {
                    hideRows(table, c);
                } else {
                    if ( e > rows.length ) {
                        e = rows.length;
                    }
                    $(table.tBodies[0]).addClass('tablesorter-hidden');
                    $.tablesorter.clearTableBody(table);
                    for ( i = s; i < e; i++ ) {
                        o = rows[i];
                        l = o.length;
                        for ( j = 0; j < l; j++ ) {
                            f.appendChild(o[j]);
                        }
                    }
                    table.tBodies[0].appendChild(f);
                    $(table.tBodies[0]).removeClass('tablesorter-hidden');
                }
                if ( c.page >= c.totalPages ) {
                    moveToLastPage(table, c);
                }
                updatePageDisplay(table, c);
                if ( !c.isDisabled ) { fixHeight(table, c); }
                $(table).trigger('applyWidgets');
            },

            showAllRows = function(table, c){
                if ( c.ajax ) {
                    pagerArrows(c, true);
                } else {
                    c.isDisabled = true;
                    $.data(table, 'pagerLastPage', c.page);
                    $.data(table, 'pagerLastSize', c.size);
                    c.page = 0;
                    c.size = c.totalRows;
                    c.totalPages = 1;
                    $('tr.pagerSavedHeightSpacer', table.tBodies[0]).remove();
                    renderTable(table, table.config.rowsCopy, c);
                }
                // disable size selector
                $(c.container).find(c.cssPageSize + ',' + c.cssGoto).each(function(){
                    $(this).addClass(c.cssDisabled)[0].disabled = true;
                });
            },

            moveToPage = function(table, c) {
                if ( c.isDisabled ) { return; }
                var p = Math.min( c.totalPages, c.filteredPages );
                if ( c.page < 0 ) { c.page = 0; }
                if ( c.page > ( p - 1 ) && p !== 0 ) { c.page = p - 1; }
                if (c.ajax) {
                    getAjax(table, c);
                } else if (!c.ajax) {
                    renderTable(table, table.config.rowsCopy, c);
                }
                $.data(table, 'pagerLastPage', c.page);
                $.data(table, 'pagerUpdateTriggered', true);
                if (c.initialized) { $(table).trigger('pageMoved', c); }
            },

            setPageSize = function(table, size, c) {
                c.size = size;
                $.data(table, 'pagerLastPage', c.page);
                $.data(table, 'pagerLastSize', c.size);
                c.totalPages = Math.ceil( c.totalRows / c.size );
                moveToPage(table, c);
            },

            moveToFirstPage = function(table, c) {
                c.page = 0;
                moveToPage(table, c);
            },

            moveToLastPage = function(table, c) {
                c.page = ( Math.min( c.totalPages, c.filteredPages ) - 1 );
                moveToPage(table, c);
            },

            moveToNextPage = function(table, c) {
                c.page++;
                if ( c.page >= ( Math.min( c.totalPages, c.filteredPages ) - 1 ) ) {
                    c.page = ( Math.min( c.totalPages, c.filteredPages ) - 1 );
                }
                moveToPage(table, c);
            },

            moveToPrevPage = function(table, c) {
                c.page--;
                if ( c.page <= 0 ) {
                    c.page = 0;
                }
                moveToPage(table, c);
            },

            destroyPager = function(table, c){
                showAllRows(table, c);
                $(c.container).hide(); // hide pager
                table.config.appender = null; // remove pager appender function
                $(table).unbind('destroy.pager sortEnd.pager filterEnd.pager enable.pager disable.pager');
            },

            enablePager = function(table, c, triggered){
                var p = $(c.cssPageSize, c.container).removeClass(c.cssDisabled).removeAttr('disabled');
                $(c.container).find(c.cssGoto).removeClass(c.cssDisabled).removeAttr('disabled');
                c.isDisabled = false;
                c.page = $.data(table, 'pagerLastPage') || c.page || 0;
                c.size = $.data(table, 'pagerLastSize') || parseInt(p.find('option[selected]').val(), 10) || c.size;
                p.val(c.size); // set page size
                c.totalPages = Math.ceil( Math.min( c.totalPages, c.filteredPages ) / c.size);
                if ( triggered ) {
                    $(table).trigger('update');
                    setPageSize(table, c.size, c);
                    hideRowsSetup(table, c);
                    fixHeight(table, c);
                }
            };

        $this.appender = function(table, rows) {
            var c = table.config.pager;
            if ( !c.ajax ) {
                table.config.rowsCopy = rows;
                c.totalRows = rows.length;
                c.size = $.data(table, 'pagerLastSize') || c.size;
                c.totalPages = Math.ceil(c.totalRows / c.size);
                renderTable(table, rows, c);
            }
        };

        $this.construct = function(settings) {
            return this.each(function() {
                // check if tablesorter has initialized
                if (!(this.config && this.hasInitialized)) { return; }
                var t, ctrls, fxn,
                    config = this.config,
                    c = config.pager = $.extend( {}, $.tablesorterPager.defaults, settings ),
                    table = this,
                    tc = table.config,
                    $t = $(table),
                    pager = $(c.container).addClass('tablesorter-pager').show(); // added in case the pager is reinitialized after being destroyed.
                config.appender = $this.appender;

                $t
                    .unbind('filterStart.pager filterEnd.pager sortEnd.pager disable.pager enable.pager destroy.pager update.pager pageSize.pager')
                    .bind('filterStart.pager', function(e, filters) {
                        $.data(table, 'pagerUpdateTriggered', false);
                        c.currentFilters = filters;
                    })
                    // update pager after filter widget completes
                    .bind('filterEnd.pager sortEnd.pager', function(e) {
                        //Prevent infinite event loops from occuring by setting this in all moveToPage calls and catching it here.
                        if ($.data(table, 'pagerUpdateTriggered')) {
                            $.data(table, 'pagerUpdateTriggered', false);
                            return;
                        }
                        if (e.type === 'filterEnd') { c.page = 0; }
                        moveToPage(table, c);
                        updatePageDisplay(table, c);
                        fixHeight(table, c);
                    })
                    .bind('disable.pager', function(){
                        showAllRows(table, c);
                    })
                    .bind('enable.pager', function(){
                        enablePager(table, c, true);
                    })
                    .bind('destroy.pager', function(){
                        destroyPager(table, c);
                    })
                    .bind('update.pager', function(){
                        hideRows(table, c);
                    })
                    .bind('pageSize.pager', function(e,v){
                        c.size = parseInt(v, 10) || 10;
                        hideRows(table, c);
                        updatePageDisplay(table, c);
                    })
                    .bind('pageSet.pager', function(e,v){
                        c.page = (parseInt(v, 10) || 1) - 1;
                        moveToPage(table, c);
                        updatePageDisplay(table, c);
                    });

                // clicked controls
                ctrls = [c.cssFirst, c.cssPrev, c.cssNext, c.cssLast];
                fxn = [ moveToFirstPage, moveToPrevPage, moveToNextPage, moveToLastPage ];

                pager.find(ctrls.join(','))
                    .unbind('click.pager')
                    .bind('click.pager', function(e){
                        var i, $this = $(this), l = ctrls.length;
                        if ( !$this.hasClass(c.cssDisabled) ) {
                            for (i = 0; i < l; i++) {
                                if ($this.is(ctrls[i])) {
                                    fxn[i](table, c);
                                    break;
                                }
                            }
                        }
                        return false;
                    });

                // goto selector
                if ( pager.find(c.cssGoto).length ) {
                    pager.find(c.cssGoto)
                        .unbind('change')
                        .bind('change', function(){
                            c.page = $(this).val() - 1;
                            moveToPage(table, c);
                        });
                    updatePageDisplay(table, c);
                }

                // page size selector
                t = pager.find(c.cssPageSize);
                if ( t.length ) {
                    t.unbind('change.pager').bind('change.pager', function() {
                        t.val( $(this).val() ); // in case there are more than one pagers
                        if ( !$(this).hasClass(c.cssDisabled) ) {
                            setPageSize(table, parseInt( $(this).val(), 10 ), c);
                            changeHeight(table, c);
                        }
                        return false;
                    });
                }

                // clear initialized flag
                c.initialized = false;
                // before initialization event
                $t.trigger('pagerBeforeInitialized', c);

                enablePager(table, c, false);

                if ( typeof(c.ajaxUrl) === 'string' ) {
                    // ajax pager; interact with database
                    c.ajax = true;
                    //When filtering with ajax, allow only custom filtering function, disable default filtering since it will be done server side.
                    tc.widgetOptions.filter_serversideFiltering = true;
                    tc.serverSideSorting = true;
                    moveToPage(table, c);
                } else {
                    c.ajax = false;
                    // Regular pager; all rows stored in memory
                    $(this).trigger("appendCache", true);
                    hideRowsSetup(table, c);
                }

                changeHeight(table, c);

                // pager initialized
                if (!c.ajax) {
                    c.initialized = true;
                    $(table).trigger('pagerInitialized', c);
                }
            });
        };

    }()
    });
// extend plugin scope
    $.fn.extend({
        tablesorterPager: $.tablesorterPager.construct
    });

})(jQuery);