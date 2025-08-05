# Hole In The Wall: VR + Kinect
[sites.lafayette.edu/lopezbec](https://sites.lafayette.edu/lopezbec)

---

This gamification application aims to integrate the body tracking of the Kinect with the visual enhancement from the Oculus headset. This is in the medium of a full body movement game called Hole in the Wall where the user must contort their body to match the shapes cut out from the wall moving towards them. It also will help observe whether VR or other game elements improve a persons preformance when it comes to completing such a task.

## References

Stranick, T., and Lopez, C. E.,  “Adaptive Virtual Reality Exergame: Promoting Physical Activity Among Workers” J. Comput. Inf. Sci. Eng., 1,22, (2021). [DOI: 10.1115/1.4053002](https://asmedigitalcollection.asme.org/computingengineering/article/22/3/031002/1127994/Adaptive-Virtual-Reality-Exergame-Promoting)

Stranick, T and Lopez, C. E.,  “Virtual Reality Exergames: Promoting physical health among industry workers”, in ASME Int. Design Engineering Technical Conf. & Computer and Informatics Engineering. Conf., Virtual Conference, August 17-20, 2021 ([see PDF](https://sites.lafayette.edu/lopezbec/files/2021/07/ASME_VR_WMSD_Final.pdf))


Stranick, T, and Lopez, C. E.,  “Leveraging Virtual Reality and Exergames to promote physical activity” in International Conference on Human-Computer Interaction, Virtual Conference, July 24-29, 2021 ISBN: 978-3-030-78644-1. ( [See PDF](https://sites.lafayette.edu/lopezbec/files/2021/09/Thomas-HCI-2021.pdf))

 Lopez, C. E., and Tucker, C. S., “The effects of player type on performance: A gamification case study,” Computers in Human Behavior (2019) 91, 333-345 ([doi:10.1016/j.chb.2018.10.005](https://www.sciencedirect.com/science/article/pii/S0747563218304898?via%3Dihub))
 
Lopez, C. E., and Tucker, C. S., “A quantitative method for evaluating the complexity of implementing and performing game features in physically-interactive gamified applications,” Computers in Human Behavior (2017), 71, 42-58 ([doi: 10.1016/j.chb.2017.01.036](https://www.sciencedirect.com/science/article/pii/S0747563217300481?via%3Dihub) )

## Progress Videos

#### 8) Full Game Demonstration: https://youtu.be/C_AJxUklfSw 

![Oculus](https://github.com/lopezbec/WholeInTheWall_VR/blob/master/8.PNG)

  This is the full demonstration of the Hole in the Wall VR application. The user can interact with the menu fully and play the game immersively. The menu was redesigned for cleanliness and tweaks were made to allow the user to reset their head position if it was misaligned. 
  
#### 7) First UI Improvements and base Voice Recognition: https://www.youtube.com/watch?v=d25_yzMvHbI

![Oculus](https://github.com/lopezbec/WholeInTheWall_VR/blob/master/7.PNG)

  The UI needed to be updated to reflect the current project and make it look a little nicer. The voice recognition system was also put in place to allow users to spell their name by saying letters into the microphone. This will be looked at further in having the game take full names or typing the name out on a keyboard to make it work better and easier.
   
#### 6) Spawning the Oculus headset view inside the Avatar head: https://youtu.be/dvUUkWfhSsc  

![Oculus](https://github.com/lopezbec/WholeInTheWall_VR/blob/master/6.PNG)

  In previous videos using the Kinect and Oculus they were never lined up. In order to fix this, the difference between the starting headset and avatar positioning and rotation was taken. The headset then moved to the corresponding position utilizing that calculation. This allows for a much more immersive experinece when the body in the VR world more closely mimics the one you have in the real world. If it is out of sync it breaks immersion.  

#### 5) Using hand colliders to press buttons: https://youtu.be/ZMX9TsXrXY8  

![Oculus](https://github.com/lopezbec/WholeInTheWall_VR/blob/master/5.PNG)

  In order to make it more immersive and interactable the controllers were taken away and the Kinect motion tracking is all thats needed in order to press buttons and such. Colliders were added to the hands as well as the buttons that allow for triggering when they touch each other. Much like pressing a button in real life the user will now have to reach out to press the button in game rather than pointing at it with the controller. This also allowed for easier tracking of the hands when not holding controllers.


#### 4) First Integration of Kinect with Oculus Hardware: https://youtu.be/BGRf_9yo7wg  

![Oculus](https://github.com/lopezbec/WholeInTheWall_VR/blob/master/4.PNG)

  This was the first time using the Kinect with the Oculus headset. The main issues that needed to be addresed were the headset not always spawning directly in the avatar when the Kinect motion tracking kicked in. This was due to the oculus having its own position tracking and not being possible to place it exactly where it is wanted in the game world.

#### 3) Example of Achievement being displayed after completion: https://youtu.be/bgYfcYglNAo 

![Oculus](https://github.com/lopezbec/WholeInTheWall_VR/blob/master/3.PNG)

  Much like the leaderboard, the achievement system intends to be another game element that effects players performance when playing the game. These achievements will be things like passing through a number of obstacles in a row or only touching the wall slightly when passing through. When the game detects that an achievement has been achieved it will display a notification on the top left. The achievement board to view achievements is still a work in progress in terms of having an indicator as to when you have completed the achievement, however does show all of the achievements the player can obtain.
  
#### 2) Demonstration of Leaderboard after game has finished: https://youtu.be/-POtxZyMrho 

![Oculus](https://github.com/lopezbec/WholeInTheWall_VR/blob/master/2.PNG)

  Looking to add game elements to effect people's perfomance the leaderboard was added. The score the player achieves at the end of the game is taken and put into the list of "previous players" to be shown on the leaderboard. To begin there is a set list of random names on the leaderboard, however if the game continues being played it will populate with real players. The random assigned names helps to not create a direct competitive link between friends and such playing the game, however an anonymous way to possibly motivate yourself to do better and improve.
  
#### 1) First Integration of Oculus Hardware: https://youtu.be/OrrPDqjEcq0  

![Oculus](https://github.com/lopezbec/WholeInTheWall_VR/blob/master/1.PNG)

  The Unity project needed to be set up for VR development rather than having the game being presented on the monitor. The OVRPlayerController was also added so that the headset could have a presence within the game world. This allowed for manipulaton of the headset positioning and controller inputs. This early model pressed the interactables of the game with the hand controller, however later turned out that they are detrimental to the Kinect tracking. Also a system was put in place to give the user a random name as there was not built in keyboard for entering one.
  

  
  

# Hole In The Wall: Reinforcement Learning to personalized obstacles

In this part of the project the goal is to train a reinforcement Learning Agent to automatically generate new obstacles of different levels of complexity. This with the goal to tailor the obstacles based on individual skills level. In order to achieve this, we first need to figure out a way to automatically measure the complexity of obstacles (i.e., how much energy would an average human use to complete it). To automate this process, the goal is to train a Avatar to pass through obstacles. 

In the videos below, you will see the Avatar that was controlled to pass via some obstacles along with the “energy expenditure” of the movement. This “energy expenditure” is calculated based on the movement of the limbs. 


https://youtu.be/8b6lvpn1zUY

https://youtu.be/azZkS3HQiZM

https://youtu.be/BpOi5kvXe70
