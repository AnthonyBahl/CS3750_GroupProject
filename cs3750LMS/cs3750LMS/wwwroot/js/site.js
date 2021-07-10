


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

function removeNotificationItem(id) {
    alert("Notification has not been deleted yet.");

    //this was my attempt to delete the notifications using ajax to call a repository function in the home controller 

    //$.post("/HomeController/RemoveNotification", id, function (data, status) {
    //    alert("Data:" + data + " Status" + status);
    //});


    //This was the code in the home controller but has now been removed.
    //        public ActionResult RemoveNotification(int id)
    //{
    //    _notification.DeleteNotification(id);   //calling the repository object _notification.DeleteNotification(id) pass in the notification id to be deleted. 
    //    return View();
    //}

    //My resource for this method is https://stackoverflow.com/questions/20766306/calling-a-c-sharp-function-by-a-html-button
    // and https://www.w3schools.com/jquery/jquery_ajax_get_post.asp
    //Maybe you can figure it out. 

}


//END --------------  Notification Bell In Navbar