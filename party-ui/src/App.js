import './App.css';
import {pm} from 'postmessage-polyfill';
import {fetch as fetchPolyfill} from 'whatwg-fetch';
import PartyWidget from "./Components/party-widget";

const originalInterval = window.setInterval;
window.setInterval = function (callback, delay = 0) {
    return originalInterval(callback, delay);
}

window.postMessage = function (message) {
    pm({
        type: message.type,
        origin: 'http://127.0.0.1/:9000',
        target: window,
        data: message,
    });
};

function App() {

    const page = window.page;

    return (
        <div className="Mod_Party_App">
            <PartyWidget />
        </div>
    );
}

export default App;
