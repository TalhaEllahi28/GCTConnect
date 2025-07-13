
// ========== ON DOM LOAD: Load Groups and Friends ==========
document.addEventListener("DOMContentLoaded", function () {
    loadUserGroups();
    loadFriends();
});

function loadUserGroups() {
    fetch('/GetGroupsAndMessages/GetUserGroups')
        .then(response => response.json())
        .then(groups => {
            let groupList = '';
            groups.forEach(group => {
                groupList += `
                    <li>
                        <div class="conversation-item" onclick="selectGroup(this); loadMessages(${group.groupId}, '${group.groupName}')">
                            <div class="conversation-avatar">
                                <div class="avatar">
                                    <div class="avatar-placeholder">${getInitials(group.groupName)}</div>
                                    <div class="status-indicator online"></div>
                                </div>
                            </div>
                            <div class="conversation-info">
                                <div class="conversation-header">
                                    <div class="conversation-name">${group.groupName}</div>
                                    <div class="conversation-time">Now</div>
                                </div>
                                <div class="conversation-last-message">
                                    Click to view messages
                                </div>
                            </div>
                        </div>
                    </li>
                `;
            });
            document.querySelector('#groups-ul').innerHTML = groupList;
        })
        .catch(error => console.error('Error fetching groups:', error));
}

function loadFriends() {
    fetch('/GetGroupsAndMessages/GetFriends')
        .then(response => response.json())
        .then(friends => {
            let friendList = '';
            friends.forEach(friend => {
                friendList += `
                    <li>
                        <div class="conversation-item" onclick="selectGroup(this); openChatWithFriend(${friend.userId}, '${friend.username}')">
                            <div class="conversation-avatar">
                                <div class="avatar">
                                    ${friend.profilePic ? `<img src="${friend.profilePic}" class="avatar-img" />` : `<div class="avatar-placeholder">${getInitials(friend.username)}</div>`}
                                    <div class="status-indicator online"></div>
                                </div>
                            </div>
                            <div class="conversation-info">
                                <div class="conversation-header">
                                    <div class="conversation-name">${friend.name} ${friend.lastName}</div>
                                    <div class="conversation-time">Now</div>
                                </div>
                                <div class="conversation-last-message">
                                    Click to start chat
                                </div>
                            </div>
                        </div>
                    </li>
                `;
            });
            document.querySelector('#friends-ul').innerHTML = friendList;
        })
        .catch(error => console.error('Error fetching friends:', error));
}

function getInitials(name) {
    if (!name) return '?';

    const words = name.trim().split(' ');
    if (words.length === 1) {
        return name.substring(0, 2).toUpperCase();
    } else {
        return (words[0].charAt(0) + words[words.length - 1].charAt(0)).toUpperCase();
    }
}

// Add hover sound effect (commented out but can be enabled if desired)
document.addEventListener('DOMContentLoaded', function () {
    // Handle filtering groups
    const searchInput = document.getElementById('group-search');
    if (searchInput) {
        searchInput.addEventListener('input', filterGroups);
    }

    // Add subtle hover effect
    document.addEventListener('mouseover', function (event) {
        if (event.target.closest('.conversation-item')) {
            const item = event.target.closest('.conversation-item');
            item.style.transition = 'all 0.2s ease';
        }
    });
});

function selectGroup(elem) {
    const items = document.querySelectorAll('.conversation-item');
    items.forEach(item => item.classList.remove('active'));

    elem.classList.add("active");

    setTimeout(() => {
        const container = document.getElementById("messages-container");
        if (container) {
            container.scrollTop = container.scrollHeight;
        }
        else {
            console.log("not found");
        }
    }, 150);
}

function loadMessages(groupId, groupName) {
    fetch(`/GetGroupsAndMessages/GetGroupMessages?groupId=${groupId}`)
        .then(response => response.text())
        .then(partialViewHtml => {
            document.querySelector('#chat-container').innerHTML = partialViewHtml;
            setupSignalR(groupId, false);
            attachSendButtonListener(groupId, "group");
        })
        .catch(error => console.error('Error loading group messages:', error));
}

function openChatWithFriend(friendId, friendName) {
    fetch(`/GetGroupsAndMessages/GetFriendsMessages?friendId=${friendId}`)
        .then(response => response.text())
        .then(partialViewHtml => {
            document.querySelector('#chat-container').innerHTML = partialViewHtml;

            setTimeout(() => {
                const container = document.getElementById("messages-container");
                if (container) {
                    container.scrollTop = container.scrollHeight;
                }
            }, 50);

            setupSignalR(friendId, true);
            attachSendButtonListener(friendId, "friends");
        })
        .catch(error => console.error('Error loading friend chat:', error));

    setTimeout(() => {
        const container = document.getElementById("messages-container");
        if (container) {
            container.scrollTop = container.scrollHeight;
        }
    }, 100);
}

let connection;
let mediaRecorder;
let audioChunks = [];
let isRecording = false;
let currentChatId = null;
let currentChatType = null;

function setupSignalR(identifier, isPrivate) {
    if (connection) {
        connection.stop(); // Stop previous connection if any
    }

    // Store current chat info globally
    currentChatId = identifier;
    currentChatType = isPrivate ? "friends" : "group";

    connection = new signalR.HubConnectionBuilder()
        .withUrl('/Services/ChatHub') // Ensure correct endpoint
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.on('ReceiveMessage', function (user, message, profilePic, fileUrl, fileType, fileName, audioUrl) {
        const chatContainer = document.getElementById('messages-container');
        const currentUser = chatContainer.dataset.currentUser;
        const messageClass = (user === currentUser) ? "sent" : "received";

        let messageContent = '';

        // Add text message if present
        if (message && message.trim() !== '') {
            messageContent += `<p>${message}</p>`;
        }

        // Add file preview if present
        if (fileUrl) {
            if (fileType && fileType.startsWith('image/')) {
                messageContent += `
                    <div class="file-preview">
                        <img src="${fileUrl}" alt="${fileName || 'Image'}" class="image-preview" />
                        <a href="${fileUrl}" download="${fileName}" class="file-download">${fileName || 'Download image'}</a>
                    </div>`;
            } else if (fileType && fileType.startsWith('video/')) {
                messageContent += `
                    <div class="file-preview">
                        <video src="${fileUrl}" controls class="video-preview"></video>
                        <a href="${fileUrl}" download="${fileName}" class="file-download">${fileName || 'Download video'}</a>
                    </div>`;
            } else if (fileType && fileType.startsWith('audio/')) {
                messageContent += `
                    <div class="file-preview">
                        <audio src="${fileUrl}" controls class="audio-preview"></audio>
                        <a href="${fileUrl}" download="${fileName}" class="file-download">${fileName || 'Download audio'}</a>
                    </div>`;
            } else {
                messageContent += `
                    <div class="file-preview">
                        <div class="generic-file">
                            <svg class="file-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                                <polyline points="14 2 14 8 20 8"></polyline>
                                <line x1="16" y1="13" x2="8" y2="13"></line>
                                <line x1="16" y1="17" x2="8" y2="17"></line>
                                <line x1="10" y1="9" x2="8" y2="9"></line>
                            </svg>
                            <span class="file-name">${fileName || 'File'}</span>
                        </div>
                        <a href="${fileUrl}" download="${fileName}" class="file-download">Download file</a>
                    </div>`;
            }
        }

        // Add audio message if present
        if (audioUrl) {
            messageContent += `
                <div class="file-preview">
                    <audio src="${audioUrl}" controls class="audio-preview"></audio>
                    <a href="${audioUrl}" download="voice-message.wav" class="file-download">Download voice message</a>
                </div>`;
        }

        const newMessage = `
            <div class="message-container ${messageClass}" data-sender="${user}">
                <div class="avatar-wrapper">
                    <img src="${profilePic}" alt="${user}" class="avatar-image"/>
                </div>
                <div class="message-content">
                    <div class="message-bubble">
                        ${messageContent}
                    </div>
                    <span class="timestamp">
                        ${new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                    </span>
                </div>
            </div>
        `;
        chatContainer.innerHTML += newMessage;
        chatContainer.scrollTop = chatContainer.scrollHeight;
    });

    connection.start()
        .then(function () {
            console.log("SignalR Connected.");
            connection.invoke("JoinGroup", identifier, isPrivate)
                .catch(function (err) {
                    return console.error(err.toString());
                });
        })
        .catch(function (err) {
            return console.error("SignalR connection error:", err.toString());
        });
}

async function sendMessage(groupId, chatType) {
    const input = document.getElementById('message-input');
    const message = input?.value.trim() || '';
    const fileInput = document.getElementById('file-input');

    // Get file if selected
    let fileUrl = null;
    let fileType = null;
    let fileName = null;
    let audioUrl = null;

    // Process file upload if exists
    if (fileInput && fileInput.files && fileInput.files.length > 0) {
        const file = fileInput.files[0];
        const formData = new FormData();
        formData.append("file", file);

        try {
            const response = await fetch('/GetGroupsAndMessages/UploadFile', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();
            if (result.success) {
                fileUrl = result.fileUrl;
                fileType = result.fileType;
                fileName = result.fileName;
            } else {
                console.error("File upload failed:", result.message);
                alert("File upload failed: " + result.message);
                return;
            }
        } catch (error) {
            console.error("Error uploading file:", error);
            alert("Error uploading file");
            return;
        }
    }

    // Process audio data if exists
    const audioDataInput = document.getElementById('recorded-audio-data');
    if (audioDataInput && audioDataInput.value) {
        try {
            const response = await fetch('/GetGroupsAndMessages/UploadAudio', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    audioData: audioDataInput.value
                })
            });

            const result = await response.json();
            if (result.success) {
                audioUrl = result.audioUrl;
            } else {
                console.error("Audio upload failed:", result.message);
                alert("Audio upload failed: " + result.message);
                return;
            }
        } catch (error) {
            console.error("Error uploading audio:", error);
            alert("Error uploading audio");
            return;
        }
    }

    // Don't send if no message, file, or audio
    if (!message && !fileUrl && !audioUrl) {
        if (!message) {
            alert('Please provide a message, file, or audio recording.');
        }
        return;
    }

    if (!connection) {
        console.error("SignalR connection not established");
        return;
    }

    connection.invoke('SendMessage', chatType, groupId, message, fileUrl, fileType, fileName, audioUrl)
        .then(() => {
            console.log("Message sent successfully");
            // Clear inputs
            if (input) input.value = '';
            if (fileInput) fileInput.value = '';
            if (audioDataInput) audioDataInput.value = '';

            // Hide audio player if visible
            const audioPlayer = document.getElementById('audio-player-container');
            if (audioPlayer) audioPlayer.style.display = 'none';

            // Hide file preview if visible
            const filePreview = document.getElementById('file-preview');
            if (filePreview) {
                filePreview.innerHTML = '';
                filePreview.style.display = 'none';
            }

            // Reset recording state
            isRecording = false;
            audioChunks = [];
            updateRecordingUI();
        })
        .catch(err => {
            console.error('SendMessage error:', err);
            alert("Failed to send message. Please try again.");
        });
    clearFileSelection();
}

function attachSendButtonListener(identifier, chatType) {
    console.log("Attaching send button listener for:", identifier, chatType);

    // Store current chat info
    currentChatId = identifier;
    currentChatType = chatType;

    // Remove any existing event listeners by cloning and replacing elements
    const sendButton = document.getElementById("send-button");
    const messageInput = document.getElementById("message-input");

    if (sendButton) {
        const newSendButton = sendButton.cloneNode(true);
        sendButton.parentNode.replaceChild(newSendButton, sendButton);

        newSendButton.addEventListener('click', async function (e) {
            e.preventDefault();
            console.log("Send button clicked");
            await sendMessage(identifier, chatType);
        });
    }

    if (messageInput) {
        const newMessageInput = messageInput.cloneNode(true);
        messageInput.parentNode.replaceChild(newMessageInput, messageInput);

        newMessageInput.addEventListener('keypress', async function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                console.log("Enter key pressed");
                await sendMessage(identifier, chatType);
            }
        });
    }

    // Add file input change listener to show selected file
    const fileInput = document.getElementById('file-input');
    if (fileInput) {
        const newFileInput = fileInput.cloneNode(true);
        fileInput.parentNode.replaceChild(newFileInput, fileInput);

        newFileInput.addEventListener('change', showSelectedFile);
    }

    // Add recording button listener
    const recordButton = document.getElementById('record-button');
    if (recordButton) {
        const newRecordButton = recordButton.cloneNode(true);
        recordButton.parentNode.replaceChild(newRecordButton, recordButton);

        newRecordButton.addEventListener('click', toggleRecording);
    }
}

function showSelectedFile() {
    const fileInput = document.getElementById('file-input');
    const filePreview = document.getElementById('file-preview');

    if (fileInput && fileInput.files && fileInput.files.length > 0 && filePreview) {
        const file = fileInput.files[0];
        filePreview.innerHTML = `<div class="selected-file">${file.name} <button type="button" onclick="clearFileSelection()" class="clear-file">&times;</button></div>`;
        filePreview.style.display = 'block';
    }
}

function clearFileSelection() {
    const fileInput = document.getElementById('file-input');
    const filePreview = document.getElementById('file-preview');

    if (fileInput) fileInput.value = '';
    if (filePreview) {
        filePreview.innerHTML = '';
        filePreview.style.display = 'none';
    }
}

async function toggleRecording() {
    if (!isRecording) {
        await startRecording();
    } else {
        await stopRecording();
    }
}

async function startRecording() {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });

        mediaRecorder = new MediaRecorder(stream);
        audioChunks = [];

        mediaRecorder.addEventListener('dataavailable', event => {
            if (event.data.size > 0) audioChunks.push(event.data);
        });

        mediaRecorder.addEventListener('stop', () => {
            // Create blob from audio chunks
            const audioBlob = new Blob(audioChunks, { type: 'audio/wav' });
            const audioUrl = URL.createObjectURL(audioBlob);

            // Show audio player
            const audioPlayerContainer = document.getElementById('audio-player-container');
            if (audioPlayerContainer) {
                audioPlayerContainer.innerHTML = `
                    <audio src="${audioUrl}" controls id="audio-preview"></audio>
                    <button type="button" onclick="clearRecording()" class="clear-recording">&times;</button>
                `;
                audioPlayerContainer.style.display = 'block';
            }

            // Store audio data as base64 for sending
            const reader = new FileReader();
            reader.readAsDataURL(audioBlob);
            reader.onloadend = function () {
                const base64data = reader.result;
                const audioDataInput = document.getElementById('recorded-audio-data');
                if (audioDataInput) {
                    audioDataInput.value = base64data;
                }
            };

            // Release microphone
            stream.getTracks().forEach(track => track.stop());
        });

        mediaRecorder.start();
        isRecording = true;
        updateRecordingUI();
    } catch (error) {
        console.error("Error starting recording:", error);
        alert("Could not access microphone. Please check permissions.");
    }
}

async function stopRecording() {
    if (mediaRecorder && mediaRecorder.state !== 'inactive') {
        mediaRecorder.stop();
        isRecording = false;
        updateRecordingUI();
    }
}

function updateRecordingUI() {
    const recordButton = document.getElementById('record-button');
    const recordIcon = document.getElementById('record-icon');
    const stopIcon = document.getElementById('stop-icon');

    if (recordButton && recordIcon && stopIcon) {
        if (isRecording) {
            recordButton.classList.add('recording');
            recordIcon.style.display = 'none';
            stopIcon.style.display = 'inline';
        } else {
            recordButton.classList.remove('recording');
            recordIcon.style.display = 'inline';
            stopIcon.style.display = 'none';
        }
    }
}

function clearRecording() {
    const audioPlayerContainer = document.getElementById('audio-player-container');
    const audioDataInput = document.getElementById('recorded-audio-data');

    if (audioPlayerContainer) {
        audioPlayerContainer.innerHTML = '';
        audioPlayerContainer.style.display = 'none';
    }

    if (audioDataInput) {
        audioDataInput.value = '';
    }
}


