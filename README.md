# HeadingGenerator
A (mostly) pointless, over-engineered executable that generates unicode text headers segments that copied to the clipboard  
  
Basic Commands:  
-exit => Close the application   
-help => See the complete list of all commands in the system  
  
Profile Commands:  
-listProfiles => See a listing of the different profiles that are available  
-deleteProfile => Delete the profile with the supplied name  
-saveProfile => Save the current generator settings that have been assigned under the supplied name  
-loadProfile => Load the generator settings that are saved under the supplied name  
-restoreDefaultSettings => Reset the generator settings to their default values  
  
Generator Settings:  
-boundarySequence => The sequence of characters that will be used to construct the outer shape of the header  
-fillSequence => The sequence of characters that will be used to fill in the negative space within the boundary  
-verticalBoundarySize => The number of full lines that will be used when creating the top and bottom boundary bars  
-horizontalBoundarySize => The number of columns that will be used when creating the left and right boundary bars  
-verticalBufferSize => The number of empty lines that will be placed between the display text and the top and bottom boundary bars  
-horizontalBufferSize => The number of columns that will be placed between the display text and the left and right boundary bars  
-sectionBufferSize => The number of empty lines that will be placed between the different sections of display text  
-useFixedWidth => A boolean flag that indicates if a constant display size will be used or the smallest possible for the text to be displayed  
-fixedSize => The fixed size that will be used if -useFixedWidth is set to True  
  
Any lines of text that are entered that don't match any of these commands will be captured and converted into a heading. The text that makes up this heading will be copied to the computers clipboard and can be pasted to the required destination  
  
