// debugging ------------------------------------------------
function logPrintn (str)
{
    window.external.logFromHTML(str);
}

function ShowResults()
{
   arg = event.type + "  fired by  " + event.srcElement.id;
   logPrintn(arg);
}

// ui routines ----------------------------------------------
function editsomething ()
{
   window.external.EditSomething(
      parseInt(window.event.srcElement.id.substring(7)),
      parseInt(window.event.srcElement.id.substring(9)));
}
// table based list routines --------------------------------
function clearlist(listname)
{
    var tbody = document.getElementById(listname);
    while (tbody.childNodes.length > 0)
    {
        tbody.removeChild(tbody.firstChild);
    }
}
// focus ----------------------------------------------------
function setFocus(itemname)
{
    var item = document.getElementById(itemname);
    item.focus();
}
// disable --------------------------------------------------
function disableElement(itemname, disable)
{
    window.document.all(itemname).disabled = (disable.length > 0 && parseInt(disable) != 0);
}
// input checkbox routines ----------------------------------
function sendcheck()
{
   window.external.CheckSomething(
      parseInt(window.event.srcElement.id.substring(7)),
      parseInt(window.event.srcElement.id.substring(9)),
      window.event.srcElement.checked
      );
}
// select (listbox) routines --------------------------------
function clearselect(listname)
{
    var lSelect = document.getElementById(listname);

    while (lSelect.options.length > 0)
    {
        lSelect.options.remove(0);
    }
}

function addoption(listname, name, id, selected)
{
   var lSelect = document.getElementById(listname);
   var oOption = document.createElement("OPTION");

   lSelect.options.add(oOption);
   oOption.innerText = name;
   oOption.value = id;

   if (selected)
   {
      oOption.selected = true;
   }
}
// --data routines ---------------------------------------

function insertHTML(itemid, textToInsert)   // this **INSERTS** vs replacing
{
   var obj_ta = window.document.all(itemid);
   obj_ta.innerHTML = window.document.all(itemid).innerHTML + textToInsert;
}
function getInnerHTML (itemid)
{
   return window.document.all(itemid).innerHTML;
}
function setInnerHTML (itemid, value)
{
   window.document.all(itemid).innerHTML = value;
}
function getInnerText (itemid)
{
   return window.document.all(itemid).innerText;
}
function setInnerText (itemid, value)
{
   window.document.all(itemid).innerText = value;
}
function setvalue(textid, text)
{
    var textElem = document.getElementById(textid);
    textElem.value = text;
}
function getvalue(textid)
{
    var textElem = document.getElementById(textid);
    return textElem.value;
}

function settext(textid, text)
{
    var textElem = document.getElementById(textid);
    textElem.value = text;
}
function gettext(textid)
{
    var textElem = document.getElementById(textid);
    return textElem.value;
}

function setcheck(checkid, onoff)
{
    var checkElem = document.getElementById(checkid);
    checkElem.checked = (onoff.length > 0 && parseInt(onoff) != 0);
}
function getcheck(checkid)
{
    var checkElem = document.getElementById(checkid);
    if (checkElem.checked == true)
    {
      return "1";
    }
    return "0";
}

function setselect(itemid, value)
{
    var selElem = document.getElementById(itemid);
    var ii;

    for (ii = 0; ii < selElem.options.length; ii++)
    {
        if (selElem.options(ii).value == value)
        {
            selElem.selectedIndex = ii;
            return;
        }
    }
}

// -- html editing functions -------------------------------
function tanExecCommand (itemid, command, value, flag)
{
   window.document.execCommand(command, flag, value);
}

function tanQueryCommandValue (itemid, command)
{
   return window.document.queryCommandValue(command);
}

// -- paste ------------------------------------------------

function tanOnPaste()
{
   // give tanjunka a chance to massage the data
   window.external.genericCallbackI(1);
}


// -- image scaling ----------------------------------------

//------pic stuff------
function tanPicEdit()
{
    tanHideMenu()
    logPrintn("pic edit:" + window.document.tanCurPic.src);
    window.external.genericCallbackISII_S(3, window.document.tanCurPic.src, 0, 0);
}
function tanPicResize()
{
    // let you change size by form
    tanHideMenu()
    logPrintn("resize:" + window.document.tanCurPic.src);
}
function tanPicProperties()
{
    // let you
    tanHideMenu()
    logPrintn("originalLink:" + window.document.tanCurPic.src);
}
function tanPicAlign(alignment)
{
    window.document.tanCurPic.align = alignment;
}
function tanPickMakeJPEG()
{
    tanHideMenu()
    logPrintn("originalLink:" + window.document.tanCurPic.src);
    var newPath = window.external.genericCallbackISII_S(2, window.document.tanCurPic.src, 0, 0);
    if (newPath.length > 0)
    {
        window.document.tanCurPic.src = newPath;
    }
}

// -- popmenus ---------------------------------------------

// Copyright 2002 Eddie Traversa, etraversa@dhtmlnirvana.com, http://dhtmlnirvana.com
// Free to use as long as this copyright notice stays intact.
// Courtesy of SimplytheBest.net - http://simplythebest.net/scripts/

// the code is based on Mr. Traversa's popups but has been heavily modified

function tanHideMenu()
{
    document.getElementById('tanpicmenu').style.visibility = 'hidden';
}

function tanPopupInit()
{
    document.attachEvent("onclick",tanHideMenu);
    document.attachEvent("onblur",tanHideMenu);
}

// ------------------------ init edit ----------------------
function tanInitEditForm()
{
    tanPopupInit();
//  var elem = document.getElementById("postbody");
//  elem.attachEvent("onbeforepaste", tanOnPaste);
}

// ------------------------ css stuff ----------------------

function changeCSS(styleURL)
{
    logPrintn("css=" + styleURL);
    document.styleSheets(0).href = styleURL;
}

