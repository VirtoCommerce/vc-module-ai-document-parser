angular.module('AiDocumentParser')
    .factory('AiDocumentParser.webApi', ['$resource', function ($resource) {
        return $resource('api/ai-document-parser');
    }]);
