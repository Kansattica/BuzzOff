"use strict";

const buzzbutton = document.getElementById("buzzbutton");
const resetbutton = document.getElementById("resetbutton");
const currentmessage = document.getElementById("currentmessage");
const hideifnomessage = document.getElementById("hideifnomessage");
const userlist = document.getElementById("userlist");
const ownerbuttons = document.getElementById("ownerbuttons");
const newname = document.getElementById("newname");
const updatename = document.getElementById("updatename");
const randomname = document.getElementById("randomname");
const listheader = document.getElementById("listheader");

let userName = newname.value;
const roomId = document.getElementById("roomname").innerText;

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
    updateMessage("Disconnected. If it doesn't come back in a few seconds, try refreshing.")
    await start();
});

function surround(name, shouldSurround, emoji) {
    return shouldSurround ? `${emoji} ${name} ${emoji}` : name;
}

function updateName() {
    const newName = newname.value;

    if (newName !== userName) {
        userName = newName;
        connection.send("UpdateName", userName);
    }
}

function randomName() {
    newname.value = "";
    updateName();
}

updatename.onclick = updateName;

var firstTime = true;

connection.on("UpdateUserList", (users) => {
    userlist.innerHTML = "";
    if (users.length === 1) {
        listheader.innerText = "User:";
    } else {
        listheader.innerText = users.length + " Users:";
    }

    let amRoomHost = false;
    for (const user of users) {
        const li = document.createElement("li");
        li.textContent = surround(surround(user.name, user.buzzedIn, '🐝'), user.isRoomHost, '🌟');
        
        if (user.buzzedIn) {
            li.className = "buzzed-in";
            updateMessage(user.name + " buzzed in!");
            buzzbutton.disabled = true;
        }
        userlist.appendChild(li);

        if (user.signalRId === connection.connectionId) {
            amRoomHost = user.isRoomHost;

            // if the server tells us our name changed, change it
            if (user.name !== userName)
                userName = newname.value = user.name;
        }
        else
        {
            // if our randomly generated name is the same as another's, we have to change
            // but only if we just got here
            if (firstTime && userName === user.name) {
                randomName();
            }
        }
    }
    firstTime = false;
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

function pressedKey(ev, keyCode) {
    return ev.repeat === false && ev.code === keyCode;
}

window.onkeydown = function (ev) {
    if (pressedKey(ev, "Space")) {
        connection.send("BuzzIn");

        // avoid the weird scenario where you have the reset button selected, and hitting the space bar buzzes and resets.
        buzzbutton.focus();
    }
    else if (pressedKey(ev, "KeyR")) {
        connection.send("Reset");
    }
}

resetbutton.onclick = function () { connection.send("Reset"); };

newname.onkeydown = function (ev) {
    if (pressedKey(ev, "Enter"))
        updateName();
}

randomname.onclick = randomName;
