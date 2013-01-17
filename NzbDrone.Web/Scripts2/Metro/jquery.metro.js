/**
 * jqMetro
 * JQUERY PLUGIN FOR METRO UI CONTROLS
 *
 * Copyright (c) 2011 Mohammad Valipour (http://manorey.net/mohblog)
 * Licensed under the MIT License:
 *   http://www.opensource.org/licenses/mit-license.php
 *
 */

;(function ($) {
    var defaults = {
        animationDuration: 150,
        headerOpacity: 0.5,
        fixedHeaders: false,
        headerSelector: function (item) { return item.children("h3").first(); },
        itemSelector: function (item) { return item.children(".pivot-item"); },
        headerItemTemplate: function () { return $("<span class='header' />"); },
        pivotItemTemplate: function () { return $("<div class='pivotItem' />"); },
        itemsTemplate: function () { return $("<div class='items' />"); },
        headersTemplate: function () { return $("<div class='headers' />"); },
        controlInitialized: undefined,
        selectedItemChanged: undefined
    };

    $.fn.metroPivot = function (settings) {
        if(this.length > 1)
        {
            return this.each(function(index, el){ $(el).wp7Pivot(settings); });
        }

        $.extend(this, defaults, settings);
        $.extend(this,{
            animating : false,
            headers : undefined,
            items : undefined,
            goToNext: function(){
                if(this.animating) return;
                this.headers.children(".current").next().trigger("click");
            },
            goToPrevious: function(){
                if(this.animating) return;
                this.headers.children(".header").last().trigger("click");
            },
            goToItemByName:function(header){
                if(this.animating) return;
                this.headers.children(":contains("+header+")").first().trigger("click");
            },
            goToItemByIndex:function(index){
                if(this.animating) return;
                this.headers.children().eq(index).trigger("click");
            },
            initialize : function () {
                var pivot = this;
                // define sections

                var headers = pivot.headersTemplate();
                var items = pivot.itemsTemplate();

                pivot.itemSelector(pivot).each(function (index, el) {
                    el = $(el);

                    var h3Element = pivot.headerSelector(el);
                    if (h3Element.length == 0) return;

                    var headerItem = pivot.headerItemTemplate().html(h3Element.html()).fadeTo(0, pivot.headerOpacity);
                    var pivotItem = pivot.pivotItemTemplate().append(el).hide();

                    if (index == 0) {
                        headerItem.addClass("current").fadeTo(0, 1);
                        pivotItem.addClass("current").show();
                    }
                    headerItem.attr("index", index);
                    headerItem.click(function() { pivot.pivotHeader_Click($(this)); });

                    headers.append(headerItem);
                    items.append(pivotItem);

                    h3Element.remove();
                });

                pivot.append(headers).append(items);
                pivot.headers = headers;
                pivot.items = items;

                this.data("controller", pivot);

                if(this.controlInitialized != undefined)
                {
                    this.controlInitialized();
                }
            },
            setCurrentHeader: function(header){
                var pivot = this;

                // make current header a normal one
                this.headers.children(".header.current").removeClass("current").fadeTo(0, this.headerOpacity);

                // make selected header to current
                header.addClass("current").fadeTo(0, 1);

                if(pivot.fixedHeaders == false)
                {
                    // create a copy for fadeout navigation
                    var copy = header.prevAll().clone();
                    // detach items to move to end of headers
                    var detached = $(header.prevAll().detach().toArray().reverse());

                    // copy animation items to beginning and animate them
                    $("<span />").append(copy).prependTo(pivot.headers).animate({ width: 0, opacity: 0 }, pivot.animationDuration, function () {
                        // when finished: delete animation objects
                        $(this).remove();

                        // and attach detached items to the end of headers
                        detached.fadeTo(0, 0).appendTo(pivot.headers).fadeTo(200, pivot.headerOpacity);
                    });
                }
            },
            setCurrentItem: function(item, index){
                var pivot = this;
                
                // hide current item immediately
                pivot.items.children(".pivotItem.current").hide().removeClass("current");

                // after a little delay
                setTimeout(function () {
                    // move the item to far right and make it visible
                    item.css({ marginLeft: item.outerWidth() }).show().addClass("current");

                    // animate it to left
                    item.animate( { marginLeft: 0 }, pivot.animationDuration, function() { pivot.currentItemChanged(index);});

                }, 200);                
            },
            currentItemChanged: function(index) {
                this.animating = false;
                if(this.selectedItemChanged != undefined)
                {
                    this.selectedItemChanged(index);
                }
            },
            pivotHeader_Click : function (me) {
                // ignore if already current
                if (me.is(".current")) return;

                // ignore if still animating
                if (this.animating) return;
                this.animating = true;

                // set current header
                this.setCurrentHeader(me);

                var index = me.attr("index");
                // find and set current item
                var item = this.items.children(".pivotItem:nth(" + index + ")");
                this.setCurrentItem(item, index);
            },
        });

        return this.initialize();
    };
})(jQuery);