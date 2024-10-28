
fetch('stubs/player-party4.json', {
    method: 'GET'
}).then(res => {
    return  res.json();
}).then(json => {
    console.log(json);
    window.modApi.setResourceContents('player-party', 'application/json', JSON.stringify(json));
});