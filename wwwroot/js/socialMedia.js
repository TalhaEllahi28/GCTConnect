//// Sample data for demonstration
//let posts = [
//    {
//        id: 1,
//        author: 'Mike Chen',
//        department: 'Computer Science, 2024',
//        avatar: 'https://images.pexels.com/photos/1547971/pexels-photo-1547971.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
//        content: 'Just finished my Machine Learning project! The AI can now recognize different types of campus buildings with 95% accuracy. Excited to present this next week! 🤖',
//        media: 'https://images.pexels.com/photos/3861958/pexels-photo-3861958.jpeg?auto=compress&cs=tinysrgb&w=600&h=300&fit=crop',
//        privacy: 'Public',
//        likes: 24,
//        comments: 8,
//        shares: 3,
//        liked: false,
//        timestamp: '2 hours ago'
//    },
//    {
//        id: 2,
//        author: 'Emma Rodriguez',
//        department: 'Psychology, 2025',
//        avatar: 'https://images.pexels.com/photos/1542085/pexels-photo-1542085.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
//        content: 'Study group for Cognitive Psychology tomorrow at 3 PM in the library! We\'ll be covering chapters 7-9. Coffee and snacks provided! ☕📚',
//        privacy: 'Department',
//        likes: 15,
//        comments: 12,
//        shares: 6,
//        liked: true,
//        timestamp: '4 hours ago'
//    },
//    {
//        id: 3,
//        author: 'Alex Thompson',
//        department: 'Engineering, 2023',
//        avatar: 'https://images.pexels.com/photos/1040880/pexels-photo-1040880.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
//        content: 'Our robotics team just won the regional competition! Thank you to everyone who supported us. Next stop: nationals! 🏆🤖',
//        media: 'https://images.pexels.com/photos/2085831/pexels-photo-2085831.jpeg?auto=compress&cs=tinysrgb&w=600&h=300&fit=crop',
//        privacy: 'Public',
//        likes: 67,
//        comments: 23,
//        shares: 15,
//        liked: false,
//        timestamp: '1 day ago'
//    }
//];

//let friends = {
//    batch: [
//        {
//            id: 1,
//            name: 'Jessica Park',
//            info: 'Computer Science, 2025',
//            avatar: 'https://images.pexels.com/photos/1065084/pexels-photo-1065084.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
//            status: 'add'
//        },
//        {
//            id: 2,
//            name: 'David Kim',
//            info: 'Computer Science, 2025',
//            avatar: 'https://images.pexels.com/photos/1222271/pexels-photo-1222271.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
//            status: 'pending'
//        },
//        {
//            id: 3,
//            name: 'Rachel Green',
//            info: 'Computer Science, 2025',
//            avatar: 'https://images.pexels.com/photos/1239291/pexels-photo-1239291.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
//            status: 'request'
//        }
//    ],
//    department: [
//        {
//            id: 4,
//            name: 'Kevin Wu',
//            info: 'Computer Science, 2024',
//            avatar: 'https://images.pexels.com/photos/1043471/pexels-photo-1043471.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
//            status: 'add'
//        },
//        {
//            id: 5,
//            name: 'Lisa Zhang',
//            info: 'Computer Science, 2026',
//            avatar: 'https://images.pexels.com/photos/1181686/pexels-photo-1181686.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
//            status: 'add'
//        },
//        {
//            id: 6,
//            name: 'Mark Johnson',
//            info: 'Computer Science, 2023',
//            avatar: 'https://images.pexels.com/photos/1484794/pexels-photo-1484794.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
//            status: 'pending'
//        }
//    ],
//    interests: [
//        {
//            id: 7,
//            name: 'Sophie Miller',
//            info: 'Art Design • AI Enthusiast',
//            avatar: 'https://images.pexels.com/photos/1130626/pexels-photo-1130626.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
//            status: 'add'
//        },
//        {
//            id: 8,
//            name: 'Ryan Cooper',
//            info: 'Business • Tech Startup',
//            avatar: 'https://images.pexels.com/photos/1212984/pexels-photo-1212984.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
//            status: 'request'
//        },
//        {
//            id: 9,
//            name: 'Maya Patel',
//            info: 'Mathematics • ML Research',
//            avatar: 'https://images.pexels.com/photos/1844012/pexels-photo-1844012.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
//            status: 'add'
//        }
//    ]
//};

//let comments = {
//    1: [
//        {
//            author: 'Anna Davis',
//            avatar: 'https://images.pexels.com/photos/1181424/pexels-photo-1181424.jpeg?auto=compress&cs=tinysrgb&w=70&h=70&fit=crop',
//            text: 'This is amazing! Can you share the code on GitHub?'
//        },
//        {
//            author: 'Tom Wilson',
//            avatar: 'https://images.pexels.com/photos/1043474/pexels-photo-1043474.jpeg?auto=compress&cs=tinysrgb&w=70&h=70&fit=crop',
//            text: 'Great work Mike! Looking forward to your presentation.'
//        },
//        {
//            author: 'Linda Brown',
//            avatar: 'https://images.pexels.com/photos/1587009/pexels-photo-1587009.jpeg?auto=compress&cs=tinysrgb&w=70&h=70&fit=crop',
//            text: 'Would love to collaborate on similar projects!'
//        }
//    ],
//    2: [
//        {
//            author: 'James Lee',
//            avatar: 'https://images.pexels.com/photos/1300402/pexels-photo-1300402.jpeg?auto=compress&cs=tinysrgb&w=70&h=70&fit=crop',
//            text: 'Count me in! What should I bring?'
//        },
//        {
//            author: 'Maria Garcia',
//            avatar: 'https://images.pexels.com/photos/1212984/pexels-photo-1212984.jpeg?auto=compress&cs=tinysrgb&w=70&h=70&fit=crop',
//            text: 'Perfect timing! I need help with chapter 8.'
//        }
//    ],
//    3: [
//        {
//            author: 'Chris Evans',
//            avatar: 'https://images.pexels.com/photos/1547971/pexels-photo-1547971.jpeg?auto=compress&cs=tinysrgb&w=70&h=70&fit=crop',
//            text: 'Congratulations! You guys deserved this win! 🎉'
//        },
//        {
//            author: 'Nicole Smith',
//            avatar: 'https://images.pexels.com/photos/1065084/pexels-photo-1065084.jpeg?auto=compress&cs=tinysrgb&w=70&h=70&fit=crop',
//            text: 'So proud of our robotics team! Good luck at nationals!'
//        }
//    ]
//};

//// Initialize the app
//document.addEventListener('DOMContentLoaded', function () {
//    renderPosts();
//    renderFriends('batch');
//    initializeEventListeners();
//});

//function initializeEventListeners() {
//    // Filter buttons for suggested friends
//    document.querySelectorAll('.filter-btn').forEach(btn => {
//        btn.addEventListener('click', function () {
//            document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
//            this.classList.add('active');
//            renderFriends(this.dataset.filter);
//        });
//    });

//    // Create post button
//    document.getElementById('createPostBtn').addEventListener('click', createPost);

//    // Search functionality
//    document.getElementById('searchInput').addEventListener('input', handleSearch);
//}

//function renderPosts() {
//    const container = document.getElementById('postsContainer');
//    container.innerHTML = posts.map(post => `
//        <div class="post-card">
//            <div class="post-header">
//                <img src="${post.avatar}" alt="${post.author}" class="post-avatar">
//                <div class="post-user-info">
//                    <h4>${post.author}</h4>
//                    <p>${post.department} • ${post.timestamp}</p>
//                </div>
//                <div class="post-privacy">${getPrivacyIcon(post.privacy)} ${post.privacy}</div>
//            </div>
//            <div class="post-content">${post.content}</div>
//            ${post.media ? `<div class="post-media"><img src="${post.media}" alt="Post media"></div>` : ''}
//            <div class="post-stats">
//                <span>${post.likes} likes</span>
//                <span>${post.comments} comments • ${post.shares} shares</span>
//            </div>
//            <div class="post-actions">
//                <button class="post-action-btn ${post.liked ? 'liked' : ''}" onclick="toggleLike(${post.id})">
//                    ${post.liked ? '❤️' : '🤍'} Like
//                </button>
//                <button class="post-action-btn" onclick="toggleComments(${post.id})">
//                    💬 Comment
//                </button>
//                <button class="post-action-btn">
//                    📤 Share
//                </button>
//            </div>
//            <div class="comments-section" id="comments-${post.id}">
//                ${renderComments(post.id)}
//            </div>
//        </div>
//    `).join('');
//}

//function renderComments(postId) {
//    const postComments = comments[postId] || [];
//    const visibleComments = postComments.slice(0, 2);
//    const remainingCount = postComments.length - 2;

//    let html = visibleComments.map(comment => `
//        <div class="comment-item">
//            <img src="${comment.avatar}" alt="${comment.author}" class="comment-avatar">
//            <div class="comment-content">
//                <div class="comment-author">${comment.author}</div>
//                <div class="comment-text">${comment.text}</div>
//            </div>
//        </div>
//    `).join('');

//    if (remainingCount > 0) {
//        html += `<div class="view-more-comments" onclick="showAllComments(${postId})">View ${remainingCount} more comments...</div>`;
//    }

//    return html;
//}

//function renderFriends(filter) {
//    const container = document.getElementById('friendsList');
//    const friendsList = friends[filter] || [];

//    container.innerHTML = friendsList.map(friend => `
//        <div class="friend-item">
//            <img src="${friend.avatar}" alt="${friend.name}" class="friend-avatar">
//            <div class="friend-info">
//                <h4>${friend.name}</h4>
//                <p>${friend.info}</p>
//            </div>
//            <div class="friend-actions">
//                ${renderFriendButton(friend)}
//            </div>
//        </div>
//    `).join('');
//}

//function renderFriendButton(friend) {
//    switch (friend.status) {
//        case 'add':
//            return `<button class="friend-btn add" onclick="sendFriendRequest(${friend.id}, '${friend.name}')">Add</button>`;
//        case 'pending':
//            return `<button class="friend-btn pending" onclick="cancelFriendRequest(${friend.id})">Pending</button>`;
//        case 'request':
//            return `
//                <button class="friend-btn accept" onclick="acceptFriendRequest(${friend.id}, '${friend.name}')">Accept</button>
//                <button class="friend-btn reject" onclick="rejectFriendRequest(${friend.id}, '${friend.name}')">Reject</button>
//            `;
//        default:
//            return '';
//    }
//}

//function getPrivacyIcon(privacy) {
//    switch (privacy) {
//        case 'Public': return '🌍';
//        case 'Department': return '🏛️';
//        case 'Batch': return '👥';
//        default: return '🌍';
//    }
//}

//function toggleLike(postId) {
//    const post = posts.find(p => p.id === postId);
//    if (post) {
//        post.liked = !post.liked;
//        post.likes += post.liked ? 1 : -1;
//        renderPosts();
//    }
//}

//function toggleComments(postId) {
//    const commentsSection = document.getElementById(`comments-${postId}`);
//    if (commentsSection) {
//        commentsSection.style.display = commentsSection.style.display === 'none' ? 'block' : 'none';
//    }
//}

//function showAllComments(postId) {
//    const postComments = comments[postId] || [];
//    const container = document.getElementById(`comments-${postId}`);

//    container.innerHTML = postComments.map(comment => `
//        <div class="comment-item">
//            <img src="${comment.avatar}" alt="${comment.author}" class="comment-avatar">
//            <div class="comment-content">
//                <div class="comment-author">${comment.author}</div>
//                <div class="comment-text">${comment.text}</div>
//            </div>
//        </div>
//    `).join('');
//}

//function createPost() {
//    const textarea = document.getElementById('postText');
//    const privacy = document.getElementById('privacySelect');

//    if (textarea.value.trim()) {
//        const newPost = {
//            id: posts.length + 1,
//            author: 'Sarah Johnson',
//            department: 'Computer Science, 2025',
//            avatar: 'https://images.pexels.com/photos/1438081/pexels-photo-1438081.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
//            content: textarea.value,
//            privacy: privacy.options[privacy.selectedIndex].text.split(' ').slice(1).join(' '),
//            likes: 0,
//            comments: 0,
//            shares: 0,
//            liked: false,
//            timestamp: 'Just now'
//        };

//        posts.unshift(newPost);
//        renderPosts();
//        textarea.value = '';

//        // Show success animation
//        showNotification('Post created successfully!', 'success');
//    }
//}

//function sendFriendRequest(friendId, friendName) {
//    // Update friend status to pending
//    updateFriendStatus(friendId, 'pending');
//    showNotification(`Friend request sent to ${friendName}`, 'success');
//}

//function acceptFriendRequest(friendId, friendName) {
//    // Remove from friends list (would be added to actual friends)
//    removeFriendFromList(friendId);
//    showNotification(`You are now friends with ${friendName}`, 'success');
//}

//function rejectFriendRequest(friendId, friendName) {
//    // Remove from friends list
//    removeFriendFromList(friendId);
//    showNotification(`Friend request from ${friendName} rejected`, 'info');
//}

//function cancelFriendRequest(friendId) {
//    updateFriendStatus(friendId, 'add');
//    showNotification('Friend request cancelled', 'info');
//}

//function updateFriendStatus(friendId, newStatus) {
//    // Update in all categories
//    Object.keys(friends).forEach(category => {
//        const friend = friends[category].find(f => f.id === friendId);
//        if (friend) {
//            friend.status = newStatus;
//        }
//    });

//    // Re-render current view
//    const activeFilter = document.querySelector('.filter-btn.active').dataset.filter;
//    renderFriends(activeFilter);
//}

//function removeFriendFromList(friendId) {
//    // Remove from all categories
//    Object.keys(friends).forEach(category => {
//        friends[category] = friends[category].filter(f => f.id !== friendId);
//    });

//    // Re-render current view
//    const activeFilter = document.querySelector('.filter-btn.active').dataset.filter;
//    renderFriends(activeFilter);
//}

//function handleSearch(event) {
//    const searchTerm = event.target.value.toLowerCase();
//    // This would typically filter posts, users, etc.
//    // For demo purposes, we'll just show a visual indication
//    if (searchTerm.length > 2) {
//        console.log(`Searching for: ${searchTerm}`);
//        // In a real app, this would trigger API calls or filter existing data
//    }
//}

//function showNotification(message, type = 'info') {
//    // Create notification element
//    const notification = document.createElement('div');
//    notification.style.cssText = `
//        position: fixed;
//        top: 80px;
//        right: 20px;
//        background: ${type === 'success' ? '#4caf50' : type === 'error' ? '#f44336' : '#7c4dff'};
//        color: white;
//        padding: 15px 20px;
//        border-radius: 8px;
//        z-index: 10000;
//        transform: translateX(100%);
//        transition: transform 0.3s ease;
//        box-shadow: 0 4px 20px rgba(0,0,0,0.1);
//    `;
//    notification.textContent = message;
//    document.body.appendChild(notification);

//    // Animate in
//    setTimeout(() => {
//        notification.style.transform = 'translateX(0)';
//    }, 100);

//    // Remove after 3 seconds
//    setTimeout(() => {
//        notification.style.transform = 'translateX(100%)';
//        setTimeout(() => {
//            document.body.removeChild(notification);
//        }, 300);
//    }, 3000);
//}

//// Add smooth scrolling to top when clicking on trending items
//document.addEventListener('click', function (e) {
//    if (e.target.closest('.trending-item')) {
//        showNotification('Feature coming soon!', 'info');
//    }
//});

//// Add keyboard shortcuts
//document.addEventListener('keydown', function (e) {
//    // Ctrl/Cmd + Enter to post
//    if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
//        const textarea = document.getElementById('postText');
//        if (textarea === document.activeElement && textarea.value.trim()) {
//            createPost();
//        }
//    }

//    // Escape to clear search
//    if (e.key === 'Escape') {
//        const searchInput = document.getElementById('searchInput');
//        if (searchInput === document.activeElement) {
//            searchInput.value = '';
//        }
//    }
//});

//// Add loading states for friend actions
//function addLoadingState(button) {
//    const originalText = button.textContent;
//    button.textContent = '...';
//    button.disabled = true;

//    setTimeout(() => {
//        button.textContent = originalText;
//        button.disabled = false;
//    }, 500);
//}

//// Enhance friend buttons with loading states
//document.addEventListener('click', function (e) {
//    if (e.target.classList.contains('friend-btn')) {
//        addLoadingState(e.target);
//    }
//});



class SocialMediaApp {
    constructor() {
        this.currentUser = null;
        this.posts = [];
        this.suggestedFriends = [];
        this.currentFilter = 'batch';
        this.currentPage = 1;
        this.loading = false;

        this.init();
    }

    async init() {
        try {
            // Load initial data
            await this.loadUserProfile();
            await this.loadSuggestedFriends();
            await this.loadFeed();
            await this.loadTrending();

            this.setupEventListeners();
            this.setupInfiniteScroll();
        } catch (error) {
            console.error('Error initializing social media app:', error);
        }
    }

    async loadUserProfile() {
        try {
            const response = await fetch('/api/social/profile');
            if (response.ok) {
                this.currentUser = await response.json();
                this.renderUserProfile();
            }
        } catch (error) {
            console.error('Error loading user profile:', error);
        }
    }

    async loadSuggestedFriends(filter = 'batch') {
        try {
            const response = await fetch(`/api/social/suggested-friends?filter=${filter}`);
            if (response.ok) {
                this.suggestedFriends = await response.json();
                this.renderSuggestedFriends();
            }
        } catch (error) {
            console.error('Error loading suggested friends:', error);
        }
    }

    async loadFeed(page = 1) {
        if (this.loading) return;

        this.loading = true;
        try {
            const response = await fetch(`/api/social/feed?page=${page}&pageSize=10`);
            if (response.ok) {
                const newPosts = await response.json();
                if (page === 1) {
                    this.posts = newPosts;
                } else {
                    this.posts.push(...newPosts);
                }
                this.renderPosts();
            }
        } catch (error) {
            console.error('Error loading feed:', error);
        } finally {
            this.loading = false;
        }
    }

    async loadTrending() {
        try {
            const response = await fetch('/api/social/trending');
            if (response.ok) {
                const trending = await response.json();
                this.renderTrending(trending);
            }
        } catch (error) {
            console.error('Error loading trending:', error);
        }
    }

    setupEventListeners() {
        // Search functionality
        const searchInput = document.getElementById('searchInput');
        const searchBtn = document.querySelector('.search-btn');

        if (searchInput) {
            let searchTimeout;
            searchInput.addEventListener('input', (e) => {
                clearTimeout(searchTimeout);
                searchTimeout = setTimeout(() => {
                    if (e.target.value.trim()) {
                        this.performSearch(e.target.value.trim());
                    }
                }, 300);
            });
        }

        if (searchBtn) {
            searchBtn.addEventListener('click', () => {
                const query = searchInput?.value.trim();
                if (query) {
                    this.performSearch(query);
                }
            });
        }

        // Filter buttons for suggested friends
        document.querySelectorAll('.filter-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
                e.target.classList.add('active');
                const filter = e.target.dataset.filter;
                this.currentFilter = filter;
                this.loadSuggestedFriends(filter);
            });
        });

        // Post creation
        const createPostBtn = document.getElementById('createPostBtn');
        if (createPostBtn) {
            createPostBtn.addEventListener('click', () => {
                this.createPost();
            });
        }

        // Enter key for post creation
        const postTextarea = document.getElementById('postText');
        if (postTextarea) {
            postTextarea.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' && e.ctrlKey) {
                    this.createPost();
                }
            });
        }
    }

    setupInfiniteScroll() {
        window.addEventListener('scroll', () => {
            if ((window.innerHeight + window.scrollY) >= document.body.offsetHeight - 1000) {
                if (!this.loading) {
                    this.currentPage++;
                    this.loadFeed(this.currentPage);
                }
            }
        });
    }

    renderUserProfile() {
        if (!this.currentUser) return;

        const profileSection = document.querySelector('.profile-card .profile-info');
        if (profileSection) {
            profileSection.innerHTML = `
                <h3>${this.currentUser.fullName}</h3>
                <p>${this.currentUser.department || 'N/A'}, ${this.currentUser.role}</p>
                <p class="profile-stats">${this.currentUser.friendsCount} friends • ${this.currentUser.postsCount} posts</p>
            `;
        }

        // Update profile picture
        const profileAvatar = document.querySelector('.profile-avatar');
        if (profileAvatar && this.currentUser.profilePic) {
            profileAvatar.src = this.currentUser.profilePic;
        }

        // Update post creation avatar
        const smallAvatar = document.querySelector('.small-avatar');
        if (smallAvatar && this.currentUser.profilePic) {
            smallAvatar.src = this.currentUser.profilePic;
        }

        // Update placeholder text
        const postTextarea = document.getElementById('postText');
        if (postTextarea) {
            postTextarea.placeholder = `What's on your mind, ${this.currentUser.fullName.split(' ')[0]}?`;
        }
    }

    renderSuggestedFriends() {
        const friendsList = document.getElementById('friendsList');
        if (!friendsList) return;

        friendsList.innerHTML = this.suggestedFriends.map(friend => `
            <div class="friend-item">
                <img src="${friend.profilePic || 'https://images.pexels.com/photos/220453/pexels-photo-220453.jpeg?auto=compress&cs=tinysrgb&w=50&h=50&fit=crop'}" 
                     alt="${friend.fullName}" class="friend-avatar">
                <div class="friend-info">
                    <h4>${friend.fullName}</h4>
                    <p>${friend.department}</p>
                    <span class="suggestion-reason">${friend.suggestionReason}</span>
                </div>
                <div class="friend-actions">
                     <button class="friend-btn add" data-user-id="${friend.userId}" 
                        ${friend.isFriendRequestSent ? 'disabled' : ''}>
                    ${friend.isFriendRequestSent ? 'Sent' : '+ Add'}</button>
                </div>
            </div>
        `).join('');

        // Add event listeners for friend requests
        friendsList.querySelectorAll('.add-friend-btn').forEach(btn => {
            if (!btn.disabled) {
                btn.addEventListener('click', (e) => {
                    const userId = parseInt(e.target.dataset.userId);
                    this.sendFriendRequest(userId, e.target);
                });
            }
        });
    }

    renderPosts() {
        const postsContainer = document.getElementById('postsContainer');
        if (!postsContainer) return;

        if (this.currentPage === 1) {
            postsContainer.innerHTML = '';
        }

        const postsHTML = this.posts.slice((this.currentPage - 1) * 10).map(post => `
            <div class="post-card" data-post-id="${post.postId}">
                <div class="post-header">
                    <img src="${post.userProfilePic || 'https://images.pexels.com/photos/220453/pexels-photo-220453.jpeg?auto=compress&cs=tinysrgb&w=50&h=50&fit=crop'}" 
                         alt="${post.userFullName}" class="post-avatar">
                    <div class="post-user-info">
                        <h4>${post.userFullName}</h4>
                        <p>${post.userDepartment || ''} ${post.userBatch ? '• ' + post.userBatch : ''} • ${post.timeAgo}</p>
                    </div>
                    <div class="post-privacy">
                        ${this.getPrivacyIcon(post.privacy)}
                    </div>
                </div>
                <div class="post-content">
                    ${post.content}
                </div>

                    ${post.mediaUrl ? `
                        <div class="post-media">
                            ${post.mediaType?.includes('image') ?
                    `<img src="${post.mediaUrl}" alt="Post media">` :
                    `<video src="${post.mediaUrl}" controls style="width:100%"> </video>`
                             }
                        </div>
                    ` : ''
                    }

            <div class="post-stats">
                <span>${post.likesCount} Likes</span>
                <span>${post.commentsCount} Comments</span>
            </div>


                <div class="post-actions">
                    <button class="post-action-btn like-btn ${post.isLiked ? 'liked' : ''}" 
                            data-post-id="${post.postId}">
                        <span class="action-icon">${post.isLiked ? '❤️' : '🤍'}</span>
                    </button>
                    <button class="post-action-btn comment-btn">
                        <span class="action-icon">💬</span>                        
                    </button>
                    <button class="post-action-btn share-btn">
                        <span class="action-icon">📤</span>
                        <span class="action-text">Share</span>
                    </button>
                </div>
            </div>
        `).join('');

        postsContainer.insertAdjacentHTML('beforeend', postsHTML);

        // Add event listeners for post actions
        this.setupPostActions();
    }

    setupPostActions() {
        document.querySelectorAll('.like-btn').forEach(btn => {
            btn.addEventListener('click', async (e) => {
                const postId = parseInt(e.currentTarget.dataset.postId);
                const isLiked = e.currentTarget.classList.contains('liked');

                try {
                    const response = await fetch(`/api/social/posts/${postId}/like`, {
                        method: isLiked ? 'DELETE' : 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        }
                    });

                    if (response.ok) {
                        const result = await response.json();
                        if (result.success) {
                            this.updatePostLike(postId, !isLiked);
                        }
                    }
                } catch (error) {
                    console.error('Error toggling like:', error);
                }
            });
        });
    }

    updatePostLike(postId, isLiked) {
        const postIndex = this.posts.findIndex(p => p.postId === postId);
        if (postIndex !== -1) {
            this.posts[postIndex].isLiked = isLiked;
            this.posts[postIndex].likesCount += isLiked ? 1 : -1;

            const likeBtn = document.querySelector(`.like-btn[data-post-id="${postId}"]`);
            if (likeBtn) {
                likeBtn.classList.toggle('liked', isLiked);
                const icon = likeBtn.querySelector('.action-icon');
                const text = likeBtn.querySelector('.action-text');

                if (icon) icon.textContent = isLiked ? '❤️' : '🤍';
                if (text) text.textContent = `${this.posts[postIndex].likesCount} Likes`;
            }
        }
    }

    renderTrending(trending) {
        const trendingCard = document.querySelector('.trending-card');
        if (!trendingCard || !trending.length) return;

        const trendingContent = trending.map(item => `
            <div class="trending-item">
                <div class="trending-icon">${item.icon}</div>
                <div class="trending-content">
                    <h4>${item.title}</h4>
                    <p>${item.content.substring(0, 50)}${item.content.length > 50 ? '...' : ''}</p>
                </div>
            </div>
        `).join('');

        const existingContent = trendingCard.innerHTML;
        const headerMatch = existingContent.match(/<h3>.*?<\/h3>/);
        const header = headerMatch ? headerMatch[0] : '<h3>Trending on Campus</h3>';

        trendingCard.innerHTML = header + trendingContent;
    }

    async createPost() {
        const postText = document.getElementById('postText');
        const privacySelect = document.getElementById('privacySelect');

        if (!postText || !postText.value.trim()) {
            alert('Please enter some content for your post.');
            return;
        }

        const postData = {
            content: postText.value.trim(),
            privacy: privacySelect?.value || 'public'
        };

        try {
            const response = await fetch('/api/social/posts', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(postData)
            });

            if (response.ok) {
                const newPost = await response.json();
                this.posts.unshift(newPost);
                this.renderPosts();

                // Clear the form
                postText.value = '';

                // Update posts count
                if (this.currentUser) {
                    this.currentUser.postsCount++;
                    this.renderUserProfile();
                }
            } else {
                alert('Error creating post. Please try again.');
            }
        } catch (error) {
            console.error('Error creating post:', error);
            alert('Error creating post. Please try again.');
        }
    }

    async sendFriendRequest(userId, button) {
        try {
            const response = await fetch('/api/social/friend-request', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(userId)
            });

            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    button.textContent = 'Sent';
                    button.disabled = true;

                    // Update the friend object
                    const friend = this.suggestedFriends.find(f => f.userId === userId);
                    if (friend) {
                        friend.isFriendRequestSent = true;
                    }
                }
            } else {
                alert('Error sending friend request.');
            }
        } catch (error) {
            console.error('Error sending friend request:', error);
            alert('Error sending friend request.');
        }
    }

    async performSearch(query) {
        try {
            const response = await fetch(`/api/social/search?q=${encodeURIComponent(query)}`);
            if (response.ok) {
                const results = await response.json();
                this.displaySearchResults(results);
            }
        } catch (error) {
            console.error('Error performing search:', error);
        }
    }

    displaySearchResults(results) {
        // Create a search results modal or update a search results container
        console.log('Search results:', results);
        // For now, just log the results. You can implement a search results UI later.
    }

    getPrivacyIcon(privacy) {
        switch (privacy) {
            case 'public': return '🌍';
            case 'department': return '🏛️';
            case 'batch': return '👥';
            default: return '🌍';
        }
    }
}

// Initialize the social media app when the DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new SocialMediaApp();
});

// Handle window resize for responsive design
window.addEventListener('resize', () => {
    // Add any responsive behavior here if needed
});