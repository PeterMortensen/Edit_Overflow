
//document.write("<h1>Hello, World!</h1>");


window.onload = function() {
    //document.lookupForm.action = get_action();
}


function get_action() {
    //return form_action;

    //window.alert("Hi from get_action...");

    var input = document.getElementById("lookupForm").elements.namedItem("LookUpTerm").value;
    //input = "js";

    var correct = incorrect2correct[input];

    if (correct === undefined)
    {
        var correct_out = "No match!!!!" + " YYY3";
    }
    else
    {
        var correct_out = correct + " XXX2";
        //var correct_out = correct;
    }

    document.getElementById("lookupForm").elements.namedItem("CorrectedTerm").value = correct_out;

    //window.alert("Correct: " + correct);

    return false;
}


//window.alert("Hi!")

var lookup = "php";
var correct = incorrect2correct[lookup];

//document.write("<h2>Lookup value: " + correct + " </h2>");

