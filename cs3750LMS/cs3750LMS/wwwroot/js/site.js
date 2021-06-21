


//Notification Bell In Navbar
//-------------------------------------------
const bellIcon = document.getElementById("bellIcon");

let ItemsCount = 3; // we need to get the amount of items passed into the notifications list and store it here. 

if (ItemsCount > 0) {

    bellIcon.style.color = "#D94B2B"; // set color to orange
    bellIcon.classList.add("ringAnimation"); //add animation to bell here.  NOTE: ORDER MATTERS COLOR SET MUST OCCURE BEFORE ANIMATION IS ADDED. 

} else {

    bellIcon.style.color = "#495057";  //set color to nav-link gray. 
    bellIcon.classList.remove("ringAnimation"); //remove animation
    }

function removeNotificationItem() {
   //TODO: THIS METHOD IS FOR WHEN THE [X] ICON IS CLICKED. 
}


//END --------------  Notification Bell In Navbar