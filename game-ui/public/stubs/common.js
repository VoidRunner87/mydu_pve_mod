// setInterval(() => {
//     CPPHud.addFailureNotification("Test");
//     CPPMod.sendModAction("Mod.Honjo.Server", 1000001, [], JSON.stringify({ hello: "world" }));
// }, 1000);

if (!window.global_resources) {
    window.global_resources = {};
}

let modApi = {};
modApi.cb = (data) => {
    console.log("Mod.DynamicEncounters", 1999999, [], JSON.stringify(data));
};

modApi.setResourceContents = (name, contentType, contents) => {
    const blob = new Blob([contents], {type: contentType});
    const blobUrl = URL.createObjectURL(blob);
    window.global_resources[name] = blobUrl;

    modApi.cb(`Set Resource ${name} as ${contentType} to ${blobUrl}`);

    return window.global_resources[name];
};

modApi.appendIframe = (id, url) => {
    const iframe = document.createElement('iframe');

    iframe.id = id;
    iframe.className = "frame";
    iframe.src = url;
    iframe.width = '800';
    iframe.height = '600';
    iframe.style.border = 'none';

    document.body.appendChild(iframe);

    return iframe;
}

modApi.addInlineCss = (cssContent) => {
    let style = document.createElement("style");
    style.type = "text/css";
    style.innerHTML = cssContent;
    document.head.appendChild(style);
}

modApi.addJsUrl = (src) => {
    let script = document.createElement("script");
    script.src = src;
    document.body.appendChild(script);
}

modApi.addJs = (type, text) => {
    let script = document.createElement("script");
    script.type = type;
    script.textContent = text;
    document.body.appendChild(script);
}

modApi.removeAppRoot = () => {
    document.getElementById("root").remove();
}

window.modApi = modApi;
