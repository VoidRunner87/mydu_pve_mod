import './App.css';
import logo from './assets/Gameface_white.png'
import { pm } from 'postmessage-polyfill';
import { fetch as fetchPolyfill } from 'whatwg-fetch';
import QuestList from "./Components/QuestList";

const originalInterval = window.setInterval;
window.setInterval = function(callback, delay = 0) {
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
  return (
    <div className="Mod_DE_App">
        <QuestList />
    </div>
  );
}

export default App;
