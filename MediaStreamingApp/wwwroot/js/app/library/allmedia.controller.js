"use strict";

angular.module('libraryApp')
    .controller('AllMediaController', ['$scope', '$route', 'SharedDataService', function($scope, $route, SharedDataService) {
        SharedDataService.set('numMediaFiles', 123, true);
    }]);