'use strict';

// Workaround fix for Angular 1.4 vs ui-bootstrap 0.13 modal animation incompatibility
// See issue https://github.com/angular-ui/bootstrap/issues/3633
ngatorControllers.directive('removeModal', ['$document', function ($document) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            element.bind('click', function () {
                $document[0].body.classList.remove('modal-open');
                angular.element($document[0].getElementsByClassName('modal-backdrop')).remove();
                angular.element($document[0].getElementsByClassName('modal')).remove();
            });
        }
    };
}]);