﻿"use strict";

const buzzbutton = document.getElementById("buzzbutton");
const resetbutton = document.getElementById("resetbutton");
const currentmessage = document.getElementById("currentmessage");
const hideifnomessage = document.getElementById("hideifnomessage");
const userlist = document.getElementById("userlist");
const ownerbuttons = document.getElementById("ownerbuttons");

function updateMessage (message) {
    currentmessage.textContent = message;
    hideifnomessage.hidden = currentmessage.textContent === "";
}

var connection = new signalR.HubConnectionBuilder().withUrl("/buzz").build();

async function start() {
    try {
        await connection.start();
        updateMessage("Connected!");
        connection.send("JoinRoom", roomId, userName);
    } catch (err) {
        console.log(err);
        setTimeout(() => start(), 5000);
    }
};

connection.onclose(async () => {
    updateMessage("Whoops, got disconnected. Hang on.")
    await start();
});

function starify(name, isHost) {
    return isHost ? `🌟 ${name} 🌟` : name;
}

connection.on("UpdateUserList", (users) => {
    userlist.innerHTML = "";
    let amRoomHost = false;
    for (const user of users) {
        const li = document.createElement("li");
        li.textContent = starify(user.name, user.isRoomHost);
        if (user.buzzedIn) {
            li.className = "buzzed-in";
            updateMessage(user.name + " buzzed in!");
            buzzbutton.disabled = true;
        }
        userlist.appendChild(li);

        if (user.isRoomHost && user.signalRId === connection.connectionId)
            amRoomHost = true;
    }
    ownerbuttons.hidden = !amRoomHost;
});

connection.on("SetButton", (shouldEnable) => {
    buzzbutton.disabled = !shouldEnable;
});

connection.on("SendMessage", updateMessage);

updateMessage("Connecting...");
// Start the connection.
start();

buzzbutton.onclick = function () { connection.send("BuzzIn"); };
window.onkeydown = function (ev) {
    console.log(ev);
    if (ev.repeat === false && ev.key === " ") {
        connection.send("BuzzIn");

        // avoid the weird scenario where you have the reset button selected, and hitting the space bar buzzes and resets.
        buzzbutton.focus();
    }
}

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
