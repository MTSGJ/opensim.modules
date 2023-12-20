<?php

// default
$PROF_DB_HOST   = OPENSIM_DB_HOST;
$PROF_DB_NAME   = OPENSIM_DB_NAME;
$PROF_DB_USER   = OPENSIM_DB_USER;
$PROF_DB_PASS   = OPENSIM_DB_PASS;
$PROF_DB_MYSQLI = OPENSIM_DB_MYSQLI;

if (OSPROFILE_DB=='HELPER' and defined('HELPER_DB_NAME')) {
    $PROF_DB_HOST   = HELPER_DB_HOST;
    $PROF_DB_NAME   = HELPER_DB_NAME;
    $PROF_DB_USER   = HELPER_DB_USER;
    $PROF_DB_PASS   = HELPER_DB_PASS;
    $PROF_DB_MYSQLI = HELPER_DB_MYSQLI;
}


// Table Base Name
if (OSPROFILE_DB=='HELPER' or OSPROFILE_DB=='OPENSIM') {
    define('PROFILE_CLASSIFIEDS_TBL_BASE',  'profile_classifieds');
    define('PROFILE_USERNOTES_TBL_BASE',    'profile_usernotes');
    define('PROFILE_USERPICKS_TBL_BASE',    'profile_userpicks');
    define('PROFILE_USERPROFILE_TBL_BASE',  'profile_userprofile');
    define('PROFILE_USERSETTINGS_TBL_BASE', 'profile_usersettings');
}
else {  // NONE (UserProfileModule)
    define('PROFILE_CLASSIFIEDS_TBL_BASE',  'classifieds');
    define('PROFILE_USERNOTES_TBL_BASE',    'usernotes');
    define('PROFILE_USERPICKS_TBL_BASE',    'userpicks');
    define('PROFILE_USERPROFILE_TBL_BASE',  'userprofile');
    define('PROFILE_USERSETTINGS_TBL_BASE', 'usersettings');
}

