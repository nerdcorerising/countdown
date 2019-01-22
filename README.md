# countdown
Solve a letter scramble from the game show countdown

The only really tricky thing here is the QuickPerm algorithm, documented at http://www.quickperm.org.

Other than that the algorithm is fairly straightforward. I use a custom hashset class to avoid having to allocate a string to check if it is included in the set.