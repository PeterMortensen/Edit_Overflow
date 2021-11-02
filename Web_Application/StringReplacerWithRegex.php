
<?php

    # Purpose: 1) Simplifies the client side of transforming a
    #             string using regular expressions, including
    #             successively applying several regular expressions.
    #
    #          2) Method compatiple with the corresponding C# in the
    #             .NET part of Edit Overflow (thus easier to keep in
    #             sync - and preparation for a data-driven approach)
    #
    #          3) Frees the client of carrying the current state of
    #             the string (e.g. double reference to it for every
    #             transformation)

    class StringReplacerWithRegex
    {

        public $mCurrentString;


        public function __construct($aParam)
        {
            $this->mCurrentString = $aParam;
        }

        function currentString()
        {
            return $this->mCurrentString;
        }

        function transform($aMatchString, $aReplaceSpecification)
        {
            # Not used: "s" is for "." matching newlines.

            $this->mCurrentString = preg_replace(
                                       '/' . $aMatchString . '/',
                                       $aReplaceSpecification,
                                       $this->mCurrentString);
        }

        # Use for tests (using regular expressions) 
        # on the current state of the string
        #
        function match($aMatchString)
        {
            $toReturn = preg_match('/' . $aMatchString . '/',
                                   $this->mCurrentString);
            return $toReturn;
        }

    }

?>


