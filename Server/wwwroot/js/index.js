"use strict";

const output = document.getElementById("output");
const roomname = document.getElementById("roomname");
const baseUrl = function () {
	let base = new URL('/', location.href).href;
	if (!base.endsWith('/'))
		base += '/';
	return base;
}();

roomname.onfocus = function() {
	roomname.className = "";
};

roomname.oninput = function() {
	roomname.size = Math.max(roomname.value.length, 44);
	output.innerText = output.href = roomname.value.length === 0 ? "" : encodeURI(baseUrl + "Room/" + roomname.value);
};

roomname.oninput();