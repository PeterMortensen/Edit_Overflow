
<?php

    # File deploymentSpecific.php


    # Mostly to isolate the deployment-specific part - there
    # is a reason for it not taking a parameter...
    #
    # The return value is a PDO instance
    #
    function connectToDatabase()
    {
        $datebaseServer_instance = 'mysql19';

        #'mysql:host=mysql19.unoeuro.com;dbname=pmortensen_eu_db',
	    #'mysql:host=mysql19.simply.com;dbname=pmortensen_eu_db;charset=utf8',
        #$datebaseServer_BaseDomain = 'unoeuro.com';
        $datebaseServer_BaseDomain = 'simply.com';

        $datebaseName = 'pmortensen_eu_db';


        # For simulating failures (e.g. for password leaks and
        # for regression testing)
        $PDO_DoFail = get_postParameter('PDO_DoFail');
        if ($PDO_DoFail === 'DNS') #Or simply take the domain as a parameter?
        {
            $datebaseServer_BaseDomain = 'simplyZZZZ.com';
        }


        # Sample:
        #
        #     mysql:host=mysql19.simply.com;dbname=pmortensen_eu_db;charset=utf8
        #
        $datebaseServer_address = $datebaseServer_instance . '.' .
                                  $datebaseServer_BaseDomain;
        $PDO_hostStr = 'mysql:host=' . $datebaseServer_address .
                       ';dbname=' . $datebaseName . ';charset=utf8';

        $pw = '__pw_toBeFilledInDuringDeployment_'; # Sample key: 2019-09-16



	    # Note:
	    #
	    #  "charset=utf8" is required for e.g. lookup of "lets" to work -
	    #  where 'correctTerm' contains the Quora quote character,
	    #  "U+2019  E2 80 99  RIGHT SINGLE QUOTATION MARK".
	    #
	    #  If not, the resulting content of 'correctTerm' is
	    #  "U+FFFD  EF BF BD  REPLACEMENT CHARACTER".
	    #
	    $somePDOinstance = new PDO($PDO_hostStr, 'pmortensen_eu', $pw);

        return $somePDOinstance;
    }

?>

