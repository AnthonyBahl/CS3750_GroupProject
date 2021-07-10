


//Notification Bell In Navbar
//-------------------------------------------
const bellIcon = document.getElementById("bellIcon");

let ItemsCount = document.getElementById("PassingNotificationCountToJavaScript").value; 

if (ItemsCount > 0) {

    bellIcon.style.color = "#D94B2B"; // set color to orange
    bellIcon.classList.add("ringAnimation"); //add animation to bell here.  NOTE: ORDER MATTERS COLOR SET MUST OCCURE BEFORE ANIMATION IS ADDED. 

} else {

    bellIcon.style.color = "#495057";  //set color to nav-link gray. 
    bellIcon.classList.remove("ringAnimation"); //remove animation
    }

function removeNotificationItem() {
    alert("Notification has been seen");
    //set the DateViewed in the assignment to DateTime.Now
}


//END --------------  Notification Bell In Navbar