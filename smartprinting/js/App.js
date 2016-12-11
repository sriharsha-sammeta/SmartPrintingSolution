(function () {
    var App = angular.module('App', ['ui.router', 'ui.bootstrap', 'AdalAngular', 'App.controllers', 'App.services', 'ngToast']);

    App.config(function ($stateProvider, $urlRouterProvider, $locationProvider, $httpProvider, adalAuthenticationServiceProvider) {
        $urlRouterProvider.otherwise('/');
        var endpoints = {
            'https://10.107.162.86:4043/SmartPrintingApi/api': 'https://SmartPrintingService',
        };


        $httpProvider.defaults.headers.common = {};
        $httpProvider.defaults.headers.post = {};
        $httpProvider.defaults.headers.put = {};
        $httpProvider.defaults.headers.patch = {};
        adalAuthenticationServiceProvider.init(
            {
                instance: 'https://login.microsoftonline.com/',
                tenant: 'microsoft.onmicrosoft.com',
                clientId: '811b3250-c227-49d8-a977-dd95ce1891ea',
                clientSecret : '30KKiRbHut6SLx2C00zlFlIInlsQq8tl2eJcIN4I8hI=',
                //instance: 'https://login.windows.net/',
                //tenant: 'microsoft.onmicrosoft.com',
                //clientId: '1e3e7e3a-61e3-4ed9-b60e-baec5490edd0',
                //extraQueryParameter: 'nux=1',
                //clientSecret: 'qbrqdvsQiz0fOH1S1ojALo9yrBEAAgzTqkyGzS2XBxA=',
                endpoints: endpoints
                //clientid: '819271c6-4bc3-4580-b4b0-39dc270424c7'
            },
            $httpProvider
        );        

        $stateProvider
            .state('dashboard', {
                url: '/',
                controller: 'DashboardController',
                templateUrl: 'Views/DashboardView.html',
                requireADLogin: true
            })
            .state('archives', {
                url: '/archives',
                controller: 'ArchiveController',
                templateUrl: 'Views/ArchiveView.html',
                requireADLogin: true
            })
            .state('trustedzone', {
                url: '/trustedzone',
                controller: 'TrustedZoneController',
                templateUrl: 'Views/TrustedZoneView.html',
                requireADLogin: true
            })
    });

})();