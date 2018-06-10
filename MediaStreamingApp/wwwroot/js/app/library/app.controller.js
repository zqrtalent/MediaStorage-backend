(function(angular) {
    "use strict";

    angular.module('libraryApp')
        .controller('MainController', ['$scope', '$rootScope', '$route', 'SharedDataService', function($scope, $rootScope, $route, SharedDataService) {
            $scope.NumMediaFiles = SharedDataService.get('numMediaFiles');
            $scope.PageName = SharedDataService.get('pageName');
            $rootScope.$on('sharedDataValueUpdated', function(event, dataChanged) {
                if (dataChanged.name === 'pageName')
                    $scope.PageName = dataChanged.value;
                else
                if (dataChanged.name === 'numMediaFiles')
                    $scope.NumMediaFiles = dataChanged.value;
            });

            $rootScope.$on('$routeChangeStart', function(event) {
                SharedDataService.persist();
            });

            $rootScope.$on('$routeChangeSuccess', function(event) {
                SharedDataService.load();
            });

            $rootScope.$on('$routeChangeError', function(event) {
                SharedDataService.load();
            });
        }]);
})(angular);