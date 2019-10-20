function updateStatus(status) {
    $('#status').text(status.message);

    const isStreaming = !!status.isStreaming;
    $('#startButton').prop('disabled', isStreaming);
    $('#stopButton').prop('disabled', !isStreaming);
    $('#pauseButton').prop('disabled', !isStreaming);
}

async function addTracks(tracks) {
    if (this.connection) {
        await this.connection.invoke("AddTracks", tracks);
    }
}

async function removeTrack(track) {
    if (this.connection) {
        await this.connection.invoke("RemoveTrack", track);
    }
}

async function startStream() {
    if (this.connection) {
        await connection.invoke("Start");
    }
}

async function stopStream() {
    if (this.connection) {
        await connection.invoke("Stop");
    }
}

async function pauseStream() {
    if (this.connection) {
        await connection.invoke("Pause");
    }
}

function registerEventHandlers(connection) {
    connection.on('StatusUpdated', status => {
        this.updateStatus(status);
    });

    connection.on('TweetReceived', tweet => {
        $('#tweets').append(`<li>${tweet.html}</li>`);
        this.twttr.widgets.load();
    });

    connection.onreconnecting(error => {
        console.assert(connection.state === signalR.HubConnectionState.Reconnecting);
        console.error(error);
    });

    connection.onreconnected(() => {
        console.assert(connection.state === signalR.HubConnectionState.Connected);
    });
}

function buildHubConnection(hubUrl) {
    return new signalR.HubConnectionBuilder()
        .withUrl(hubUrl)
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();
}

async function start(hubUrl, tracks) {
    if (!hubUrl) {
        return;
    }

    this.connection = buildHubConnection(hubUrl);

    try {
        registerEventHandlers(connection);
        await connection.start();

        console.assert(connection.state === signalR.HubConnectionState.Connected);
        console.log('connected');

        await connection.invoke("AddTracks", tracks);

    } catch (err) {
        console.assert(connection.state === signalR.HubConnectionState.Disconnected);
        console.error(err);
        setTimeout(() => start(), 5000);
    }
}