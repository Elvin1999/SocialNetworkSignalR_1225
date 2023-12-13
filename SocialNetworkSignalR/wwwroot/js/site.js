function SendFollow(id) {
    $.ajax({
        url: `/Home/SendFollow/${id}`,
        method: "GET",
        success: function (data) {
            let element = document.querySelector("#alert");
            element.style.display = "block";
            element.innerHTML = "You friend request sent successfully";
            GetAllUsers();
            SendFollowCall(id);
            setTimeout(() => {
                element.innerHTML = "";
                element.style.display = "none";
            }, 5000);
        }
    })
}


function GetMyRequests() {
    $.ajax({
        url: "/Home/GetAllRequests",
        method: "GET",
        success: function (data) {
            let content = "";
            let subContent = "";
            console.log(data);
            for (var i = 0; i < data.length; i++) {
                if (data[i].status = "Request") {
                    subContent = `
                    <div class='card-body'>
                        <button class='btn btn-success'>Accept </button>
                        <button class='btn btn-secondary'>Decline</button>
                    </div>
                    `;
                }

                let item = `<div class='card' style='width:15rem;'>
                        <div class='card-body'> 
                            <h5 class='card-title'>Friend Request </h5>
                        </div>
                               <ul class='list-group list-group-flush'>
                <li class='list-group-item'>${data[i].content} </li>
                </ul>
                    ${subContent}
                </div>`;

                content += item;
            }
            $("#requests").html(content);
        }
    })
}


function GetAllUsers() {

    $.ajax({
        url: "/Home/GetAllUsers",
        method: "GET",

        success: function (data) {
            let content = "";
            for (var i = 0; i < data.length; i++) {
                let dateContent = "";
                let style = '';
                let subContent = "";

                subContent = `<button onclick="SendFollow('${data[i].id}')" class='btn btn-outline-primary'> Follow</button>`;

                if (data[i].isOnline) {
                    style = 'border:5px solid springgreen;';
                }
                else {
                    style = 'border:5px solid red;';
                    let disconnectedDate = new Date(data[i].disconnectTime);
                    let currentDate = new Date();
                    let diffTime = Math.abs(currentDate - disconnectedDate);
                    let diffMinutes = Math.ceil(diffTime / (1000 * 60));
                    if (diffMinutes >= 60) {
                        diffMinutes = Math.ceil(diffMinutes / 60);
                        dateContent = `<span class='btn btn-warning' > Left ${diffMinutes} hours ago</span>`;
                    }
                    else {
                        dateContent = `<span class='btn btn-warning' > Left ${diffMinutes} minutes ago</span>`;
                    }

                }

                let item = `
                <div class='card' style='${style}width:14rem;margin:5px;'>
                    <img style='width:100%;height:220px;' src='/images/${data[i].imageUrl}'/>
                    <div class='card-body'>
                        <h5 class='card-title'>${data[i].userName}</h5>
                        <p class='card-text'>${data[i].email}</p>
                        ${subContent}
                        <p class='card-text mt-2'>${dateContent}</p>
                    </div>
                </div>
                `;
                content += item;
            }
            $("#allusers").html(content);
        }
    })
}

GetAllUsers();