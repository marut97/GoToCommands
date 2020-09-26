# **Go To Commands for C++:**

## About:
This extension is meant to ease the navigation of large C++ repositories by providing navigation commands in the context menu of the code editor. As an additional feature, it is also capable of navigating through the code of installed packages and references. This would be helpful in scenarios where the source code of the numerous nuget packages and dlls are also downloaded and stored on the local machine. Additionally, the path to these locations need not be configured in the project settings.

## Current Features:
1. Go to code file from header file
2. Go to header file from code file
3. Go to class test file from header file or code file
4. Go to class header file from class test file

## Upcoming Features
1. Configuration settings to provide paths. Both absolute and relative paths.
2. Tool Window at the bottom of the screen to handle multiple results
3. Go to Implementation from interface
4. Go to Derived Class from Base class
5. Go to base class from derived class

Feature requests always welcome :)

## Current Limitations:
1. If the header file is not in a visual studio project, Relative path between the include and source directory is hardcoded. It will work only if the relative path is "include/../src"
2. Go To test will work only if the test projects have the keyword "test" in their name(case insensitive). All files in the test project are considered as test files.
3. Header and Code file must have the same file name. Case insensitive.
4. Class test must contain the name of the file in which the class is present. (Will be extended in the future to contain the name of the class rather than the name of the file in which the class is present)
5. Currently supports only single class in a file
