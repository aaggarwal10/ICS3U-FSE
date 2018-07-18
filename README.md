# WORLD AT WAR - ICS3U-FSE

# Summary
Our final assessment for the course was to make a game. Instead of doing something simple, we went beyond the scope of not just the course, but beyond the expectations of all high school courses. We created a 3D real time strategy (multiplayer) game in Unity with online saving when expected to make a simple 2D game in Python, using the Pygame module.
Through many sleepless nights, we created a World at War.

# How to Install & Play
 - Installing
 1. Download the compressed "Server.exe folder" from this repository and extract it. Run the .exe on a computer in your network.
 2. Download the client from https://drive.google.com/file/d/124FajWiwsLMM_C75LIOyA0Kxz2g1e7JY/view?usp=sharing and extract it. You may run any number of clients on any number of computers. The clients must be run after the server has initiated.
 3. When you have the client open, you will see the main menu and be forced to log in or make an account. 
 4. Once you are in an account, you can toggle the menus. Go to "Play" and create or join a queue.
 5. When there are six people in a queue, it will automatically start the game!
 
 - Playing
 
  The core of the game is simple. Don't go bankrupt! You can build plants to increase your income (which starts at negative $5) on the     crystals. Unfortunately, the world doesn't have many crystals, which is why it is at war over income. No one has figured a way to       build a plant that can protect itself, so it needs to always be protected from the other players! Build buildings, make units, and    
  destroy each others bases!
 
# In-game Controls
 - Camera Movement
 
 You are a floating point in 3D space, yay! To enjoy your time, you can move using the WASD and rotate where you are facing by holding and moving the right mouse button.
 - Building a building
 
 Click on "Builds" then select the item you wish to build. Hover your mouse around to see all the spots you can select and left click to place it when you find the perfect spot.
 
 - Creating a unit
 
 Click on the "Units" and left click on a building to select it. It will show you all the units it can spawn. Keep in mind that units need time to spawn in! (2-10 seconds depending on the selected unit) 
 
 - Moving units
 
 Select all the wanted units by left clicking on them, or drag a rectangle over them to select them. Click anywhere on the map and the selected units will try to get there. Keep in mind that some units cannot cross water/land and will just stand by the shore when they get there. 
  
# What makes this impressive
Although it may not seem like much, this was a very intense project to make. We had not used Unity before and had to quickly adapt. 
This project uses many algorithms to efficently do tasks. 
For example, every unit/building that can shoot needs to check for a target every time the main server loop runs, which can be any other unit/building. There can be thousands of units, but instead of having units^2 iterations, we simulate the map with 10 by 10 squares and fit the units in them when they get spawned or move, according to their x and z components. This allows us to only check the units within nearby squares (according to range) instead of checking every unit. This is similar to how the database uses hashtables instead of a linear search. 

We have posted our source codes on this repository. Please note that the server was mainly one organized code and posted as one file while the client had many scripts in the game hierarchy to do many small/large tasks efficently so it was posted as seperate codes. 
The database code (.js) was not posted for security reasons (We don't want people knowing how to spam us efficently).

# Notes
 - The computer running the server will require WiFi to access the online database
 - You only run the server on one computer. However, this is a 6 player game, so if you wish to fill up the queue, it is best to use 6 computers under the network each with the client installed. If you need a few more clients to get the game started, run more than one client on the same computer. 
 - The reason the client was uploaded to a google drive was because the file was too large to place here
 - After you have extracted the compressed folders, don't move any components to another folder without moving the rest of the extracted items
 - The Unity games only run while focused on. That makes it important that whichever computer runs the server has it focused. (You can't be working on another tab)
