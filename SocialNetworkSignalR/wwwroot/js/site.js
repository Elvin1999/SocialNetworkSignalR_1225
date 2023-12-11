function GetAllUsers() {

    $.ajax({
        url: "/Home/GetAllUsers",
        method: "GET",

        success: function (data) {
            let content = "";
            for (var i = 0; i < data.length; i++) {
                let item = `
                <div class='card' style='width:14rem; height:5px;margin:5px;'>
                    <img style='width:220px;height:220px;' src='/images/${data[i].imageUrl}'/>
                    <div class='card-body'>
                        <h5 class='card-title'>${data[i].userName}</h5>
                        <p class='card-text'>${data[i].email}</p>
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