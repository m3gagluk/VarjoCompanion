# VarjoCompanion
Because you can't have an app that uses both OpenVR and Varjo APIs.

This tiny application is supposed to be launched from [my fork](https://github.com/m3gagluk/VRCFaceTracking) of the [benaclejames' VRCFaceTracking mod](https://github.com/benaclejames/VRCFaceTracking). It starts a Varjo session and dumps its eye tracking data into a MemoryMappedFile which can then be read by the plugin.
