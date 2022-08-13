Renaming Pictures taken by a specified Camera Model to a unique Timestamp
# Image-TimeStamp-Renaming

Command line program that renames images (.jpg, .jpeg, .nef) taken by a specified Camera Model. The name takes the form of a timestamp with a id (incase the timestamp is not unique). 

## Usage

Configure the program by modifying the `CAM_MAKE` and `BASE_DIR` variables at the start of `Program.cs` and then run the program

E.g.,
```c#
private const string CAM_MAKE = "NIKON D3300";
private const string BASE_DIR = @"D:\Pictures\Nikon";
```
