/*
* File:        jquery.dataTables.editable.js
* Version:     2.0.8
* Author:      Jovan Popovic 
* 
* Copyright 2010-2011 Jovan Popovic, all rights reserved.
*
* This source file is free software, under either the GPL v2 license or a
* BSD style license, as supplied with this software.
* 
* This source file is distributed in the hope that it will be useful, but 
* WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
* or FITNESS FOR A PARTICULAR PURPOSE. 
* 
* Parameters:
* @sUpdateURL                       String      URL of the server-side page used for updating cell. Default value is "UpdateData".
* @sAddURL                          String      URL of the server-side page used for adding new row. Default value is "AddData".
* @sDeleteURL                       String      URL of the server-side page used to delete row by id. Default value is "DeleteData".
* @fnShowError                      Function    function(message, action){...}  used to show error message. Action value can be "update", "add" or "delete".
* @sAddNewRowFormId                 String      Id of the form for adding new row. Default id is "formAddNewRow".
* @oAddNewRowFormOptions            Object        Options that will be set to the "Add new row" dialog
* @sAddNewRowButtonId               String      Id of the button for adding new row. Default id is "btnAddNewRow".
* @oAddNewRowButtonOptions            Object        Options that will be set to the "Add new" button
* @sAddNewRowOkButtonId             String      Id of the OK button placed in add new row dialog. Default value is "btnAddNewRowOk".
* @oAddNewRowOkButtonOptions        Object        Options that will be set to the Ok button in the "Add new row" form
* @sAddNewRowCancelButtonId         String      Id of the Cancel button placed in add new row dialog. Default value is "btnAddNewRowCancel".
* @oAddNewRowCancelButtonOptions    Object        Options that will be set to the Cancel button in the "Add new row" form
* @sDeleteRowButtonId               String      Id of the button for adding new row. Default id is "btnDeleteRow".
* @oDeleteRowButtonOptions            Object        Options that will be set to the Delete button
* @sSelectedRowClass                String      Class that will be associated to the selected row. Default class is "row_selected".
* @sReadOnlyCellClass               String      Class of the cells that should not be editable. Default value is "read_only".
* @sAddDeleteToolbarSelector        String      Selector used to identify place where add and delete buttons should be placed. Default value is ".add_delete_toolbar".
* @fnStartProcessingMode            Function    function(){...} called when AJAX call is started. Use this function to add "Please wait..." message  when some button is pressed.
* @fnEndProcessingMode              Function    function(){...} called when AJAX call is ended. Use this function to close "Please wait..." message.
* @aoColumns                        Array       Array of the JEditable settings that will be applied on the columns
* @sAddHttpMethod                   String      Method used for the Add AJAX request (default is 'POST')
* @sAddDataType                     String      Data type expected from the server when adding a row; allowed values are the same as those accepted by JQuery's "datatype" parameter, e.g. 'text' and 'json'. The default is 'text'.
* @sDeleteHttpMethod                String      Method used for the Delete AJAX request (default is 'POST')
* @sDeleteDataType                  String      Data type expected from the server when deleting a row; allowed values are the same as those accepted by JQuery's "datatype" parameter, e.g. 'text' and 'json'. The default is 'text'.
* @fnOnDeleting                     Function    function(tr, id, fnDeleteRow){...} Function called before row is deleted.
tr isJQuery object encapsulating row that will be deleted
id is an id of the record that will be deleted.
fnDeleteRow(id) callback function that should be called to delete row with id
returns true if plugin should continue with deleting row, false will abort delete.
* @fnOnDeleted                      Function    function(status){...} Function called after delete action. Status can be "success" or "failure"
* @fnOnAdding                       Function    function(){...} Function called before row is added.
returns true if plugin should continue with adding row, false will abort add.
* @fnOnNewRowPosted                    Function    function(data) Function that can override default function that is called when server-side sAddURL returns result
You can use this function to add different behaviour when server-side page returns result
* @fnOnAdded                        Function    function(status){...} Function called after add action. Status can be "success" or "failure"
* @fnOnEditing                      Function    function(input){...} Function called before cell is updated.
input JQuery object wrapping the input element used for editing value in the cell.
returns true if plugin should continue with sending AJAX request, false will abort update.
* @fnOnEdited                       Function    function(status){...} Function called after edit action. Status can be "success" or "failure"
* @sEditorHeight                    String      Default height of the cell editors
* @sEditorWidth                     String      Default width of the cell editors
* @oDeleteParameters                Object      Additonal objects added to the DELETE Ajax request
* @oUpdateParameters                Object      Additonal objects added to the UPDATE Ajax request
* @sIDToken                         String      Token in the add new row dialog that will be replaced with a returned id of the record that is created
* @sSuccessResponse                    String        Text returned from the server if record is successfully deleted or edited. Default "ok" 
*/
(function ($) {

    $.fn.makeEditable = function (options) {

        var iDisplayStart = 0;

        function fnGetCellID(cell) {
            ///<summary>
            ///Utility function used to determine id of the cell
            ///By default it is assumed that id is placed as an id attribute of <tr> that that surround the cell (<td> tag). E.g.:
            ///<tr id="17">
            ///  <td>...</td><td>...</td><td>...</td><td>...</td>
            ///</tr>
            ///</summary>
            ///<param name="cell" type="DOM" domElement="true">TD cell refference</param>

            return properties.fnGetRowID($(cell.parentNode));
        }

        function _fnSetRowIDInAttribute(row, id, overwrite) {
            ///<summary>
            ///Utility function used to set id of the row. Usually when a new record is created, added to the table,
            ///and when id of the record is retrieved from the server-side.
            ///It is assumed that id is placed as an id attribute of <tr> that that surround the cell (<td> tag). E.g.:
            ///<tr id="17">
            ///  <td>...</td><td>...</td><td>...</td><td>...</td>
            ///</tr>
            ///This function is used when a datatable is configured in the server side processing mode or ajax source mode
            ///</summary>
            ///<param name="row" type="DOM" domElement="true">TR row where record is placed</param>

            if (overwrite) {
                row.attr("id", id);
            } else {
                if (row.attr("id") == null || row.attr("id") == "")
                    row.attr("id", id);
            }
        }

        function _fnGetRowIDFromAttribute(row) {
            ///<summary>
            ///Utility function used to get id of the row.
            ///It is assumed that id is placed as an id attribute of <tr> that that surround the cell (<td> tag). E.g.:
            ///<tr id="17">
            ///  <td>...</td><td>...</td><td>...</td><td>...</td>
            ///</tr>
            ///This function is used when a datatable is configured in the standard client side mode
            ///</summary>
            ///<param name="row" type="DOM" domElement="true">TR row where record is placed</param>
            ///<returns type="Number">Id of the row - by default id attribute placed in the TR tag</returns>

            return row.attr("id");
        }

        function _fnSetRowIDInFirstCell(row, id) {
            ///<summary>
            ///Utility function used to set id of the row. Usually when a new record is created, added to the table,
            ///and when id of the record is retrieved from the server-side).
            ///It is assumed that id is placed as a value of the first &lt;TD&gt; cell in the &lt;TR&gt;. As example:
            ///<tr>
            ///     <td>17</td><td>...</td><td>...</td><td>...</td>
            ///</tr>
            ///This function is used when a datatable is configured in the server side processing mode or ajax source mode
            ///</summary>
            ///<param name="row" type="DOM" domElement="true">TR row where record is placed</param>

            $("td:first", row).html(id);
        }


        function _fnGetRowIDFromFirstCell(row) {
            ///<summary>
            ///Utility function used to get id of the row.
            ///It is assumed that id is placed as a value of the first &lt;TD&gt; cell in the &lt;TR&gt;. As example:
            ///<tr>
            ///     <td>17</td><td>...</td><td>...</td><td>...</td>
            ///</tr>
            ///This function is used when a datatable is configured in the server side processing mode or ajax source mode
            ///</summary>
            ///<param name="row" type="DOM" domElement="true">TR row where record is placed</param>
            ///<returns type="Number">Id of the row - by default id attribute placed in the TR tag</returns>

            return $("td:first", row).html();

        }

        //Reference to the DataTable object
        var oTable;
        //Refences to the buttons used for manipulating table data
        var oAddNewRowButton, oDeleteRowButton, oConfirmRowAddingButton, oCancelRowAddingButton;
        //Reference to the form used for adding new data
        var oAddNewRowForm;

        //Plugin options
        var properties;

        function _fnShowError(errorText, action) {
            ///<summary>
            ///Shows an error message (Default function)
            ///</summary>
            ///<param name="errorText" type="String">text that should be shown</param>
            ///<param name="action" type="String"> action that was executed when error occured e.g. "update", "delete", or "add"</param>

            alert(errorText);
        }

        function _fnStartProcessingMode() {
            ///<summary>
            ///Function that starts "Processing" mode i.e. shows "Processing..." dialog while some action is executing(Default function)
            ///</summary>

            if (oTable.fnSettings().oFeatures.bProcessing) {
                $(".dataTables_processing").css('visibility', 'visible');
            }
        }

        function _fnEndProcessingMode() {
            ///<summary>
            ///Function that ends the "Processing" mode and returns the table in the normal state(Default function)
            ///</summary>

            if (oTable.fnSettings().oFeatures.bProcessing) {
                $(".dataTables_processing").css('visibility', 'hidden');
            }
        }

        var sOldValue, sNewCellValue, sNewCellDislayValue;

        function fnApplyEditable(aoNodes) {
            ///<summary>
            ///Function that applies editable plugin to the array of table rows
            ///</summary>
            ///<param name="aoNodes" type="Array[TR]">Aray of table rows &lt;TR&gt; that should be initialized with editable plugin</param>

            if (properties.bDisableEditing)
                return;
            var oDefaultEditableSettings = {
                event: 'dblclick',

                "onsubmit": function (settings, original) {
                    sOldValue = original.revert;
                    sNewCellValue = null;
                    sNewCellDisplayValue = null;
                    if(settings.type == "text" || settings.type == "select" || settings.type == "textarea" )
                    {
                        var input = $("input,select,textarea", this);
                        sNewCellValue = $("input,select,textarea", $(this)).val();
                        if (input.length == 1) {
                            var oEditElement = input[0];
                            if (oEditElement.nodeName.toLowerCase() == "select" || oEditElement.tagName.toLowerCase() == "select")
                                sNewCellDisplayValue = $("option:selected", oEditElement).text(); //For select list use selected text instead of value for displaying in table
                            else
                                sNewCellDisplayValue = sNewCellValue;
                        }

                        if (!properties.fnOnEditing(input))
                            return false;
                        var x = settings;
                        if (settings.cssclass != null) {
                            input.addClass(settings.cssclass);
                            if (!input.valid() || 0 == input.valid())
                                return false;
                            else
                                return true;
                        }
                    }
                    
                    iDisplayStart = fnGetDisplayStart();
                    properties.fnStartProcessingMode();
                },
                "submitdata": function (value, settings) {
                    //iDisplayStart = fnGetDisplayStart();
                    //properties.fnStartProcessingMode();
                    var id = fnGetCellID(this);
                    var rowId = oTable.fnGetPosition(this)[0];
                    var columnPosition = oTable.fnGetPosition(this)[1];
                    var columnId = oTable.fnGetPosition(this)[2];
                    var sColumnName = oTable.fnSettings().aoColumns[columnId].sName;
                    if (sColumnName == null || sColumnName == "")
                        sColumnName = oTable.fnSettings().aoColumns[columnId].sTitle;
                    var updateData = null;
                    if (properties.aoColumns == null || properties.aoColumns[columnId] == null) {
                        updateData = $.extend({},
                                            properties.oUpdateParameters,
                                            {
                                                "id": id,
                                                "rowId": rowId,
                                                "columnPosition": columnPosition,
                                                "columnId": columnId,
                                                "columnName": sColumnName
                                            });
                    }
                    else {
                        updateData = $.extend({},
                                            properties.oUpdateParameters,
                                            properties.aoColumns[columnId].oUpdateParameters,
                                            {
                                                "id": id,
                                                "rowId": rowId,
                                                "columnPosition": columnPosition,
                                                "columnId": columnId,
                                                "columnName": sColumnName
                                            });
                    }
                    return updateData;
                },
                "callback": function (sValue, settings) {
                    properties.fnEndProcessingMode();
                    var status = "";
                    var aPos = oTable.fnGetPosition(this);

                    if (properties.sSuccessResponse == "IGNORE" || 
                        (     properties.aoColumns != null
                            && properties.aoColumns[aPos[2]] != null 
                            && properties.aoColumns[aPos[2]].sSuccessResponse == "IGNORE") || 
                        (sNewCellValue == sValue) || 
                        properties.sSuccessResponse == sValue) {
                        if(sNewCellDisplayValue == null)
                        {
                            //sNewCellDisplayValue = sValue;
                            oTable.fnUpdate(sValue, aPos[0], aPos[2]);
                        }else{
                            oTable.fnUpdate(sNewCellDisplayValue, aPos[0], aPos[2]);
                        }
                        $("td.last-updated-cell", oTable).removeClass("last-updated-cell");
                        $(this).addClass("last-updated-cell");
                        status = "success";
                    } else {
                        oTable.fnUpdate(sOldValue, aPos[0], aPos[2]);
                        properties.fnShowError(sValue, "update");
                        status = "failure";
                    }

                    properties.fnOnEdited(status, sOldValue, sNewCellDisplayValue, aPos[0], aPos[1], aPos[2]);
                    if (settings.fnOnCellUpdated != null) {
                        settings.fnOnCellUpdated(status, sValue, aPos[0], aPos[2], settings);
                    }
                    
                    fnSetDisplayStart();
                },
                "onerror": function () {
                    properties.fnEndProcessingMode();
                    properties.fnShowError("Cell cannot be updated", "update");
                    properties.fnOnEdited("failure");
                },
                "height": properties.sEditorHeight,
                "width": properties.sEditorWidth
            };

            var cells = null;

            if (properties.aoColumns != null) {

                for (var iDTindex = 0, iDTEindex = 0; iDTindex < oSettings.aoColumns.length; iDTindex++) {
                    if (oSettings.aoColumns[iDTindex].bVisible) {//if DataTables column is visible
                        if (properties.aoColumns[iDTEindex] == null) {
                            //If editor for the column is not defined go to the next column
                            iDTEindex++;
                            continue;
                        }
                        //Get all cells in the iDTEindex column (nth child is 1-indexed array)
                        cells = $("td:nth-child(" + (iDTEindex + 1) + ")", aoNodes);

                        var oColumnSettings = oDefaultEditableSettings;
                        oColumnSettings = $.extend({}, oDefaultEditableSettings, properties.oEditableSettings, properties.aoColumns[iDTEindex]);
                        iDTEindex++;
                        var sUpdateURL = properties.sUpdateURL;
                        try {
                            if (oColumnSettings.sUpdateURL != null)
                                sUpdateURL = oColumnSettings.sUpdateURL;
                        } catch (ex) {
                        }
                        //cells.editable(sUpdateURL, oColumnSettings);
                        cells.each(function () {
                            if (!$(this).hasClass(properties.sReadOnlyCellClass)) {
                                $(this).editable(sUpdateURL, oColumnSettings);
                            }
                        });
                    }

                } //end for
            } else {
                cells = $('td:not(.' + properties.sReadOnlyCellClass + ')', aoNodes);
                cells.editable(properties.sUpdateURL, $.extend({}, oDefaultEditableSettings, properties.oEditableSettings));
            }
        }

        function fnOnRowAdding(event) {
            ///<summary>
            ///Event handler called when a user click on the submit button in the "Add new row" form. 
            ///</summary>
            ///<param name="event">Event that caused the action</param>

            if (properties.fnOnAdding()) {
                if (oAddNewRowForm.valid()) {
                    iDisplayStart = fnGetDisplayStart();
                    properties.fnStartProcessingMode();

                    if (properties.bUseFormsPlugin) {
                        //Still in beta(development)
                        $(oAddNewRowForm).ajaxSubmit({
                            dataType: 'xml',
                            success: function (response, statusString, xhr) {
                                if (xhr.responseText.toLowerCase().indexOf("error") != -1) {
                                    properties.fnEndProcessingMode();
                                    properties.fnShowError(xhr.responseText.replace("Error",""), "add");
                                    properties.fnOnAdded("failure");
                                } else {
                                    fnOnRowAdded(xhr.responseText);
                                }

                            },
                            error: function (response) {
                                properties.fnEndProcessingMode();
                                properties.fnShowError(response.responseText, "add");
                                properties.fnOnAdded("failure");
                            }
                        }
                        );

                    } else {

                        var params = oAddNewRowForm.serialize();
                        $.ajax({ 'url': properties.sAddURL,
                            'data': params,
                            'type': properties.sAddHttpMethod,
                            'dataType': properties.sAddDataType,
                            success: fnOnRowAdded,
                            error: function (response) {
                                properties.fnEndProcessingMode();
                                properties.fnShowError(response.responseText, "add");
                                properties.fnOnAdded("failure");
                            }
                        });
                    }
                }
            }
            event.stopPropagation();
            event.preventDefault();
        }

        function _fnOnNewRowPosted(data) {
            ///<summary>Callback function called BEFORE a new record is posted to the server</summary>
            ///TODO: Check this

            return true;
        }

        function fnAddRowFromForm(oForm) {
            ///<summary>
            ///Adding a row in the table from the action form
            ///</summary>
            ///<param name="oForm" type="JQuery">Form that contains data to be copied into the row</param>

            var oSettings = oTable.fnSettings();
            var iColumnCount = oSettings.aoColumns.length;
            var values = new Array();

            $("input:text[rel],input:radio[rel][checked],input:hidden[rel],select[rel],textarea[rel],span.datafield[rel],input:checkbox[rel]", oForm).each(function () {
                var rel = $(this).attr("rel");
                var sCellValue = "";
                if (rel >= iColumnCount)
                    properties.fnShowError("In the add form is placed input element with the name '" + $(this).attr("name") + "' with the 'rel' attribute that must be less than a column count - " + iColumnCount, "add");
                else {
                    if (this.nodeName.toLowerCase() == "select" || this.tagName.toLowerCase() == "select") {
                        //sCellValue = $("option:selected", this).text();
                        sCellValue = $.map(
                                             $.makeArray($("option:selected", this)),
                                             function (n, i) {
                                                 return $(n).text();
                                             }).join(",");
                    }
                    else if (this.nodeName.toLowerCase() == "span" || this.tagName.toLowerCase() == "span")
                        sCellValue = $(this).html();
                    else {
                        if (this.type == "checkbox") {
                            if (this.checked)
                                sCellValue = (this.value != "on") ? this.value : "true";
                            else
                                sCellValue = (this.value != "on") ? "" : "false";
                        } else
                            sCellValue = this.value;
                    }
                    sCellValue = sCellValue.replace(properties.sIDToken, data);//@BUG What is data?????
                    values[rel] = sCellValue;
                }
            });

            //Add values from the form into the table
            var rtn = oTable.fnAddData(values);
            var oTRAdded = oTable.fnGetNodes(rtn);
            //Apply editable plugin on the cells of the table
            fnApplyEditable(oTRAdded);

        }

        function fnOnRowAdded(data) {
            ///<summary>
            ///Function that is called when  a new row is added, and Ajax response is returned from server
            ///</summary>
            ///<param name="data" type="int">Id of the new row that is returned from the server</param>

            properties.fnEndProcessingMode();

            if (properties.fnOnNewRowPosted(data)) {

                var oSettings = oTable.fnSettings();
                if (!oSettings.oFeatures.bServerSide) {
                    var iColumnCount = oSettings.aoColumns.length;
                    var values = new Array();
                    var rowData = new Object();

                    $("input:text[rel],input:radio[rel][checked],input:hidden[rel],select[rel],textarea[rel],span.datafield[rel],input:checkbox[rel]", oAddNewRowForm).each(function () {
                        var rel = $(this).attr("rel");
                        var sCellValue = "";
                        if (rel >= iColumnCount)
                            properties.fnShowError("In the add form is placed input element with the name '" + $(this).attr("name") + "' with the 'rel' attribute that must be less than a column count - " + iColumnCount, "add");
                        else {
                            if (this.nodeName.toLowerCase() == "select" || this.tagName.toLowerCase() == "select") {
                                //sCellValue = $("option:selected", this).text();
                                sCellValue = $.map(
                                             $.makeArray($("option:selected", this)),
                                             function (n, i) {
                                                 return $(n).text();
                                             }).join(",");
                            }
                            else if (this.nodeName.toLowerCase() == "span" || this.tagName.toLowerCase() == "span")
                                sCellValue = $(this).html();
                            else {
                                if (this.type == "checkbox") {
                                    if (this.checked)
                                        sCellValue = (this.value != "on") ? this.value : "true";
                                    else
                                        sCellValue = (this.value != "on") ? "" : "false";
                                } else
                                    sCellValue = this.value;
                            }

                            sCellValue = sCellValue.replace(properties.sIDToken, data);
                            if (oSettings.aoColumns != null
                                && oSettings.aoColumns[rel] != null
                                && isNaN(parseInt(oSettings.aoColumns[0].mDataProp))) {
                                rowData[oSettings.aoColumns[rel].mDataProp] = sCellValue;
                            } else {
                                values[rel] = sCellValue;
                            }
                        }
                    });

                    var rtn;
                    //Add values from the form into the table
                    if (oSettings.aoColumns != null && isNaN(parseInt(oSettings.aoColumns[0].mDataProp))) {
                        rtn = oTable.fnAddData(rowData);
                    }
                    else {
                        rtn = oTable.fnAddData(values);
                    }

                    var oTRAdded = oTable.fnGetNodes(rtn);
                    //add id returned by server page as an TR id attribute
                    properties.fnSetRowID($(oTRAdded), data, true);
                    //Apply editable plugin on the cells of the table
                    fnApplyEditable(oTRAdded);

                    $("tr.last-added-row", oTable).removeClass("last-added-row");
                    $(oTRAdded).addClass("last-added-row");
                } else {
                    oTable.fnDraw(false);
                }
                //Close the dialog
                oAddNewRowForm.dialog('close');
                $(oAddNewRowForm)[0].reset();
                $(".error", $(oAddNewRowForm)).html("");

                fnSetDisplayStart();
                properties.fnOnAdded("success");
            }
        }

        function fnOnCancelRowAdding(event) {
            ///<summary>
            ///Event handler function that is executed when a user press cancel button in the add new row form
            ///</summary>
            ///<param name="event" type="int">DOM event that caused an error</param>

            //Clear the validation messages and reset form
            $(oAddNewRowForm).validate().resetForm();  // Clears the validation errors
            $(oAddNewRowForm)[0].reset();

            $(".error", $(oAddNewRowForm)).html("");
            $(".error", $(oAddNewRowForm)).hide();  // Hides the error element

            //Close the dialog
            oAddNewRowForm.dialog('close');
            event.stopPropagation();
            event.preventDefault();
        }


        function fnDisableDeleteButton() {
            ///<summary>
            ///Function that disables delete button
            ///</summary>

            if (properties.oDeleteRowButtonOptions != null) {
                //oDeleteRowButton.disable();
                oDeleteRowButton.button("option", "disabled", true);
            } else {
                oDeleteRowButton.attr("disabled", "true");
            }
        }

        function fnEnableDeleteButton() {
            ///<summary>
            ///Function that enables delete button
            ///</summary>

            if (properties.oDeleteRowButtonOptions != null) {
                //oDeleteRowButton.enable();
                oDeleteRowButton.button("option", "disabled", false);
            } else {
                oDeleteRowButton.removeAttr("disabled");
            }
        }



        function fnDeleteRow(id, sDeleteURL) {
            ///<summary>
            ///Function that deletes a row with an id, using the sDeleteURL server page
            ///</summary>
            ///<param name="id" type="int">Id of the row that will be deleted. Id value is placed in the attribute of the TR tag that will be deleted</param>
            ///<param name="sDeleteURL" type="String">Server URL where delete request will be posted</param>

            var sURL = sDeleteURL;
            if (sDeleteURL == null)
                sURL = properties.sDeleteURL;
            properties.fnStartProcessingMode();
            var data = $.extend(properties.oDeleteParameters, { "id": id });
            $.ajax({ 'url': sURL,
                'type': properties.sDeleteHttpMethod,
                'data': data,
                "success": fnOnRowDeleted,
                "dataType": properties.sDeleteDataType,
                "error": function (response) {
                    properties.fnEndProcessingMode();
                    properties.fnShowError(response.responseText, "delete");
                    properties.fnOnDeleted("failure");

                }
            });
        }

        function _fnOnRowDelete(event) {
            ///<summary>
            ///Event handler for the delete button
            ///</summary>
            ///<param name="event" type="Event">DOM event</param>

            iDisplayStart = fnGetDisplayStart();
            if ($('tr.' + properties.sSelectedRowClass + ' td', oTable).length == 0) {
                fnDisableDeleteButton();
                return;
            }
            var id = fnGetCellID($('tr.' + properties.sSelectedRowClass + ' td', oTable)[0]);
            if (properties.fnOnDeleting($('tr.' + properties.sSelectedRowClass, oTable), id, fnDeleteRow)) {
                fnDeleteRow(id);
            }
        }

        function fnOnRowDeleted(response) {
            ///<summary>
            ///Called after the record is deleted on the server (in the ajax success callback)
            ///</summary>
            ///<param name="response" type="String">Response text eturned from the server-side page</param>

            properties.fnEndProcessingMode();
            var oTRSelected = $('tr.' + properties.sSelectedRowClass, oTable)[0];
            if (response == properties.sSuccessResponse || response == "") {
                oTable.fnDeleteRow(oTRSelected);
                fnDisableDeleteButton();
                fnSetDisplayStart();
                properties.fnOnDeleted("success");
            }
            else {
                properties.fnShowError(response, "delete");
                properties.fnOnDeleted("failure");
            }
        }

        function _fnOnDeleting(tr, id, fnDeleteRow) {
            ///<summary>
            ///The default function that is called before row is deleted
            ///Returning false will abort delete
            ///Function can be overriden via plugin properties in order to create custom delete functionality
            ///in that case call fnDeleteRow with parameter id, and return false to prevent double delete action
            ///</summary>
            ///<param name="tr" type="JQuery">JQuery wrapper around the TR tag that will be deleted</param>
            ///<param name="id" type="String">Id of the record that wil be deleted</param>
            ///<param name="fnDeleteRow" type="Function(id)">Function that will be called to delete a row. Default - fnDeleteRow(id)</param>

            return confirm("Are you sure that you want to delete this record?"); ;
        }

        /* Function called after delete action
        * @param    result  string 
        *           "success" if row is actually deleted 
        *           "failure" if delete failed
        * @return   void
        */
        function _fnOnDeleted(result) { }

        function _fnOnEditing(input) { return true; }
        function _fnOnEdited(result, sOldValue, sNewValue, iRowIndex, iColumnIndex, iRealColumnIndex) {

        }

        function fnOnAdding() { return true; }
        function _fnOnAdded(result) { }

        var oSettings;
        function fnGetDisplayStart() {
            return oSettings._iDisplayStart;
        }

        function fnSetDisplayStart() {
            ///<summary>
            ///Set the pagination position(do nothing in the server-side mode)
            ///</summary>

            if (oSettings.oFeatures.bServerSide === false) {
                oSettings._iDisplayStart = iDisplayStart;
                oSettings.oApi._fnCalculateEnd(oSettings);
                //draw the 'current' page
                oSettings.oApi._fnDraw(oSettings);
            }
        }

        function _fnOnBeforeAction(sAction) {
            return true;
        }

        function _fnOnActionCompleted(sStatus) {

        }

        function fnGetActionSettings(sAction) {
            ///<summary>Returns settings object for the action</summary>
            ///<param name="sAction" type="String">The name of the action</param>

            if (properties.aoTableAction)
                properties.fnShowError("Configuration error - aoTableAction setting are not set", sAction);
            var i = 0;

            for (i = 0; i < properties.aoTableActions.length; i++) {
                if (properties.aoTableActions[i].sAction == sAction)
                    return properties.aoTableActions[i];
            }

            properties.fnShowError("Cannot find action configuration settings", sAction);
        }


        function fnUpdateRow(oActionForm) {
            ///<summary>Updates table row using  form fields</summary>
            ///<param name="oActionForm" type="DOM">Form used to enter data</param>

            var sAction = $(oActionForm).attr("id");
            sAction = sAction.replace("form", "");
            var sActionURL = $(oActionForm).attr("action");
            if (properties.fnOnBeforeAction(sAction)) {
                if ($(oActionForm).valid()) {
                    iDisplayStart = fnGetDisplayStart();
                    properties.fnStartProcessingMode();
                    if (properties.bUseFormsPlugin) {

                        //Still in beta(development)
                        var oAjaxSubmitOptions = {
                            success: function (response, statusString, xhr) {
                                properties.fnEndProcessingMode();
                                if (response.toLowerCase().indexOf("error") != -1 || statusString != "success") {
                                    properties.fnShowError(response, sAction);
                                    properties.fnOnActionCompleted("failure");
                                } else {
                                    fnUpdateRowOnSuccess(oActionForm);
                                    properties.fnOnActionCompleted("success");
                                }

                            },
                            error: function (response) {
                                properties.fnEndProcessingMode();
                                properties.fnShowError(response.responseText, sAction);
                                properties.fnOnActionCompleted("failure");
                            }
                        };
                        var oActionSettings = fnGetActionSettings(sAction);
                        oAjaxSubmitOptions = $.extend({}, properties.oAjaxSubmitOptions, oAjaxSubmitOptions);
                        $(oActionForm).ajaxSubmit(oAjaxSubmitOptions);

                    } else {
                        var params = $(oActionForm).serialize();
                        $.ajax({ 'url': sActionURL,
                            'data': params,
                            'type': properties.sAddHttpMethod,
                            'dataType': properties.sAddDataType,
                            success: function (response) {
                                properties.fnEndProcessingMode();
                                fnUpdateRowOnSuccess(oActionForm);
                                properties.fnOnActionCompleted("success");
                            },
                            error: function (response) {
                                properties.fnEndProcessingMode();
                                properties.fnShowError(response.responseText, sAction);
                                properties.fnOnActionCompleted("failure");
                            }
                        });
                    }
                }
            }
        }

        function fnUpdateRowOnSuccess(oActionForm) {
            ///<summary>Updates table row using  form fields after the ajax success callback is executed</summary>
            ///<param name="oActionForm" type="DOM">Form used to enter data</param>

            var iRowID = jQuery.data(oActionForm, 'ROWID');
            //var iDataRowID = jQuery.data(oActionForm, 'DATAROWID');
            var oSettings = oTable.fnSettings();
            var iColumnCount = oSettings.aoColumns.length;
            var values = new Array();

            var sAction = $(oActionForm).attr("id");
            sAction = sAction.replace("form", "");

            //$("input.ROWID").val(iRowID);
            //$("input.DATAROWID").val(iDataRowID);

            $("input:text[rel],input:radio[rel][checked],input:hidden[rel],select[rel],textarea[rel],span.datafield[rel],input:checkbox[rel]", oActionForm).each(function () {
                var rel = $(this).attr("rel");
                var sCellValue = "";
                if (rel >= iColumnCount)
                    properties.fnShowError("In the add form is placed input element with the name '" + $(this).attr("name") + "' with the 'rel' attribute that must be less than a column count - " + iColumnCount, "add");
                else {
                    if (this.nodeName.toLowerCase() == "select" || this.tagName.toLowerCase() == "select") {
                        //sCellValue = $("option:selected", this).text();
                        sCellValue = $.map(
                                             $.makeArray($("option:selected", this)),
                                             function (n, i) {
                                                 return $(n).text();
                                             }).join(",");
                    }
                    else if (this.nodeName.toLowerCase() == "span" || this.tagName.toLowerCase() == "span")
                        sCellValue = $(this).html();
                    else {
                        if (this.type == "checkbox") {
                            if (this.checked)
                                sCellValue = (this.value != "on") ? this.value : "true";
                            else
                                sCellValue = (this.value != "on") ? "" : "false";
                        } else
                            sCellValue = this.value;
                    }

                    //sCellValue = sCellValue.replace(properties.sIDToken, data);
                    //values[rel] = sCellValue;
                    oTable.fnUpdate(sCellValue, iRowID, rel);
                }
            });
            $(oActionForm).dialog('close');


        }

        oTable = this;

        var defaults = {

            sUpdateURL: "UpdateData",
            sAddURL: "AddData",
            sDeleteURL: "DeleteData",
            sAddNewRowFormId: "formAddNewRow",
            oAddNewRowFormOptions: { autoOpen: false, modal: true },
            sAddNewRowButtonId: "btnAddNewRow",
            oAddNewRowButtonOptions: null,
            sAddNewRowOkButtonId: "btnAddNewRowOk",
            sAddNewRowCancelButtonId: "btnAddNewRowCancel",
            oAddNewRowOkButtonOptions: { label: "Ok" },
            oAddNewRowCancelButtonOptions: { label: "Cancel" },
            sDeleteRowButtonId: "btnDeleteRow",
            oDeleteRowButtonOptions: null,
            sSelectedRowClass: "row_selected",
            sReadOnlyCellClass: "read_only",
            sAddDeleteToolbarSelector: ".add_delete_toolbar",
            fnShowError: _fnShowError,
            fnStartProcessingMode: _fnStartProcessingMode,
            fnEndProcessingMode: _fnEndProcessingMode,
            aoColumns: null,
            fnOnDeleting: _fnOnDeleting,
            fnOnDeleted: _fnOnDeleted,
            fnOnAdding: fnOnAdding,
            fnOnNewRowPosted: _fnOnNewRowPosted,
            fnOnAdded: _fnOnAdded,
            fnOnEditing: _fnOnEditing,
            fnOnEdited: _fnOnEdited,
            sAddHttpMethod: 'POST',
            sAddDataType: "text",
            sDeleteHttpMethod: 'POST',
            sDeleteDataType: "text",
            fnGetRowID: _fnGetRowIDFromAttribute,
            fnSetRowID: _fnSetRowIDInAttribute,
            sEditorHeight: "100%",
            sEditorWidth: "100%",
            bDisableEditing: false,
            oDeleteParameters: {},
            oUpdateParameters: {},
            sIDToken: "DATAROWID",
            aoTableActions: null,
            fnOnBeforeAction: _fnOnBeforeAction,
            bUseFormsPlugin: false,
            fnOnActionCompleted: _fnOnActionCompleted,
            sSuccessResponse: "ok"


        };

        properties = $.extend(defaults, options);
        oSettings = oTable.fnSettings();

        return this.each(function () {

            if (oTable.fnSettings().sAjaxSource != null) {
                oTable.fnSettings().aoDrawCallback.push({
                    "fn": function () {
                        //Apply jEditable plugin on the table cells
                        fnApplyEditable(oTable.fnGetNodes());
                        $(oTable.fnGetNodes()).each(function () {
                            var position = oTable.fnGetPosition(this);
                            var id = oTable.fnGetData(position)[0];
                            properties.fnSetRowID($(this), id);
                        }
                        );
                    },
                    "sName": "fnApplyEditable"
                });

            } else {
                //Apply jEditable plugin on the table cells
                fnApplyEditable(oTable.fnGetNodes());
            }

            //Setup form to open in dialog
            oAddNewRowForm = $("#" + properties.sAddNewRowFormId);
            if (oAddNewRowForm.length != 0) {

                ///Check does the add new form has all nessecary fields
                var oSettings = oTable.fnSettings();
                var iColumnCount = oSettings.aoColumns.length;
                for (i = 0; i < iColumnCount; i++) {
                    if ($("[rel=" + i + "]", oAddNewRowForm).length == 0)
                        properties.fnShowError("In the form that is used for adding new records cannot be found an input element with rel=" + i + " that will be bound to the value in the column " + i + ". See http://code.google.com/p/jquery-datatables-editable/wiki/AddingNewRecords#Add_new_record_form for more details", "init");
                }


                if (properties.oAddNewRowFormOptions != null) {
                    properties.oAddNewRowFormOptions.autoOpen = false;
                } else {
                    properties.oAddNewRowFormOptions = { autoOpen: false };
                }
                oAddNewRowForm.dialog(properties.oAddNewRowFormOptions);

                //Add button click handler on the "Add new row" button
                oAddNewRowButton = $("#" + properties.sAddNewRowButtonId);
                if (oAddNewRowButton.length != 0) {
                    oAddNewRowButton.click(function () {
                        oAddNewRowForm.dialog('open');
                    });
                } else {
                    if ($(properties.sAddDeleteToolbarSelector).length == 0) {
                        throw "Cannot find a button with an id '" + properties.sAddNewRowButtonId + "', or placeholder with an id '" + properties.sAddDeleteToolbarSelector + "' that should be used for adding new row although form for adding new record is specified";
                    } else {
                        oAddNewRowButton = null; //It will be auto-generated later
                    }
                }

                //Prevent Submit handler
                if (oAddNewRowForm[0].nodeName.toLowerCase() == "form") {
                    oAddNewRowForm.unbind('submit');
                    oAddNewRowForm.submit(function (event) {
                        fnOnRowAdding(event);
                        return false;
                    });
                } else {
                    $("form", oAddNewRowForm[0]).unbind('submit');
                    $("form", oAddNewRowForm[0]).submit(function (event) {
                        fnOnRowAdding(event);
                        return false;
                    });
                }

                // array to add default buttons to
                var aAddNewRowFormButtons = [];

                oConfirmRowAddingButton = $("#" + properties.sAddNewRowOkButtonId, oAddNewRowForm);
                if (oConfirmRowAddingButton.length == 0) {
                    //If someone forgotten to set the button text
                    if (properties.oAddNewRowOkButtonOptions.text == null
                        || properties.oAddNewRowOkButtonOptions.text == "") {
                        properties.oAddNewRowOkButtonOptions.text = "Ok";
                    }
                    properties.oAddNewRowOkButtonOptions.click = fnOnRowAdding;
                    properties.oAddNewRowOkButtonOptions.id = properties.sAddNewRowOkButtonId;
                    // push the add button onto the array
                    aAddNewRowFormButtons.push(properties.oAddNewRowOkButtonOptions);
                } else {
                    oConfirmRowAddingButton.click(fnOnRowAdding);
                }

                oCancelRowAddingButton = $("#" + properties.sAddNewRowCancelButtonId);
                if (oCancelRowAddingButton.length == 0) {
                    //If someone forgotten to the button text
                    if (properties.oAddNewRowCancelButtonOptions.text == null
                        || properties.oAddNewRowCancelButtonOptions.text == "") {
                        properties.oAddNewRowCancelButtonOptions.text = "Cancel";
                    }
                    properties.oAddNewRowCancelButtonOptions.click = fnOnCancelRowAdding;
                    properties.oAddNewRowCancelButtonOptions.id = properties.sAddNewRowCancelButtonId;
                    // push the cancel button onto the array
                    aAddNewRowFormButtons.push(properties.oAddNewRowCancelButtonOptions);
                } else {
                    oCancelRowAddingButton.click(fnOnCancelRowAdding);
                }
                // if the array contains elements, add them to the dialog
                if (aAddNewRowFormButtons.length > 0) {
                    oAddNewRowForm.dialog('option', 'buttons', aAddNewRowFormButtons);
                }
                //Issue: It cannot find it with this call:
                //oConfirmRowAddingButton = $("#" + properties.sAddNewRowOkButtonId, oAddNewRowForm);
                //oCancelRowAddingButton = $("#" + properties.sAddNewRowCancelButtonId, oAddNewRowForm);
                oConfirmRowAddingButton = $("#" + properties.sAddNewRowOkButtonId);
                oCancelRowAddingButton = $("#" + properties.sAddNewRowCancelButtonId);
            } else {
                oAddNewRowForm = null;
            }

            //Set the click handler on the "Delete selected row" button
            oDeleteRowButton = $('#' + properties.sDeleteRowButtonId);
            if (oDeleteRowButton.length != 0)
                oDeleteRowButton.click(_fnOnRowDelete);
            else {
                oDeleteRowButton = null;
            }

            //If an add and delete buttons does not exists but Add-delete toolbar is specificed
            //Autogenerate these buttons
            oAddDeleteToolbar = $(properties.sAddDeleteToolbarSelector);
            if (oAddDeleteToolbar.length != 0) {
                if (oAddNewRowButton == null && properties.sAddNewRowButtonId != ""
                    && oAddNewRowForm != null) {
                    oAddDeleteToolbar.append("<button id='" + properties.sAddNewRowButtonId + "' class='add_row'>Add</button>");
                    oAddNewRowButton = $("#" + properties.sAddNewRowButtonId);
                    oAddNewRowButton.click(function () { oAddNewRowForm.dialog('open'); });
                }
                if (oDeleteRowButton == null && properties.sDeleteRowButtonId != "") {
                    oAddDeleteToolbar.append("<button id='" + properties.sDeleteRowButtonId + "' class='delete_row'>Delete</button>");
                    oDeleteRowButton = $("#" + properties.sDeleteRowButtonId);
                    oDeleteRowButton.click(_fnOnRowDelete);
                }
            }

            //If delete button exists disable it until some row is selected
            if (oDeleteRowButton != null) {
                if (properties.oDeleteRowButtonOptions != null) {
                    oDeleteRowButton.button(properties.oDeleteRowButtonOptions);
                }
                fnDisableDeleteButton();
            }

            //If add button exists convert it to the JQuery-ui button
            if (oAddNewRowButton != null) {
                if (properties.oAddNewRowButtonOptions != null) {
                    oAddNewRowButton.button(properties.oAddNewRowButtonOptions);
                }
            }


            //If form ok button exists convert it to the JQuery-ui button
            if (oConfirmRowAddingButton != null) {
                if (properties.oAddNewRowOkButtonOptions != null) {
                    oConfirmRowAddingButton.button(properties.oAddNewRowOkButtonOptions);
                }
            }

            //If form cancel button exists convert it to the JQuery-ui button
            if (oCancelRowAddingButton != null) {
                if (properties.oAddNewRowCancelButtonOptions != null) {
                    oCancelRowAddingButton.button(properties.oAddNewRowCancelButtonOptions);
                }
            }

            //Add handler to the inline delete buttons
            $(".table-action-deletelink", oTable).live("click", function (e) {

                e.preventDefault();
                e.stopPropagation();
                var sURL = $(this).attr("href");

                if (sURL == null || sURL == "")
                    sURL = properties.sDeleteURL;

                iDisplayStart = fnGetDisplayStart();
                var oTD = ($(this).parents('td'))[0];
                var oTR = ($(this).parents('tr'))[0];

                $(oTR).addClass(properties.sSelectedRowClass);

                var id = fnGetCellID(oTD);
                if (properties.fnOnDeleting(oTD, id, fnDeleteRow)) {
                    fnDeleteRow(id, sURL);
                }


            }
            );

            //Add handler to the inline delete buttons
            $(".table-action-editlink", oTable).live("click", function (e) {

                e.preventDefault();
                e.stopPropagation();
                var sURL = $(this).attr("href");

                if (sURL == null || sURL == "")
                    sURL = properties.sDeleteURL;

                iDisplayStart = fnGetDisplayStart();
                var oTD = ($(this).parents('td'))[0];
                var oTR = ($(this).parents('tr'))[0];

                $(oTR).addClass(properties.sSelectedRowClass);

                var id = fnGetCellID(oTD);
                if (properties.fnOnDeleting(oTD, id, fnDeleteRow)) {
                    fnDeleteRow(id, sURL);
                }


            }
            );

            //Set selected class on row that is clicked
            //Enable delete button if row is selected, disable delete button if selected class is removed
            $("tbody", oTable).click(function (event) {
                if ($(event.target.parentNode).hasClass(properties.sSelectedRowClass)) {
                    $(event.target.parentNode).removeClass(properties.sSelectedRowClass);
                    if (oDeleteRowButton != null) {
                        fnDisableDeleteButton();
                    }
                } else {
                    $(oTable.fnSettings().aoData).each(function () {
                        $(this.nTr).removeClass(properties.sSelectedRowClass);
                    });
                    $(event.target.parentNode).addClass(properties.sSelectedRowClass);
                    if (oDeleteRowButton != null) {
                        fnEnableDeleteButton();
                    }
                }
            });


            if (properties.aoTableActions != null) {
                for (var i = 0; i < properties.aoTableActions.length; i++) {
                    var oTableAction = $.extend({ sType: "edit" }, properties.aoTableActions[i]);
                    var sAction = oTableAction.sAction;
                    var sActionFormId = oTableAction.sActionFormId;

                    var oActionForm = $("#form" + sAction);
                    if (oActionForm.length != 0) {
                        var oFormOptions = { autoOpen: false, modal: true };
                        oFormOptions = $.extend({}, oTableAction.oFormOptions, oFormOptions);
                        oActionForm.dialog(oFormOptions);
                        oActionForm.data("action-options", oTableAction);

                        var oActionFormLink = $(".table-action-" + sAction);
                        if (oActionFormLink.length != 0) {

                            oActionFormLink.live("click", function () {


                                var sClass = this.className;
                                var classList = sClass.split(/\s+/);
                                var sActionFormId = "";
                                var sAction = "";
                                for (i = 0; i < classList.length; i++) {
                                    if (classList[i].indexOf("table-action-") > -1) {
                                        sAction = classList[i].replace("table-action-", "");
                                        sActionFormId = "#form" + sAction;
                                    }
                                }
                                if (sActionFormId == "") {
                                    properties.fnShowError("Cannot find a form with an id " + sActionFormId + " that should be associated to the action - " + sAction, sAction)
                                }

                                var oTableAction = $(sActionFormId).data("action-options");

                                if (oTableAction.sType == "edit") {

                                    var oTD = ($(this).parents('td'))[0];
                                    var oTR = ($(this).parents('tr'))[0];

                                    $(oTR).addClass(properties.sSelectedRowClass);

                                    var iRowID = oTable.fnGetPosition(oTR);

                                    var id = fnGetCellID(oTD);

                                    $(sActionFormId).validate().resetForm();
                                    jQuery.data($(sActionFormId)[0], 'DATARECORDID', id);
                                    $("input.DATARECORDID", $(sActionFormId)).val(id);
                                    jQuery.data($(sActionFormId)[0], 'ROWID', iRowID);
                                    $("input.ROWID", $(sActionFormId)).val(iRowID);


                                    var oSettings = oTable.fnSettings();
                                    var iColumnCount = oSettings.aoColumns.length;


                                    $("input:text[rel],input:radio[rel][checked],input:hidden[rel],select[rel],textarea[rel],input:checkbox[rel]",
                                    $(sActionFormId)).each(function () {
                                        var rel = $(this).attr("rel");


                                        if (rel >= iColumnCount)
                                            properties.fnShowError("In the action form is placed input element with the name '" + $(this).attr("name") + "' with the 'rel' attribute that must be less than a column count - " + iColumnCount, "add");
                                        else {
                                            var sCellValue = oTable.fnGetData(oTR)[rel];
                                            if (this.nodeName.toLowerCase() == "select" || this.tagName.toLowerCase() == "select") {
                                                //sCellValue = $("option:selected", this).text();
                                                /*sCellValue = $.map(
                                                $.makeArray($("option:selected", this)),
                                                function (n, i) {
                                                return $(n).text();
                                                }).join(",");
                                                */
                                                //$(this).val(sCellValue);
                                                $(this).attr("value", sCellValue);

                                            }
                                            else if (this.nodeName.toLowerCase() == "span" || this.tagName.toLowerCase() == "span")
                                                $(this).html(sCellValue);
                                            else {
                                                if (this.type == "checkbox") {
                                                    if (sCellValue == "true") {
                                                        $(this).attr("checked", true);
                                                    }
                                                } else
                                                {
                                                    if(this.type == "radio"){
                                                        if(this.value == sCellValue){
                                                            this.checked = true;
                                                        }
                                                    }else{
                                                        this.value = sCellValue;
                                                    }
                                                }
                                            }

                                            //sCellValue = sCellValue.replace(properties.sIDToken, data);
                                            //values[rel] = sCellValue;
                                            //oTable.fnUpdate(sCellValue, iRowID, rel);
                                        }
                                    });


                                }
                                $(sActionFormId).dialog('open');
                            });
                        }

                        oActionForm.submit(function (event) {

                            var oTableAction = $(this).data("action-options");

                            if (oTableAction.sType == "edit") {
                                ///Start function fnUpdateRow
                                fnUpdateRow(this);
                                ///end function fnUpdateRow
                            } else {
                                fnAddRowFromForm(this);
                            }
                            return false;
                        });


                        var aActionFormButtons = new Array();

                        //var oActionSubmitButton = $("#form" + sAction + "Ok", oActionForm);
                        //aActionFormButtons.push(oActionSubmitButton);
                        var oActionFormCancel = $("#form" + sAction + "Cancel", oActionForm);
                        if (oActionFormCancel.length != 0) {
                            aActionFormButtons.push(oActionFormCancel);
                            oActionFormCancel.click(function () {

                                var oActionForm = $(this).parents("form")[0];
                                //Clear the validation messages and reset form
                                $(oActionForm).validate().resetForm();  // Clears the validation errors
                                $(oActionForm)[0].reset();

                                $(".error", $(oActionForm)).html("");
                                $(".error", $(oActionForm)).hide();  // Hides the error element
                                $(oActionForm).dialog('close');
                            });
                        }

                        //Convert all action form buttons to the JQuery UI buttons
                        $("button", oActionForm).button();
                        /*
                        if (aActionFormButtons.length > 0) {
                        oActionForm.dialog('option', 'buttons', aActionFormButtons);
                        }
                        */



                    }




                } // end for (var i = 0; i < properties.aoTableActions.length; i++)
            } //end if (properties.aoTableActions != null)


        });
    };
})(jQuery);
