//HoldOn Page Loader Events
$(document).ready(function () {
    initLoadHoldOn();
});

$(window).on('load',function() {
    HoldOn.close(); 
});

//$(document).on('submit', 'form', function () {
//    initLoadHoldOn();
//});

$(document).on('invalid-form.validate', 'form', function () {
    HoldOn.close();
});


$(document).ajaxStart(function () {
    initLoadHoldOn(setMessageIn);
});

$(document).ajaxStop(function () {
    HoldOn.close();
});

function initLoadHoldOnClose() {
    
    HoldOn.close();
}
var setMessageIn = "";
function SetMessageIn(messageIn)
{
    
    setMessageIn = messageIn;
}
function initLoadHoldOn(messageIn) {
    
    if (messageIn == "" || messageIn == null)
    {
        messageIn = 'Loading..Please Wait';
    }
    
    HoldOn.open({
        theme: "sk-cube-grid",
        message: messageIn,
        textColor: "white",
    });
    messageIn = "";
}
function initLoadHoldOnCustom(messageIn) {

    HoldOn.open({
        theme: "sk-cube-grid",
        message: messageIn,
        textColor: "white",
    });
}