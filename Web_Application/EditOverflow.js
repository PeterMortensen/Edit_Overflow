
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
        //var correct_out = "No match!!!!" + " YYY3";
        //
        // We return the input so blind application (e.g. by a macro
        // keybaord) will not overwrite the original text (or rather,
        // replace it with the same).
        //
        var correct_out = input;
    }
    else
    {
        //Easier to see if JavaScript is used or not. Remove when all
        //this is established/stable.
        //var correct_out = correct + " XXX2";

        // The space is to be output compatible with the current
        // form-based lookup
        var correct_out = correct + " ";
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

