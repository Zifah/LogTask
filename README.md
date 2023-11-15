## General improvements
1. Upgrade project to from **.NET framework 4.6** to **.NET 7**
2. Fix the mismatch between the project and folder name for the `LogComponent`
3. Use appropriate data structure (`ConcurrentQueue`) to hold logs for writing
4. Make naming consistent for fields
5. Move the config value: `LogFolder` to the config file
6. Make re-used string literals into constants
7. Organize project files into folders

## Bugfixes
1. Dispose of File I/O handle after use
2. Prevent LogComponent from crashing calling thread
3. Synchronize the flags: `QuitWithFlush` and `Exit` to ensure expected exit behaviour

## Major Refactoring changes
1. Increase the compactness of methods and classes across the solution
2. Reduced nesting levels in methods across the solution
3. Use dependency injection for log-writing (`ILogWriter`) and time-fetching (`IClock`) functionalities

 ## Unit tests
 As demanded
