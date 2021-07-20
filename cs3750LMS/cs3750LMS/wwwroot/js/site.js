


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

document.querySelectorAll('.noti').forEach(item => item.addEventListener('mousedown', function (e) {
    console.log(e)
    console.log(e.target)
    e.target.parentNode.parentNode.parentNode.style.display = "none";
}));

document.getElementById('notiDropdown').addEventListener('click', function (e) {
    e.stopPropagation();
    e.preventDefault();
    console.log("WTF");
    if (document.getElementById('notiMenu').style.display === "none") {
        document.getElementById('notiMenu').style.display = "block";
    } else {
        document.getElementById('notiMenu').style.display = "none";
    }
});



//END --------------  Notification Bell In Navbar