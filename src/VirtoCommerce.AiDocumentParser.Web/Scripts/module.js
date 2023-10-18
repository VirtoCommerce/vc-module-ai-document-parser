// Call this to register your module to main application
var moduleName = 'AiDocumentParser';

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .config(['$stateProvider',
        function ($stateProvider) {
            $stateProvider
                .state('workspace.AiDocumentParserState', {
                    url: '/AiDocumentParser',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        'platformWebApp.bladeNavigationService',
                        function (bladeNavigationService) {
                            var newBlade = {
                                id: 'blade1',
                                controller: 'AiDocumentParser.helloWorldController',
                                template: 'Modules/$(VirtoCommerce.AiDocumentParser)/Scripts/blades/hello-world.html',
                                isClosingDisabled: true,
                            };
                            bladeNavigationService.showBlade(newBlade);
                        }
                    ]
                });
        }
    ])
    .run(['platformWebApp.mainMenuService', '$state',
        function (mainMenuService, $state) {
            //Register module in main menu
            var menuItem = {
                path: 'browse/AiDocumentParser',
                icon: 'fa fa-cube',
                title: 'AiDocumentParser',
                priority: 100,
                action: function () { $state.go('workspace.AiDocumentParserState'); },
                permission: 'AiDocumentParser:access',
            };
            mainMenuService.addMenuItem(menuItem);
        }
    ]);
