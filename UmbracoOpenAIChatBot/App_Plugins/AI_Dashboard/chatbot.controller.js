angular.module("umbraco").controller("ChatbotController", function ($http, userService) {
  var vm = this;

  vm.messages = [];
  vm.currentUser = ''; // initieer een variabele voor de huidige gebruiker

  // Haal de naam van de ingelogde gebruiker op
  userService.getCurrentUser().then(function (user) {
    vm.currentUser = user.name;
  });

  vm.clearMessages = function () {
    $http.post('/umbraco/api/chatai/clearmessages', { User: vm.currentUser });
    vm.messages = [];
  }

  vm.sendMessage = function () {
    // Voeg het nieuwe bericht toe aan de lijst van berichten
    vm.messages.push({ content: vm.newMessageContent, type: 'sent' });

    // Stuur het nieuwe bericht naar de server
    $http.post('/umbraco/api/chatai/sendmessage', { Content: vm.newMessageContent, User: vm.currentUser })
      .then(function (response) {
        // Hier kun je eventuele logica toevoegen om de serverrespons te verwerken
        console.log('Bericht succesvol verzonden:', response.data[0]);
        vm.messages.push(response.data[0]);
      }, function (error) {
        console.error('Fout tijdens het verzenden van het bericht: ', error);
      });

    // Wis het invoerveld na het verzenden
    vm.newMessageContent = '';
  };
});
