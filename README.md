# Task-Management-App

&emsp;This is a University project, in which I have implemented a simulation of a Task Management Platform, using ASP.Net. <br /><br />

## Quick Description <br />

&emsp;This app uses an Identity Authentification System, that accepts users and roles. Here, the roles used are Admin and User. Therefore there is a rule that says that when a user creates a project in this app he becomes the Organizer of that project, meaning that he can create a team that can have assigned that project, and in that team, he can add or remove members and tasks. Note that the teams a user can create must have a project that he is the organizer of and that it's not already assigned to another team. If a user does not have a a project, but he wants to create a team, he will not be allowed to do that, because he must be the organizer of the project that will be assigned to the team first. If the user it's just a member of the team, he can not perform those operations, and the only thing he can do it's to edit the tasks that are assigned to him or leave comments on the tasks that belong to the team that he is in. <br /><br />

&emsp;To make this app easy and efficient to use, I have applied a new rule so that the connected user can only see the projects that belong to him (where he is an organizer) and the projects that he is a member of. With this new rule, I made sure that the registered user will not receive a long list that contains thousands of projects that he can not access, edit or perform any operation onto. <br /><br />

&emsp;The admin can add, edit or remove teams, projects, tasks, comments, members in the teams, and even users. He can change any project organizer and I called that operation 'revoking the rights' of a user. <br /><br />

## Rules of the app <br />
&emsp;&emsp;-> the unregistered users cand only view the Home page of the platform and the authentification and registration forms
&emsp;&emsp;-> the user that creates a project becomes organizer of that project
&emsp;&emsp;-> the organizer can create a team with one of the projects that he created assigned, add members to that new team and create, edit and delete tasks for that team
&emsp;&emsp;-> team-members cand only leave comments to the existent tasks and edit or delete their own comments
&emsp;&emsp;-> team-members can only view the tasks and comments from a team that they are a member of
&emsp;&emsp;-> both the organizer and the team-members of a specific team can change the status of a task (not assigned, not started, in progress, completed)
&emsp;&emsp;-> the admin provides the good functionality of the platform and can perform any kind of operation on any entity

## Some insights <br />
![Screenshot (828)](https://user-images.githubusercontent.com/93484228/221356800-a0a31236-524a-4801-b890-bb964cd9f06f.png)
<br />
&emsp;This picture represents the home page of the app
