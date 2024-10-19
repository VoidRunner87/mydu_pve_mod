
fetch('stubs/faction-quests.json', {
    method: 'GET'
}).then(res => {
    return  res.json();
}).then(json => {
    console.log(json);
    window.modApi.setResourceContents('faction-quests', 'application/json', JSON.stringify(json));
});

fetch('stubs/player-quests.json', {
    method: 'GET'
}).then(res => {
    return  res.json();
}).then(json => {
    console.log(json);
    window.modApi.setResourceContents('player-quests', 'application/json', JSON.stringify(json));
});