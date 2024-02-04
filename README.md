# Mayazuc Media Player
WinUI 3 based media player. This is an on-going development. 
## Build and Deploy
There are currently no automated build binaries available. In order to use it you need to built it yourself:
1. Download and install Visual Studio 2022 community edition
2. Install the following workloads:
   1. .net desktop development
   2. universal windows platform development
   3. desktop development with C++
   4. windows 11 SDK (10.0.22000.0)
   5. C++ v143 Universal Windows Platform tools
3. Open the SLN file
4. Build and run the project
## OpenSubtitles integration
At the moment you will need to get your own API key from OpenSubtitles.com in order to be able to access the API. Additionally, you will need an account there (which you will need anyways for the API key). Both are free. Create an environment variable for your user with the ID mayazuc.opensubtitles.apikey and the value of your API key.



