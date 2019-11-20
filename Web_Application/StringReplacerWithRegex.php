
<?php
    class StringReplacerWithRegex
    {

        public $mCurrentString;


        public function __construct($aParam) 
        {
            $this->$mCurrentString = $aParam;
        }

        function currentString()
        {
            return $this->$mCurrentString;
        }

        function transform($aMatchString, $aReplaceSpecification)
        {
            $this->$mCurrentString = preg_replace(
                                       '/' . $aMatchString . '/',
                                       $aReplaceSpecification,
                                       $this->$mCurrentString);
        }

    }

?>

