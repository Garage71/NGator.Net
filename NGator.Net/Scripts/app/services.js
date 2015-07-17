"use strict";

/* Services */

var ngatorServices = angular.module('ngatorServices', ['ngResource']);

ngatorServices.factory('SourcesService', ['$resource',
  function ($resource) {
      return $resource('api/sources', {}, {
          getSources: { method: 'GET', params: { }, isArray: true }
      });
  }]);
