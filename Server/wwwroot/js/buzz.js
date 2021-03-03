"use strict";

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
const colorblind = document.getElementById("colorblind");

let userName = "";
const roomId = document.getElementById("roomname").textContent;

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
		connstatus.textContent = "Connected!";
		connstatus.className = "connected";
		connection.send("JoinRoom", roomId, userName);
	} catch (err) {
		console.log(err);
		setTimeout(() => start(), 5000);
	}
};

connection.onclose(async () => {
	updateMessage("Disconnected. If it doesn't come back in a few seconds, try refreshing.")
	connstatus.textContent = "Disconnected";
	connstatus.className = "disconnected";
	await start();
});

connection.onreconnecting((err) => {
	updateMessage("Reconnecting...");
	connstatus.textContent = "Reconnecting...";
	connstatus.className = "connecting";
	console.log(err);
});

connection.onreconnected(() => {
	updateMessage("");
	connstatus.textContent = "Connected!";
	connstatus.className = "connected";
	connection.send("JoinRoom", roomId, userName);
});

function surround(name, shouldSurround, emoji) {
	return shouldSurround ? `${emoji}\xa0${name}\xa0${emoji}` : name;
}

function updateName(newName) {
	if (newName !== userName) {
		userName = newName;
		connection.send("UpdateName", userName);
	}
}

updatename.onclick = function () { updateName(newname.value); };

var buzzShouldBeDisabled = false;

let currentRoom = undefined;

function updateRoom(room) {
	const users = room.users;

	buzzbutton.textContent = surround("Buzz!", room.isPrelocked, '🔒');

	buzzbutton.disabled = buzzShouldBeDisabled = !room.buzzButtonEnabled;

	userlist.innerHTML = "";
	userlist.className = users.length >= 10 ? "multicol" : "onecol";
	if (users.length === 1) {
		listheader.textContent = "Player:";
	} else {
		listheader.textContent = users.length + " Players:";
	}

	const buzzOrder = (room.maxBuzzedIn === 1 && room.buzzedInIds.length === 1) ? ['🐝'] : (colorblind.checked ? ["1️⃣", "2️⃣", "3️⃣"] : ["🥇", "🥈", "🥉"]);

	let amRoomHost = false;
	const buzzedInUsers = [];
	for (const user of users) {
		const li = document.createElement("li");

		// \xa0 is a non-breaking space
		// this should be handled serverside, but nothing wrong with double-checking
		user.name = user.name.replace(/\s+/, '\xa0');
		li.textContent = surround(surround(surround(user.name, user.isHost, '🌟'), user.buzzedIn, buzzOrder[room.buzzedInIds.indexOf(user.signalRId)]), user.lockedOut, '🔒');

		if (user.buzzedIn) {
			li.className = "buzzed-in";
			buzzedInUsers[room.buzzedInIds.indexOf(user.signalRId)] = user.name;
		}

		if (user.isHost)
			li.id = "hostentry";

		userlist.appendChild(li);

		if (user.signalRId === connection.connectionId) {
			amRoomHost = user.isHost;

			buzzbutton.disabled = buzzShouldBeDisabled || user.buzzedIn || user.lockedOut;

			userName = user.name;

			if (newname !== document.activeElement)
				newname.value = user.name;
		}
	}

	if (room.maxBuzzedIn === 1 && room.buzzedInIds.length === 1)
	{
		updateMessage(buzzedInUsers[0] + " buzzed in!");
	}
	else if (buzzedInUsers.length >= 1)
	{
		const ul = document.createElement("ul");
		for (let i = 0; i < room.buzzedInIds.length; i++)
		{
			let name = buzzedInUsers[i];
			const li = document.createElement("li");
			if (name === undefined) {
				name = "(user disconnected)"
				li.className = "disconnected";
			}
			li.textContent = surround(name, true, buzzOrder[i]);
			ul.appendChild(li)
		}
		currentmessage.textContent  = '';
		currentmessage.appendChild(ul);
		hideifnomessage.hidden = false;
	}

	if (amRoomHost) {
		prelock.disabled = room.isPrelocked;
		unlock.disabled = !room.isPrelocked;
	}

	hostbuttons.hidden = !amRoomHost;
	for (let i = 0; i < countselect.options.length; i++) {
		if (parseInt(countselect.options[i].value) === room.maxBuzzedIn) {
			countselect.selectedIndex = i;
			modedisplay.textContent = "Game Mode: " + countselect.options[i].textContent;
			return;
		}
	}
}

connection.on("UpdateRoom", (room) => {
	updateRoom(currentRoom = room);
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
colorblind.onchange = function () { updateRoom(currentRoom); };
