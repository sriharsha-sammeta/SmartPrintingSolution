(function () {
    var App = angular.module("App.services", []);
    App.factory('ApiURLs', function () {
        //var apiBaseUrl = 'https://10.107.162.86:4043/SmartPrintingApi/api/';
        var drWhomApiUrl = 'http://who/PeopleStore.asmx/FindPersonByAlias';
        var apiBaseUrl = 'http://smartprintingapidemo.azurewebsites.net/api/';
        var baseUrls = {
            'get_print_jobs': apiBaseUrl + 'printjob/active',
            'delete_job': apiBaseUrl + 'printjob/delete',
            'update_count': apiBaseUrl + 'printjob/updateNumberOfCopies',
            'change_state': apiBaseUrl + 'printjob/changestate',
            'delegate_job': apiBaseUrl + 'printjob/assignproxy',
            'drWhom': drWhomApiUrl,
            'get_print_jobs_log': apiBaseUrl + 'printjob/getlog',

            //Trusted zone URLs
            'get_delegates': apiBaseUrl + 'delegate/all',
            'add_delegate': apiBaseUrl + 'delegate/add',
            'change_delegate_status': apiBaseUrl + 'delegate/changeState' 
        }
        return angular.extend(baseUrls, {});
    });

})();