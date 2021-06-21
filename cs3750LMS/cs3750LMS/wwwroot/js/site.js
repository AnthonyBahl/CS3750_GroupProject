


//Notification Bell In Navbar
//-------------------------------------------
const bellIcon = document.getElementById("bellIcon");

let ItemsCount = 3; // we need to get the amount of items passed into the notifications list and store it here. 

if (ItemsCount > 0) {

        //bellIcon.classList.add("") //add animation to bell here. 

        bellIcon.style.color = "#D94B2B"; // set color to orange

    } else {
        bellIcon.style.color = "#495057";  //set color to nav-link gray. 
    }

function removeNotificationItem() {
   // document.getElementById("");
}


//END --------------  Notification Bell In Navbar