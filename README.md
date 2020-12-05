# **Go To Commands for C++:**

## About:
This extension is meant to ease the navigation of large C++ repositories by providing navigation commands in the context menu of the code editor. As an additional feature, it is also capable of navigating through the code of installed packages and references. This would be helpful in scenarios where the source code of the numerous nuget packages and dlls are also downloaded and stored on the local machine. Additionally, the path to these locations need not be configured in the project settings.

## Current Features:
1. Go To Header / Code - To toggle between header and code files in and outside the VS project
2. Go To Test / Class - To toggle between the test and the code class for any VS project item
3. Go to Implementation from interface
4. Go to Derived Class from Base class
5. Go to base class from derived class

## Upcoming Features
1. Configuration settings to provide paths. Both absolute and relative paths.
2. Tool Window at the bottom of the screen to handle multiple results

Feature requests always welcome :)

## Current Limitations:
1. If the header file is not in a visual studio project, Relative path between the include and source directory is hardcoded. It will work only if the relative path is "include/../src"
2. Go To test will work only if the test projects have the keyword "test" in their name(case insensitive). All files in the test project are considered as test files.
3. Header and Code file must have the same file name. Case insensitive.
4. Class test must contain the name of the file in which the class is present. (Will be extended in the future to contain the name of the class rather than the name of the file in which the class is present)
5. Currently supports only single class in a file
6. Might not work for solutions with deeply nested subprojects and solution folders
7. Multiple classes with the same name under different namespaces may cause issues with Go to implementation, base class and derived class.

## Version Details:
1.6.1&nbsp;&nbsp;- Fixing bugs. 
1.6 - Go To Implementation.<br />
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;- Go To Base Class.<br />
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;- Go To Derived Class.<br />
1.5.1&nbsp;&nbsp;- Fixing extension load time issues.  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;- Fixing Go To Test bug where non header or cpp files could be opened.          
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;- Fixing Go To Test bug where Go To Test was available for non VS project items.  
1.5 - Go To Test/Class  
1.4 - Go to Header/Code  
