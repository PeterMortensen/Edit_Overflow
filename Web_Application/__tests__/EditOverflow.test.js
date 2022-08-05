
/*   @jest-environment jsdom  */
//
// The above must be the very first in the file! Not even comments
// are allowed before. And it must be a C-type comment, not C++...
//
// Required in later versions of Jest.
//
// To avoid this error:
//
//     ReferenceError: window is not defined
//
//         > 31 | window.onload = function()


/////////////////////////////////////////////////////////////////////////
//
// Purpose: Unit test for the JavaScript part of Edit Overflow,
//          especially the main function, looking up terms
//          (typically a misspelled word).
//
//          It is using the Jest unit testing framework (that
//          runs under Nodes.js).
//
//
/////////////////////////////////////////////////////////////////////////





////////////////////////////////////////////////////////////////////////
// Lots of attempts to provide the global to EditOverflow.js so
// a Node.js-specific import is not needed there (that would
// also break it in the web browser)...
//
//const incorrect2correct = require('../EditOverflowList'); // For global "incorrect2correct"
//var { incorrect2correct } = require('../EditOverflowList'); // For global "incorrect2correct"
//const { incorrect2correct } = require('../EditOverflowList'); // For global "incorrect2correct"
//var incorrect2correct = require('../EditOverflowList'); // For global "incorrect2correct"
//incorrect2correct = require('../EditOverflowList'); // For global "incorrect2correct"
//@jest/globals.incorrect2correct = require('../EditOverflowList'); // For global "incorrect2correct"
//var @jest/globals.incorrect2correct = require('../EditOverflowList'); // For global "incorrect2correct"
//var global.incorrect2correct = require('../EditOverflowList'); // For global "incorrect2correct"

// For providing global "incorrect2correct" to EditOverflow.js
global.incorrect2correct = require('../EditOverflowList');



//const lookup = require('../EditOverflow'); // For the (central) lookup function
const { lookup } = require('../EditOverflow'); // For the (central) lookup function


//const incorrect2correct = require('./EditOverflowList');


//const get_action = require('../EditOverflow');


//import { incorrect2correct } from '../EditOverflowList';
//import incorrect2correct from '../EditOverflowList';


//const { filterByTerm } = require('../filterByTerm'); // Does not work... Why???

//const filterByTerm = require('filterByTerm'); // Does not work - "Cannot find module 'filterByTerm'
//                                              //                  from '__tests__/filterByTerm.test.js'"

//const filterByTerm = require('filterByTerm.js'); // Does not work - "Cannot find module 'filterByTerm.js'
//                                                 //                  from '__tests__/filterByTerm.test.js'"

//const filterByTerm = require('../filterByTerm.js'); // Works!



describe("Filter function", () =>
{
    // Helper function
    function lookup2(anIncorrectTerm)
    {
        //return incorrect2correct[anIncorrectTerm];
        return incorrect2correct.hasOwnProperty(anIncorrectTerm) ? incorrect2correct[anIncorrectTerm]: undefined;
    };


    test("Testing the central Edit Overflow lookup", () =>
    {
        const input = [
            {id: 1, url: "https://www.url1.dev" },
            {id: 2, url: "https://www.url2.dev" },
            {id: 3, url: "https://www.link3.dev"}
        ];

        const output = [{id: 3, url: "https://www.link3.dev"}];


        // Using the global directly (this can be phased out)

        expect(incorrect2correct["php"]).toEqual("PHP");
        expect(incorrect2correct["prototype"]).toEqual("Prototype");

        // Identity lookup
        expect(incorrect2correct["PHP"]).toEqual("PHP");

        // Using hasOwnProperty()
        expect(incorrect2correct.hasOwnProperty("PHP1978") ? incorrect2correct["PHP1978"]: undefined).toEqual(undefined);
        expect(incorrect2correct.hasOwnProperty("php") ? incorrect2correct["php"]: undefined).toEqual("PHP");

        // Failed lookups (expected)
        expect(incorrect2correct["PHP1978"]).toEqual(undefined);

            // Fails - actually returns "[object Object]", because
            // "__proto__" is built-in property in JavaScript...
            //expect(incorrect2correct["__proto__"]).toEqual(undefined);

        // "__proto__" is a built-in property in JavaScript...
        expect(incorrect2correct.hasOwnProperty("__proto__") ? incorrect2correct["__proto__"]: undefined).toEqual(undefined);


        // An API instead

          expect(lookup("php")).toEqual("PHP");
          expect(lookup("prototype")).toEqual("Prototype");

          // Identity lookup
          expect(lookup("PHP")).toEqual("PHP");

          expect(lookup("PHP1978")).toEqual(undefined);
          expect(lookup("__proto__")).toEqual(undefined);

    });
});

