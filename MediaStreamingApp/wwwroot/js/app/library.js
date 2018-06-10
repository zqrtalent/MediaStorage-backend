angular.module('libraryApp', ['ngRoute'])
    .config(['$routeProvider', '$locationProvider', function($routeProvider, $locationProvider) {
        $routeProvider.when('/', {
                templateUrl: '/js/app/library/templates/allmedia.html',
                controller: 'AllMediaController',
                resolve: {
                    init: ['SharedDataService', function(SharedDataService) {
                        SharedDataService.set('pageName', 'allmedia', true);
                        return true;
                    }]
                }
            })
            .when('/upload', {
                templateUrl: '/js/app/library/templates/media.upload.start.html',
                controller: 'UploadController',
                resolve: {
                    init: ['SharedDataService', function(SharedDataService) {
                        SharedDataService.set('pageName', 'upload', true);
                    }],
                    uploadSyncInfo: ['uploadService', function(uploadService) {
                        return uploadService.getUploadSyncInfo();
                    }]
                }
            })
            .when('/upload/complete', {
                templateUrl: '/js/app/library/templates/media.upload.complete.html',
                controller: 'UploadCompleteController',
                resolve: {
                    init: ['SharedDataService', function(SharedDataService) {
                        SharedDataService.set('pageName', 'upload', true);
                    }],
                    uploadSyncInfo: ['uploadService', function(uploadService) {
                        return uploadService.getUploadSyncInfo();
                    }]
                }
            })
            .otherwise('/');
    }]);

/*Shared data service*/
angular.module('libraryApp').factory('SharedDataService', ['$rootScope', function($rootScope) {
    var sharedData = {};

    sharedData.objects = {};
    sharedData.set = function(name, value, triggerEvent) {
        sharedData.objects[name] = value;
        if (triggerEvent === true)
            $rootScope.$emit('sharedDataValueUpdated', { name: name, value: value });
    }

    sharedData.get = function(name) {
        return sharedData.objects[name];
    }

    sharedData.persist = function() {
        localStorage.setItem('libraryApp.SharedData', JSON.stringify(sharedData.objects));
    }

    sharedData.load = function() {
        var objects = JSON.parse(localStorage.getItem('libraryApp.SharedData'));
        sharedData.objects = objects || {};
    }
    return sharedData;
}]);