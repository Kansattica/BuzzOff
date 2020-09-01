const output = document.getElementById("output");
const roomname = document.getElementById("roomname");
const baseUrl = new URL('/', location.href).href;
if (!baseUrl.endsWith('/'))
    baseUrl += '/';

roomname.oninput = function() {
    roomname.size = Math.max(roomname.value.length, 30);
    output.innerText = output.href = roomname.value.length === 0 ? "" : encodeURI(baseUrl + "Room/" + roomname.value);
};

roomname.oninput();