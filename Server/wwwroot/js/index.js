const output = document.getElementById("output");
const roomname = document.getElementById("roomname");
const baseUrl = new URL('/', location.href).href;
if (!baseUrl.endsWith('/'))
    baseUrl += '/';

roomname.oninput = () => {
    output.innerText = output.href = encodeURI(baseUrl + "Room/" + roomname.value);
}