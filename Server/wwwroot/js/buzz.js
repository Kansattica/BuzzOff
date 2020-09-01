"use strict";

const buzzbutton = document.getElementById("buzzbutton");
const resetbutton = document.getElementById("resetbutton");
const currentmessage = document.getElementById("currentmessage");
const hideifnomessage = document.getElementById("hideifnomessage");
const userlist = document.getElementById("userlist");

var connection = new signalR.HubConnectionBuilder().withUrl("/buzz").build();

async function start() {
    try {
        await connection.start();
        console.log("connected");
        connection.send("JoinRoom", roomId, userName);
    } catch (err) {
        console.log(err);
        setTimeout(() => start(), 5000);
    }
};

connection.onclose(async () => {
    await start();
});

connection.on("UpdateUserList", (users) => {
    userlist.innerHTML = "";
    for (const user of users) {
        const roomHostStar = user.isRoomHost ? '🌟 ' : '';
        const li = document.createElement("li");
        li.textContent = roomHostStar + user.name;
        if (user.buzzedIn) {
            li.className = "buzzed-in";
        }
        userlist.appendChild(li);

        if (user.isRoomHost && user.signalRId === connection.connectionId)
            resetbutton.hidden = false;
    }
});

connection.on("SetButton", (shouldEnable) => {
    buzzbutton.disabled = !shouldEnable;
});

connection.on("SendMessage", (message) => {
    currentmessage.textContent = message;
    hideifnomessage.hidden = currentmessage.textContent === "";
});

// Start the connection.
start();

buzzbutton.onclick = function () { connection.send("BuzzIn"); };
resetbutton.onclick = function () { connection.send("Reset"); };

function updateName() {
    const newName = document.getElementById("newname").value;

    if (newName !== userName) {
        userName = newName;
        connection.send("UpdateName", userName);
    }
}

const updatename = document.getElementById("updatename");

updatename.onclick = updateName;
document.getElementById("newname").onkeydown = function (ev) {
    if (ev.repeat === false && ev.code === "Enter")
        updateName();
}
