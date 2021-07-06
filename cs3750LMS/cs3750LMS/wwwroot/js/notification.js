using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.wwwroot.js
{
    public class notification   //TODO: FIX AND CHANGE THIS CODE!!!
    {
        "use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/notificationHub").build();

    //Disable update grade button until connection is established
    document.getElementById("submitGrade").disabled = true;
   

    connection.on("ReceiveMessage", function (item, message) {
        var li = document.createElement("li");
        document.getElementById("notificationList").appendChild(li);
        // We can assign user-supplied strings to an element's textContent because it
        // is not interpreted as markup. If you're assigning in any other way, you 
        // should be aware of possible script injection concerns.
        li.textContent = `${item} says ${message}`;
    });

    connection.start().then(function () {
        document.getElementById("submitGrade").disabled = false;
    }).catch(function (err) {
        return console.error(err.toString());
    });

    document.getElementById("submitGrade").addEventListener("click", function (event) {
        var item = document.getElementById("AssignmentID").value;
        var message = 'Changes Have Been Made.'
        connection.invoke("SendMessage", item, message).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    });
    }
}
