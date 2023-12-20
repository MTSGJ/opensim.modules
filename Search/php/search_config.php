<?php

// default
$SRCH_DB_HOST   = OPENSIM_DB_HOST;
$SRCH_DB_NAME   = OPENSIM_DB_NAME;
$SRCH_DB_USER   = OPENSIM_DB_USER;
$SRCH_DB_PASS   = OPENSIM_DB_PASS;
$SRCH_DB_MYSQLI = OPENSIM_DB_MYSQLI;

if (OSSEARCH_DB=='HELPER' and defined('HELPER_DB_NAME')) {
    $SRCH_DB_HOST   = HELPER_DB_HOST;
    $SRCH_DB_NAME   = HELPER_DB_NAME;
    $SRCH_DB_USER   = HELPER_DB_USER;
    $SRCH_DB_PASS   = HELPER_DB_PASS;
    $SRCH_DB_MYSQLI = HELPER_DB_MYSQLI;
} 


if (OSSEARCH_DB=='HELPER' or OSSEARCH_DB=='OPENSIM') {
    define('SEARCH_ALLPARCELS_TBL_BASE',    'search_allparcels');
    define('SEARCH_EVENTS_TBL_BASE',        'search_events');
    define('SEARCH_HOSTSREGISTER_TBL_BASE', 'search_hostsregister');
    define('SEARCH_OBJECTS_TBL_BASE',       'search_objects');
    define('SEARCH_PARCELS_TBL_BASE',       'search_parcels');
    define('SEARCH_PARCELSALES_TBL_BASE',   'search_parcelsales');
    define('SEARCH_POPULARPLACES_TBL_BASE', 'search_popularplaces');
    define('SEARCH_REGIONS_TBL_BASE',       'search_regions');
}
else {  // NONE
    define('SEARCH_ALLPARCELS_TBL_BASE',    '');
    define('SEARCH_EVENTS_TBL_BASE',        '');
    define('SEARCH_HOSTSREGISTER_TBL_BASE', '');
    define('SEARCH_OBJECTS_TBL_BASE',       '');
    define('SEARCH_PARCELS_TBL_BASE',       '');
    define('SEARCH_PARCELSALES_TBL_BASE',   '');
    define('SEARCH_POPULARPLACES_TBL_BASE', '');
    define('SEARCH_REGIONS_TBL_BASE',       '');
}

