
// ***********************************************************************
//
// Purpose: Client-side lookup of words (in contrast to web form submit
//          (server side)) - the primary function of Edit Overflow.
//
//          In JavaScript. Intended to both run in a web browser and
//          under Node.js for testing (using Jest).
//
// ***********************************************************************



// Note: The global array 'incorrect2correct' (our word list) is provided
//       from the outside, either by Node.js/Jest during testing or
//       by a "<script>" tag in the HTML (EditOverflowList.js).




//Delete at any time
//document.write("<h1>Hello, World!</h1>");

//Delete at any time
// Node.js specific? Yes. And lookup will not work on the web
// if it is enabled...
//const incorrect2correct = require('./EditOverflowList');



window.onload = function()
{
    //document.lookupForm.action = get_action();
}


function lookup(anIncorrectTerm)
{
    //Delete at any time
    //return incorrect2correct[anIncorrectTerm];

    var i2c = incorrect2correct; // An alias(?) Or a real copy (is it
                                 // costly or not)???

    // hasOwnProperty() is to avoid false positive matches for
    // built-in stuff in JavaScript, e.g. "__proto__"
    //
    return i2c.hasOwnProperty(anIncorrectTerm) ? i2c[anIncorrectTerm]: undefined;
};


function get_action()
{
    //window.alert("Hi from get_action...");

    var input = document.getElementById("lookupForm").elements.namedItem("LookUpTerm").value;

    var correct = lookup(input);

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


//Why do we have this here?? For initial lookup for the default content?
//var lookup = "php";
//var correct7 = incorrect2correct[lookup];



//document.write("<h2>Lookup value: " + correct + " </h2>");

// Node.js specific
//module.exports = lookup;
module.exports = { lookup };


