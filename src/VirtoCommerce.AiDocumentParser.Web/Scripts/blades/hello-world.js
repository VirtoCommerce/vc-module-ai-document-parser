angular.module('AiDocumentParser')
    .controller('AiDocumentParser.helloWorldController', ['$scope', 'AiDocumentParser.webApi', function ($scope, api) {
        var blade = $scope.blade;
        blade.title = 'AiDocumentParser';

        blade.refresh = function () {
            api.get(function (data) {
                blade.title = 'AiDocumentParser.blades.hello-world.title';
                blade.data = data.result;
                blade.isLoading = false;
            });
        };

        blade.refresh();
    }]);
