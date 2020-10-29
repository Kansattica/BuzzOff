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
const makesound = document.getElementById("makesound");
const buzzsound = document.getElementById("buzzsound");
const connstatus = document.getElementById("connstatus");
const modedisplay = document.getElementById("modedisplay");
const countselect = document.getElementById("countselect");

let userName = newname.value;
const roomId = document.getElementById("roomname").innerText;

function updateMessage(message) {
	currentmessage.textContent = message;
	hideifnomessage.hidden = currentmessage.textContent === "";
}

if (typeof (signalr) === "undefined") {
	updateMessage("Couldn't load signalr.js.");
}

var connection = new signalR.HubConnectionBuilder().withUrl("/buzz").withAutomaticReconnect().build();

async function start() {
	try {
		await connection.start();
		updateMessage("");
		connstatus.innerText = "Connected!";
		connstatus.className = "connected";
		connection.send("JoinRoom", roomId, userName);
	} catch (err) {
		console.log(err);
		setTimeout(() => start(), 5000);
	}
};

connection.onclose(async () => {
	updateMessage("Disconnected. If it doesn't come back in a few seconds, try refreshing.")
	connstatus.innerText = "Disconnected";
	connstatus.className = "disconnected";
	await start();
});

connection.onreconnecting((err) => {
	updateMessage("Reconnecting...");
	connstatus.innerText = "Reconnecting...";
	connstatus.className = "connecting";
	console.log(err);
});

connection.onreconnected(() => {
	updateMessage("");
	connstatus.innerText = "Connected!";
	connstatus.className = "connected";
	connection.send("JoinRoom", roomId, userName);
});

function surround(name, shouldSurround, emoji) {
	return shouldSurround ? `${emoji} ${name} ${emoji}` : name;
}

function updateName(newName) {
	if (newName !== userName) {
		userName = newName;
		connection.send("UpdateName", userName);
	}
}

updatename.onclick = function () { updateName(newname.value); };

var firstTime = true;
var buzzShouldBeDisabled = false;

connection.on("UpdateRoom", (room) => {
	const users = room.users;

	buzzbutton.innerText = surround("Buzz!", room.isPrelocked, '🔒');

	buzzbutton.disabled = buzzShouldBeDisabled = !room.buzzButtonEnabled;

	userlist.innerHTML = "";
	if (users.length === 1) {
		listheader.innerText = "Player:";
	} else {
		listheader.innerText = users.length + " Players:";
	}

	const buzzOrder = room.buzzedInIds.length == 1 ? ['🐝'] : ["🥇", "🥈", "🥉"];

	let amRoomHost = false;
	for (const user of users) {
		const li = document.createElement("li");
		li.textContent = surround(surround(surround(user.name, user.isHost, '🌟'), user.buzzedIn, buzzOrder[room.buzzedInIds.indexOf(user.signalRId)]), user.lockedOut, '🔒');

		if (user.buzzedIn) {
			li.className = "buzzed-in";
			updateMessage(user.name + " buzzed in!");
		}
		userlist.appendChild(li);

		if (user.signalRId === connection.connectionId) {
			amRoomHost = user.isHost;

			buzzbutton.disabled = buzzShouldBeDisabled || user.buzzedIn || user.lockedOut;

			// if the server tells us our name changed, change it
			if (user.name !== userName)
				userName = newname.value = user.name;
		}
		else if (firstTime && userName === user.name) {
			firstTime = false;
			// if our randomly generated name is the same as another's, we have to change
			// but only if we just got here. The server will handle it otherwise.
			updateName("");
		}
	}
	firstTime = false;
	if (amRoomHost) {
		prelock.disabled = room.isPrelocked;
		unlock.disabled = !room.isPrelocked;
	}
	hostbuttons.hidden = !amRoomHost;
	for (let i = 0; i < countselect.options.length; i++) {
		if (parseInt(countselect.options[i].value) === room.maxBuzzedIn) {
			countselect.selectedIndex = i;
			modedisplay.innerText = "Game Mode: " + countselect.options[i].innerText;
			return;
		}
	}
});

connection.on("SendMessage", updateMessage);

connection.on("Buzz", (shouldBuzz) => {
	buzzsound.muted = !makesound.checked;
	if (!shouldBuzz) {
		buzzsound.pause();
		buzzsound.currentTime = 0;
		return;
	}

	if (makesound.checked && buzzsound.paused)
		buzzsound.play();
});

updateMessage("Connecting...");
// Start the connection.
start();

buzzbutton.onclick = function () { connection.send("BuzzIn"); };

const keyFunc = {
	" ": function () {
		connection.send("BuzzIn");
		// avoid the weird scenario where you have the reset button selected, and hitting the space bar buzzes and resets.
		buzzbutton.focus();
	},
	"r": function () { connection.send("Reset"); },
	"p": function () { connection.send("SetPrelock", true); },
	"l": function () { connection.send("SetPrelock", true); },
	"u": function () { connection.send("SetPrelock", false); }
}

window.onkeydown = function (ev) {
	const key = ev.key.toLowerCase();
	if (ev.repeat || !keyFunc.hasOwnProperty(key)) { return; }
	keyFunc[key]();
}

resetbutton.onclick = function () { connection.send("Reset"); };

newname.onkeydown = function (ev) {
	ev.stopPropagation();
	if (ev.repeat === false && ev.key === "Enter")
		updateName(newname.value);
}

randomname.onclick = function () { updateName("") };

prelock.onclick = function () { connection.send("SetPrelock", true); }
unlock.onclick = function () { connection.send("SetPrelock", false); }
countselect.onchange = function (ev) { connection.send("UpdateMaxBuzzedIn", parseInt(ev.target.value)); };
