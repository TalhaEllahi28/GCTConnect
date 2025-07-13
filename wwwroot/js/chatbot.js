
// College data (loaded from JSON file)
let collegeData = {};

// Load college data from JSON file
fetch('/collegeData.json')
    .then(response => response.json())
    .then(data => {
        collegeData = data;
        console.log("College data loaded successfully");
    })
    .catch(error => {
        console.error("Error loading college data:", error);
        // Fallback to simple responses if JSON fails to load
    });

// DOM Elements
const messagesContainer = document.getElementById('messages');
const chatForm = document.getElementById('chat-form');
const userInput = document.getElementById('user-input');
const sendButton = document.getElementById('send-button');

// Add a message to the chat
function addMessage(message, isUser = false) {
    const messageDiv = document.createElement('div');
    messageDiv.className = `message ${isUser ? 'user-message' : 'bot-message'}`;

    const messageContent = document.createElement('div');
    messageContent.className = 'message-content';
    messageContent.innerHTML = `<p>${message}</p>`;

    messageDiv.appendChild(messageContent);
    messagesContainer.appendChild(messageDiv);

    // Scroll to bottom
    messagesContainer.scrollTop = messagesContainer.scrollHeight;
}

// Add typing indicator
function showTypingIndicator() {
    const typingDiv = document.createElement('div');
    typingDiv.className = 'message bot-message typing-indicator';
    typingDiv.id = 'typing-indicator';

    const typingContent = document.createElement('div');
    typingContent.className = 'message-content';
    typingContent.innerHTML = `<div class="typing-dots">
        <span></span>
        <span></span>
        <span></span>
    </div>`;

    typingDiv.appendChild(typingContent);
    messagesContainer.appendChild(typingDiv);

    // Scroll to bottom
    messagesContainer.scrollTop = messagesContainer.scrollHeight;
}

// Remove typing indicator
function removeTypingIndicator() {
    const typingIndicator = document.getElementById('typing-indicator');
    if (typingIndicator) {
        typingIndicator.remove();
    }
}

// Call the API to get a response from Mistral
async function getAIResponse(query) {
    try {
        const response = await fetch(`/api/Chatbot/ask?query=${encodeURIComponent(query)}`);
        if (!response.ok) {
            throw new Error('API request failed');
        }
        const data = await response.json();
        return data.response;
    } catch (error) {
        console.error('Error calling AI service:', error);
        // Fallback to local response
        return generateFallbackResponse(query);
    }
}

// Generate fallback response from local data if API fails
function generateFallbackResponse(query) {
    query = query.toLowerCase();

    if (!collegeData || Object.keys(collegeData).length === 0) {
        return "I apologize, but I'm having trouble accessing the college information right now. Please try again later.";
    }

    // Basic greetings
    if (query.includes("hello") || query.includes("hi") || query.includes("hey")) {
        return `Hello! I'm the chatbot for ${collegeData.basicInfo.name}. How can I help you today?`;
    }

    // College information
    if (query.includes("about") && (query.includes("college") || query.includes("institution"))) {
        return `${collegeData.basicInfo.name} is located in ${collegeData.basicInfo.address}. 
            You can contact the college at ${collegeData.basicInfo.phone} or ${collegeData.basicInfo.email}.`;
    }

    // Principal information
    if (query.includes("principal")) {
        return `The Principal of our college is ${collegeData.principalMessage.name}. ${collegeData.principalMessage.message}`;
    }

    // Programs and courses
    if (query.includes("program") || query.includes("course") || query.includes("degree")) {
        return `Our college offers various academic programs:
            - Intermediate: ${collegeData.academicPrograms.intermediate.join(", ")}
            - Associate Degree Programs: ${collegeData.academicPrograms.associate.join(", ")}
            - Bachelor's Programs: ${collegeData.academicPrograms.bachelors.join(", ")}
            - Short Courses: ${collegeData.academicPrograms.shortCourses}`;
    }

    // Department specific information
    const departments = ["IT", "Botany", "Chemistry", "English"];
    for (const dept of departments) {
        if (query.includes(dept.toLowerCase())) {
            const deptInfo = collegeData.departmentInfo[dept];
            return `The ${dept} department offers courses such as ${deptInfo.courses.join(", ")}. 
                Faculty members include ${deptInfo.faculty.join(", ")}. 
                Department facilities: ${deptInfo.facilities.join(", ")}.`;
        }
    }

    // Admission information
    if (query.includes("admission") || query.includes("apply") || query.includes("application")) {
        return `Admission Process: ${collegeData.admissionInfo.process}
            Requirements: ${collegeData.admissionInfo.requirements.join(", ")}
            Deadlines: Fall Semester - ${collegeData.admissionInfo.deadlines.fallSemester}, 
            Spring Semester - ${collegeData.admissionInfo.deadlines.springSemester}
            For more information, contact ${collegeData.admissionInfo.contact.email} or call ${collegeData.admissionInfo.contact.phone}.`;
    }

    // Events and activities
    if (query.includes("event") || query.includes("activit")) {
        return `Recent events at our college include: ${collegeData.recentEvents.join(", ")}.`;
    }

    // Scholarships
    if (query.includes("scholarship") || query.includes("financial aid") || query.includes("fee")) {
        return collegeData.features.find(f => f.title === "Scholarships").description;
    }

    // Faculty information
    if (query.includes("faculty") || query.includes("teacher") || query.includes("professor")) {
        return collegeData.features.find(f => f.title === "Faculty").description;
    }

    // Facilities
    if (query.includes("lab") || query.includes("library") || query.includes("facility")) {
        return collegeData.features.find(f => f.title === "Equipped Labs").description;
    }

    // Contact information
    if (query.includes("contact") || query.includes("phone") || query.includes("email")) {
        return `You can contact ${collegeData.basicInfo.name} at:
            Phone: ${collegeData.basicInfo.phone}
            Email: ${collegeData.basicInfo.email}
            Address: ${collegeData.basicInfo.address}`;
    }

    return "I apologize, but I don't have specific information about that. Could you please ask something else about our college's programs, admissions, facilities, or faculty?";
}

// Handle form submission
chatForm.addEventListener('submit', async (e) => {
    e.preventDefault();

    const message = userInput.value.trim();
    if (!message) return;

    // Disable input and button while processing
    userInput.disabled = true;
    sendButton.disabled = true;

    // Add user message
    addMessage(message, true);
    userInput.value = '';

    // Show typing indicator
    showTypingIndicator();

    try {
        // Get response from backend
        const response = await getAIResponse(message);

        // Remove typing indicator
        removeTypingIndicator();

        // Add bot response
        addMessage(response);
    } catch (error) {
        console.error('Error:', error);
        removeTypingIndicator();
        addMessage("I'm sorry, I couldn't process your request. Please try again later.");
    } finally {
        // Re-enable input and button
        userInput.disabled = false;
        sendButton.disabled = false;
        userInput.focus();
    }
});
