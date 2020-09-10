﻿"use strict";

const buzzbutton = document.getElementById("buzzbutton");
const resetbutton = document.getElementById("resetbutton");
const currentmessage = document.getElementById("currentmessage");
const hideifnomessage = document.getElementById("hideifnomessage");
const userlist = document.getElementById("userlist");
const hostbuttons = document.getElementById("hostbuttons");
const newname = document.getElementById("newname");
const updatename = document.getElementById("updatename");
const randomname = document.getElementById("randomname");
const listheader = document.getElementById("listheader");
const prelock = document.getElementById("prelock");
const unlock = document.getElementById("unlock");

let userName = newname.value;
const roomId = document.getElementById("roomname").innerText;

function updateMessage (message) {
	currentmessage.textContent = message;
	hideifnomessage.hidden = currentmessage.textContent === "";
}

var connection = new signalR.HubConnectionBuilder().withUrl("/buzz").withAutomaticReconnect().build();

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

connection.onreconnecting((err) => {
	updateMessage("Reconnecting...");
	console.log(err);
});

connection.onreconnected(() => {
	updateMessage("Reconnected!");
	connection.send("JoinRoom", roomId, userName);
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
		li.textContent = surround(surround(surround(user.name, user.buzzedIn, '🐝'), user.isRoomHost, '🌟'), user.lockedOut, '🔒');
		
		if (user.buzzedIn) {
			li.className = "buzzed-in";
			updateMessage(user.name + " buzzed in!");
			buzzbutton.disabled = true;
		}
		userlist.appendChild(li);

		if (user.signalRId === connection.connectionId) {
			amRoomHost = user.isRoomHost;

			buzzbutton.disabled = user.lockedOut;

			// if the server tells us our name changed, change it
			if (user.name !== userName)
				userName = newname.value = user.name;
		}
		else if (firstTime && userName === user.name) {
			// if our randomly generated name is the same as another's, we have to change
			// but only if we just got here
				randomName();
		}
	}
	firstTime = false;
	hostbuttons.hidden = !amRoomHost;
});

connection.on("SetButton", (shouldEnable) => {
	buzzbutton.disabled = !shouldEnable;
});

connection.on("PrelockStatus", (isPrelocked) => {
	buzzbutton.innerText = surround("Buzz!", isPrelocked, '🔒');
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
	} else if (pressedKey(ev, "KeyP")) {
		connection.send("SetPrelock", true);
	}  else if (pressedKey(ev, "KeyU")) {
		connection.send("SetPrelock", false);
	}
}

resetbutton.onclick = function () { connection.send("Reset"); };

newname.onkeydown = function (ev) {
	if (pressedKey(ev, "Enter"))
		updateName();
}

randomname.onclick = randomName;

prelock.onclick = function () { connection.send("SetPrelock", true); }
unlock.onclick = function () { connection.send("SetPrelock", false); }
