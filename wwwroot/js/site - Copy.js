
// ========== DIFFERENT/CONFLICTING FUNCTIONS BETWEEN THE TWO FILES ==========

// =================================================================
// VERSION 1: Simple file upload (no categorization)
// =================================================================

// Global variables for Version 1
// (No selectedFileType or selectedFile variables)





function showSelectedFile() {
    const fileInput = document.getElementById('file-input');
    const filePreview = document.getElementById('file-preview');

    if (fileInput && fileInput.files && fileInput.files.length > 0 && filePreview) {
        const file = fileInput.files[0];
        filePreview.innerHTML = `<div class="selected-file">${file.name} <button type="button" onclick="clearFileSelection_v1()" class="clear-file">&times;</button></div>`;
        filePreview.style.display = 'block';
    }
}


// =================================================================
// VERSION 2: File upload with categorization (modal selection)
// =================================================================

// Global variables for Version 2
let selectedFileType = null;
let selectedFile = null;








































async function sendMessage_v1(groupId, chatType) {
    const input = document.getElementById('message-input');
    const message = input?.value.trim() || '';
    const fileInput = document.getElementById('file-input');

    // Get file if selected
    let fileUrl = null;
    let fileType = null;
    let fileName = null;
    let audioUrl = null;

    // Process file upload if exists (Simple version)
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
    clearFileSelection_v1();
}

async function sendMessage_v2(groupId, chatType) {
    const input = document.getElementById('message-input');
    const message = input?.value.trim() || '';

    let fileUrl = null;
    let fileType = null;
    let fileName = null;
    let audioUrl = null;

    // Upload file if one is selected with a file type
    if (selectedFile && selectedFileType) {
        const formData = new FormData();
        formData.append("file", selectedFile);
        formData.append("fileType", selectedFileType);

        try {
            const response = await fetch('/GetGroupsAndMessages/UploadFile', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();
            if (result.success) {
                fileUrl = result.fileUrl;
                fileType = selectedFileType;
                fileName = result.fileName;
                console.log("File uploaded successfully:", fileName, "Type:", fileType);
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
            if (input) input.value = '';

            // Clear file selection
            selectedFile = null;
            selectedFileType = null;
            const fileInput = document.getElementById('file-input');
            if (fileInput) fileInput.value = '';

            if (audioDataInput) audioDataInput.value = '';

            const audioPlayer = document.getElementById('audio-player-container');
            if (audioPlayer) audioPlayer.style.display = 'none';

            const filePreview = document.getElementById('file-preview');
            if (filePreview) {
                filePreview.innerHTML = '';
                filePreview.style.display = 'none';
            }

            isRecording = false;
            audioChunks = [];
            updateRecordingUI();
        })
        .catch(err => {
            console.error('SendMessage error:', err);
            alert("Failed to send message. Please try again.");
        });
}
























function attachSendButtonListener_v1(identifier, chatType) {
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
            await sendMessage_v1(identifier, chatType);
        });
    }

    if (messageInput) {
        const newMessageInput = messageInput.cloneNode(true);
        messageInput.parentNode.replaceChild(newMessageInput, messageInput);

        newMessageInput.addEventListener('keypress', async function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                console.log("Enter key pressed");
                await sendMessage_v1(identifier, chatType);
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
function attachSendButtonListener_v2(identifier, chatType) {
    console.log("Attaching send button listener for:", identifier, chatType);

    currentChatId = identifier;
    currentChatType = chatType;

    const sendButton = document.getElementById("send-button");
    const messageInput = document.getElementById("message-input");

    if (sendButton) {
        const newSendButton = sendButton.cloneNode(true);
        sendButton.parentNode.replaceChild(newSendButton, sendButton);

        newSendButton.addEventListener('click', async function (e) {
            e.preventDefault();
            console.log("Send button clicked");
            await sendMessage_v2(identifier, chatType);
        });
    }

    if (messageInput) {
        const newMessageInput = messageInput.cloneNode(true);
        messageInput.parentNode.replaceChild(newMessageInput, messageInput);

        newMessageInput.addEventListener('keypress', async function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                console.log("Enter key pressed");
                await sendMessage_v2(identifier, chatType);
            }
        });
    }

    const recordButton = document.getElementById('record-button');
    if (recordButton) {
        const newRecordButton = recordButton.cloneNode(true);
        recordButton.parentNode.replaceChild(newRecordButton, recordButton);

        newRecordButton.addEventListener('click', toggleRecording);
    }
}

function setupUploadModalHandlers() {
    const fileBtn = document.querySelector('.message-action-btn');
    if (fileBtn) {
        fileBtn.removeAttribute('onclick');
        fileBtn.addEventListener('click', function () {
            const fileInput = document.getElementById('file-input');
            if (fileInput) {
                fileInput.click();
            }
        });
    }

    const fileInput = document.getElementById('file-input');
    if (fileInput) {
        fileInput.addEventListener('change', handleFileSelection);
    }

    document.querySelectorAll('.upload-option').forEach(option => {
        option.addEventListener('click', () => {
            document.querySelectorAll('.upload-option').forEach(opt => opt.classList.remove('selected'));
            option.classList.add('selected');
            selectedFileType = option.getAttribute('data-type');
            document.getElementById('uploadBtn').disabled = false;
        });
    });

    const modal = document.getElementById('uploadModal');
    if (modal) {
        modal.addEventListener('click', e => {
            if (e.target === modal) closeUploadModal();
        });
    }
}

function handleFileSelection() {
    const fileInput = document.getElementById('file-input');
    if (!fileInput.files || !fileInput.files.length) {
        return;
    }

    selectedFile = fileInput.files[0];
    console.log("File selected:", selectedFile.name);

    openUploadModal();
}

function openUploadModal() {
    const modal = document.getElementById('uploadModal');
    modal.style.display = 'block';
    selectedFileType = null;
    document.getElementById('uploadBtn').disabled = true;

    document.querySelectorAll('.upload-option').forEach(el => {
        el.classList.remove('selected');
    });
}

function closeUploadModal() {
    const modal = document.getElementById('uploadModal');
    modal.style.display = 'none';
    selectedFileType = null;

    const fileInput = document.getElementById('file-input');
    if (fileInput) fileInput.value = '';
}

function handleFileUpload() {
    if (!selectedFileType || !selectedFile) {
        alert('Please select a file type');
        return;
    }

    closeUploadModal();

    console.log('File ready to send:', selectedFile.name, 'Type:', selectedFileType);

    const filePreview = document.getElementById('file-preview');
    if (filePreview) {
        filePreview.innerHTML = `
            <div class="file-ready-indicator">
                <span>📎 File ready: ${selectedFile.name} (${selectedFileType})</span>
                <button onclick="clearSelectedFile()" class="clear-file-btn">×</button>
            </div>
        `;
        filePreview.style.display = 'block';
    }
}













function clearFileSelection_v1() {
    const fileInput = document.getElementById('file-input');
    const filePreview = document.getElementById('file-preview');

    if (fileInput) fileInput.value = '';
    if (filePreview) {
        filePreview.innerHTML = '';
        filePreview.style.display = 'none';
    }
}


function clearSelectedFile() {
    selectedFile = null;
    selectedFileType = null;
    const filePreview = document.getElementById('file-preview');
    if (filePreview) {
        filePreview.innerHTML = '';
        filePreview.style.display = 'none';
    }
    const fileInput = document.getElementById('file-input');
    if (fileInput) fileInput.value = '';
}