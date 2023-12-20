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


function AcceptRequest(id, id2, requestId) {
    $.ajax({
        url: `/Home/AcceptRequest?userId=${id}&senderId=${id2}&requestId=${requestId}`,
        method: "GET",
        success: function (data) {
            let element = document.querySelector("#alert");
            element.style.display = "block";
            element.innerHTML = "You accept request successfully";
            GetAllUsers();
            SendFollowCall(id);
            SendFollowCall(id2);
            GetMyRequests();
            GetAllUsers();

            setTimeout(() => {
                element.innerHTML = "";
                element.style.display = "none";
            }, 5000);
        }
    })
}

function DeleteRequest(recieverId, requestId) {
    $.ajax({
        url: `/Home/DeleteRequest?requestId=${requestId}`,
        method: "GET",
        success: function (data) {
            SendFollowCall(recieverId);
        }
    })
}


function DeclineRequest(id, senderId) {
    $.ajax({
        url: `/Home/DeclineRequest?id=${id}&senderId=${senderId}`,
        method: "GET",
        success: function (data) {
            let element = document.querySelector("#alert");
            element.style.display = "block";
            element.innerHTML = "You declined request";
            SendFollowCall(senderId);
            GetMyRequests();
            GetAllUsers();

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
                if (data[i].status == "Request") {
                    subContent = `
                    <div class='card-body'>
                        <button class='btn btn-success' onclick="AcceptRequest('${data[i].senderId}','${data[i].receiverId}','${data[i].id}')">Accept </button>
                        <button class='btn btn-secondary' onclick="DeclineRequest(${data[i].id},'${data[i].senderId}')">Decline</button>
                    </div>
                    `;
                }
                else {
                    subContent = ` <div class='card-body'>
                        <button class='btn btn-secondary' onclick="DeleteRequest('${data[i].receiverId}','${data[i].id}')">Delete</button>
                    </div>`;
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

function UnFollowCall(id) {
    $.ajax({
        url: `/Home/UnFollow?id=${id}`,
        method: "GET",
        success: function (data) {
            GetMyRequests();
            GetAllUsers();
            SendFollowCall(id);
        }
    })
}

function GetFriends() {
    $.ajax({
        url: `/Home/GetMyFriends`,
        method: "GET",
        success: function (data) {
            let content = "";
            for (var i = 0; i < data.length; i++) {
                let css = 'border:3px solid springgreen';
                if (!data[i].yourFriend.isOnline) {
                    css = 'border:3px solid red';
                }
                let item = `
                <section style='display:flex;width:300px;border:2px solid deepskyblue;margin-top:10px;padding:15px;border-radius:15px;'>
                <img style='width:60px;height:60px;border-radius:50%;${css}'
                src='/images/${data[i].yourFriend.imageUrl}'
                />

                <section style='margin-left:20px;'>
                    <h4>${data[i].yourFriend.userName}</h4>
                    <a class='btn btn-outline-success'>Go Chat</a>
                </section>

                </section>
                `;
                content += item;
            }
            $("#friends").html(content);
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


                if (data[i].hasRequestPending) {
                    subContent = `<button  class='btn btn-outline-secondary'> Already Sent</button>`;
                }
                else {
                    if (data[i].isFriend) {

                        subContent = `<button  class='btn btn-outline-secondary' onclick="UnFollowCall('${data[i].id}')"> UnFollow</button>`;
                    }
                    else {

                        subContent = `<button onclick="SendFollow('${data[i].id}')" class='btn btn-outline-primary'> Follow</button>`;
                    }
                }

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
            GetFriends();
        }
    })
}

GetAllUsers();
GetMyRequests();