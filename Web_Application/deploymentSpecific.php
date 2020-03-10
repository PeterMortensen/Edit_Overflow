
<?php

    # File deploymentSpecific.php


    # Mostly to isolate the deployment-specific part - there
    # is a reason for it not taking a parameter...
    #
    # The return value is a PDO instance
    #
    function connectToDatabase()
    {
	     # Note:
	     #
	     #  "charset=utf8" is required for e.g. lookup of "lets" to work -
	     #  where 'correctTerm' contains the Quora quote character,
	     #  "U+2019  E2 80 99  RIGHT SINGLE QUOTATION MARK".
	     #
	     #  If not, the resulting content of 'correctTerm' is
	     #  "U+FFFD  EF BF BD  REPLACEMENT CHARACTER".
	     #
	     $somePDOinstance = new PDO(
	       #'mysql:host=mysql19.unoeuro.com;dbname=pmortensen_eu_db',
	       'mysql:host=mysql19.simply.com;dbname=pmortensen_eu_db;charset=utf8',
	       'pmortensen_eu',
	       '__pw_toBeFilledInDuringDeployment_'); # Sample key: 2019-09-16

        return $somePDOinstance;
    }

?>

