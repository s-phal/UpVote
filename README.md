# UpVote
A vote driven Idea Management application built with ASP.NET MVC
- You can demo the app with provided login credentials:
  - [UpVote demo](http://upvote.samphal.com)

Project was developed using:
- ASP.NET MVC
- Entity Framework
- Bootstrap 5
- [X.PagedList](https://github.com/dncuug/X.PagedList)
- [Humanizer](https://github.com/Humanizr/Humanizer)
- PostgreSQL

Hosting Environment:
- Debian Linux 11
- Apache Web Server that proxies to the Kestrel Server

# Screenshots
![upvoteFront](https://user-images.githubusercontent.com/111925825/210673837-baffb70d-300f-4321-b68c-5590e0ad4d13.jpg)
![upvoteIn](https://user-images.githubusercontent.com/111925825/210673839-8e9c9fc9-72a5-4e65-a2d5-deb40d813f6a.jpg)

# Features
- Auto generated Slug titles for friendly URLs
- Auto generated Display Names during registration
- Extended privileges for Admin role 
  - Content Moderation
  - Spam Management
  - Change Post Status
- Pagination
- Image Uploads using the FileStream class
- Auto generate unique filenames upon upload to prevent file overwrites
- Member Activity Notifications
- Sort, Filter, Search
- Commenting

# Installation
- Have PostgreSQL installed
- Change the database connection string in the appsettings.json file
- Apply Migration during first page load
- Register initial user and assign role as admin
  - You will need to change the UserRole column value from "member" to "admin" in the AspNetUsers Table
  
 # TODO
 - Make initial registered user an admin
 - User details page
